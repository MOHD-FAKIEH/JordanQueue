using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace iLearnDataIntegration
{
    class Program
    {
        static readonly string[] MappedFields =
        {
            "EmployeeNumber",
            "First_name",
            "Last_name",
            "Email",
            "Department",
            "Division",
            "JobTitle",
            "DutyStation",
            "Gender",
            "EmpGroup",
            "EmpSubGroup",
            "IsActive",
            "EntryOfDuty",
            "Area",
            "Grade"
        };

        static readonly string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
        static readonly string username = ConfigurationManager.AppSettings["ApiUsername"];
        static readonly string password = ConfigurationManager.AppSettings["ApiPassword"];

        static readonly string employeesFile =
            ConfigurationManager.AppSettings["EmployeesFile"];

        static readonly string uploadedEmployeesFile =
            ConfigurationManager.AppSettings["UploadedEmployeesFile"];

        static readonly string logPath =
            ConfigurationManager.AppSettings["LogPath"];

        static readonly int maxRetries =
            int.Parse(ConfigurationManager.AppSettings["MaxRetries"]);

        static async Task Main(string[] args)
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrWhiteSpace(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                Log("===== PROCESS STARTED =====");

                var sourceFile = ResolveEmployeesFile(args);
                Log($"Loading employees from file: {sourceFile}");

                var employees = GetEmployeesFromFile(sourceFile);

                Log($"Loaded {employees.Count} employee record(s) from file");

                var uploadedEmployees = LoadUploadedEmployees();

                var handler = new HttpClientHandler
                {
                    // REMOVE THIS IN PRODUCTION AFTER SSL IS FIXED
                    ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true
                };

                using HttpClient httpClient = new HttpClient(handler);

                httpClient.Timeout = TimeSpan.FromMinutes(5);

                var authToken = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{username}:{password}"));

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);

                foreach (var employee in employees)
                {
                    string empNumber = employee["EmployeeNumber"]?.ToString();

                    if (string.IsNullOrWhiteSpace(empNumber))
                        continue;

                    string json = JsonSerializer.Serialize(employee);

                    string hash = GenerateHash(json);

                    bool needsUpload = false;

                    if (!uploadedEmployees.ContainsKey(empNumber))
                    {
                        Log($"Employee {empNumber} - NEW RECORD");
                        needsUpload = true;
                    }
                    else if (uploadedEmployees[empNumber] != hash)
                    {
                        Log($"Employee {empNumber} - DATA CHANGED");
                        needsUpload = true;
                    }
                    else
                    {
                        Log($"Employee {empNumber} - SKIPPED (NO CHANGES)");
                    }

                    if (!needsUpload)
                        continue;

                    bool success = false;
                    int attempt = 0;

                    while (!success && attempt < maxRetries)
                    {
                        attempt++;

                        try
                        {
                            var content = new StringContent(
                                json,
                                Encoding.UTF8,
                                "application/json");

                            HttpResponseMessage response =
                                await httpClient.PostAsync(apiUrl, content);

                            string responseText =
                                await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                Log($"Employee {empNumber} - SUCCESS");

                                uploadedEmployees[empNumber] = hash;

                                SaveUploadedEmployees(uploadedEmployees);

                                success = true;
                            }
                            else
                            {
                                Log($"Employee {empNumber} - FAIL: " +
                                    $"{response.StatusCode} - {responseText}");
                            }
                        }
                        catch (TaskCanceledException ex)
                        {
                            Log($"Employee {empNumber} - TIMEOUT Attempt {attempt}: {ex.Message}");

                            if (attempt >= maxRetries)
                            {
                                Log($"Employee {empNumber} - FINAL FAIL AFTER RETRIES");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"Employee {empNumber} - EXCEPTION: {ex}");

                            break;
                        }

                        if (!success)
                            await Task.Delay(2000);
                    }
                }

                Log("===== PROCESS COMPLETED =====");
            }
            catch (Exception ex)
            {
                Log($"FATAL ERROR: {ex}");
            }
        }

        static string ResolveEmployeesFile(string[] args)
        {
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                return args[0];

            if (string.IsNullOrWhiteSpace(employeesFile))
                throw new InvalidOperationException(
                    "EmployeesFile is not configured. Set App.config EmployeesFile or pass a file path as the first argument.");

            return employeesFile;
        }

        static List<Dictionary<string, object>> GetEmployeesFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Employees file not found.", path);

            var extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension == ".json")
                return LoadEmployeesFromJson(path);

            if (extension == ".csv")
                return LoadEmployeesFromCsv(path);

            throw new NotSupportedException(
                $"Unsupported employees file type '{extension}'. Use .json or .csv.");
        }

        static List<Dictionary<string, object>> LoadEmployeesFromJson(string path)
        {
            var json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("Employees JSON must be an array of objects.");

            var list = new List<Dictionary<string, object>>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    continue;

                var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var property in element.EnumerateObject())
                    raw[property.Name] = JsonElementToString(property.Value);

                list.Add(MapEmployee(raw));
            }

            return list;
        }

        static List<Dictionary<string, object>> LoadEmployeesFromCsv(string path)
        {
            var lines = File.ReadAllLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (lines.Length < 2)
                throw new InvalidDataException("Employees CSV must include a header row and at least one data row.");

            var headers = ParseCsvLine(lines[0]);
            var list = new List<Dictionary<string, object>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i]);
                var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (int column = 0; column < headers.Count; column++)
                {
                    var header = headers[column];
                    if (string.IsNullOrWhiteSpace(header))
                        continue;

                    raw[header] = column < values.Count ? values[column] : string.Empty;
                }

                list.Add(MapEmployee(raw));
            }

            return list;
        }

        static Dictionary<string, object> MapEmployee(Dictionary<string, string> raw)
        {
            var row = new Dictionary<string, object>();

            foreach (var field in MappedFields)
            {
                if (raw.TryGetValue(field, out var value) && !string.IsNullOrWhiteSpace(value))
                    row[field] = value;
                else
                    row[field] = field == "IsActive" ? "Active" : string.Empty;
            }

            return row;
        }

        static string JsonElementToString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => string.Empty,
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => element.GetRawText(),
                _ => element.GetRawText()
            };
        }

        static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            values.Add(current.ToString().Trim());
            return values;
        }

        static Dictionary<string, string> LoadUploadedEmployees()
        {
            var result = new Dictionary<string, string>();

            if (!File.Exists(uploadedEmployeesFile))
                return result;

            var lines = File.ReadAllLines(uploadedEmployeesFile);

            foreach (var line in lines)
            {
                var parts = line.Split('|');

                if (parts.Length >= 2)
                {
                    result[parts[0]] = parts[1];
                }
            }

            return result;
        }

        static void SaveUploadedEmployees(Dictionary<string, string> employees)
        {
            var lines = employees.Select(x =>
                $"{x.Key}|{x.Value}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            File.WriteAllLines(uploadedEmployeesFile, lines);
        }

        static string GenerateHash(string input)
        {
            using SHA256 sha = SHA256.Create();

            byte[] bytes = sha.ComputeHash(
                Encoding.UTF8.GetBytes(input));

            return Convert.ToHexString(bytes);
        }

        static void Log(string message)
        {
            File.AppendAllText(
                logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
        }
    }
}
