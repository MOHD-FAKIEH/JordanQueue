using Microsoft.Data.SqlClient;
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
        static readonly JsonSerializerOptions TrackingJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        static readonly string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
        static readonly string username = ConfigurationManager.AppSettings["ApiUsername"];
        static readonly string password = ConfigurationManager.AppSettings["ApiPassword"];

        static readonly string uploadedEmployeesFile =
            ConfigurationManager.AppSettings["UploadedEmployeesFile"];

        static readonly string logPath =
            ConfigurationManager.AppSettings["LogPath"];

        static readonly int maxRetries =
            int.Parse(ConfigurationManager.AppSettings["MaxRetries"]);

        static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        static async Task Main(string[] args)
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrWhiteSpace(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                Log("===== PROCESS STARTED =====");

                var employees = GetEmployeesFromDatabase();

                Log($"Loaded {employees.Count} employee record(s) from database");

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
                    else if (uploadedEmployees[empNumber].Hash != hash)
                    {
                        LogFieldChanges(empNumber, employee, uploadedEmployees[empNumber]);
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

                                uploadedEmployees[empNumber] = new UploadedEmployeeState
                                {
                                    Hash = hash,
                                    UploadedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    Payload = ToPayload(employee)
                                };

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

        static List<Dictionary<string, object>> GetEmployeesFromDatabase()
        {
            var list = new List<Dictionary<string, object>>();

            using var conn = new SqlConnection(connectionString);

            conn.Open();

            string sql = @"SELECT [EmployeeNumber]
      ,[EmployeeFullName]
      ,[First_name]
      ,[Last_name]
      ,[Email]
      ,[Department]
      ,[Division]
      ,[JobTitle]
      ,[Grade]
      ,[DutyStation]
      ,[Area]
      ,[Gender]
      ,[EmpGroup]
      ,[EmpSubGroup]
      ,[EntryOfDuty]
  FROM [IH_MasterData].[HR].[v_iLearn]";

            using var cmd = new SqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>
                {
                    ["EmployeeNumber"] = reader["EmployeeNumber"]?.ToString(),
                    ["First_name"] = reader["First_name"]?.ToString(),
                    ["Last_name"] = reader["Last_name"]?.ToString(),
                    ["Email"] = reader["Email"]?.ToString(),
                    ["Department"] = reader["Department"]?.ToString(),
                    ["Division"] = reader["Division"]?.ToString(),
                    ["JobTitle"] = reader["JobTitle"]?.ToString(),
                    ["DutyStation"] = reader["DutyStation"]?.ToString(),
                    ["Gender"] = reader["Gender"]?.ToString(),
                    ["EmpGroup"] = reader["EmpGroup"]?.ToString(),
                    ["EmpSubGroup"] = reader["EmpSubGroup"]?.ToString(),
                    ["IsActive"] = "Active",
                    ["EntryOfDuty"] = reader["EntryOfDuty"]?.ToString(),
                    ["Area"] = reader["Area"]?.ToString(),
                    ["Grade"] = reader["Grade"]?.ToString()
                };

                list.Add(row);
            }

            return list;
        }

        static void LogFieldChanges(
            string empNumber,
            Dictionary<string, object> current,
            UploadedEmployeeState previous)
        {
            var changes = GetChangedFields(current, previous?.Payload);

            if (changes.Count == 0)
            {
                if (previous?.Payload == null || previous.Payload.Count == 0)
                {
                    Log($"Employee {empNumber} - DATA CHANGED (previous field values not stored yet; they will be stored after this successful upload)");
                }
                else
                {
                    Log($"Employee {empNumber} - DATA CHANGED (hash changed, no mapped field value difference found)");
                }

                return;
            }

            Log($"Employee {empNumber} - DATA CHANGED ({changes.Count} field(s))");

            foreach (var change in changes)
                Log($"Employee {empNumber} - FIELD {change}");
        }

        static List<string> GetChangedFields(
            Dictionary<string, object> current,
            Dictionary<string, string> previous)
        {
            var changes = new List<string>();
            var keys = new SortedSet<string>(current.Keys, StringComparer.Ordinal);

            if (previous != null)
            {
                foreach (var key in previous.Keys)
                    keys.Add(key);
            }

            foreach (var key in keys)
            {
                if (string.Equals(key, "EmployeeNumber", StringComparison.OrdinalIgnoreCase))
                    continue;

                var newValue = current.TryGetValue(key, out var currentValue)
                    ? currentValue?.ToString() ?? string.Empty
                    : string.Empty;

                var oldValue = previous != null && previous.TryGetValue(key, out var previousValue)
                    ? previousValue ?? string.Empty
                    : string.Empty;

                if (previous == null || previous.Count == 0)
                    continue;

                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                {
                    changes.Add($"{key}: '{FormatLogValue(oldValue)}' -> '{FormatLogValue(newValue)}'");
                }
            }

            return changes;
        }

        static string FormatLogValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            const int maxLength = 200;
            var sanitized = value.Replace("\r", " ").Replace("\n", " ");
            return sanitized.Length <= maxLength
                ? sanitized
                : sanitized.Substring(0, maxLength) + "...";
        }

        static Dictionary<string, string> ToPayload(Dictionary<string, object> employee)
        {
            var payload = new Dictionary<string, string>();

            foreach (var pair in employee)
                payload[pair.Key] = pair.Value?.ToString() ?? string.Empty;

            return payload;
        }

        static Dictionary<string, UploadedEmployeeState> LoadUploadedEmployees()
        {
            var result = new Dictionary<string, UploadedEmployeeState>();

            if (!File.Exists(uploadedEmployeesFile))
                return result;

            var content = File.ReadAllText(uploadedEmployeesFile);

            if (string.IsNullOrWhiteSpace(content))
                return result;

            var trimmed = content.TrimStart();

            if (trimmed.StartsWith("{"))
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, UploadedEmployeeState>>(content);
                return parsed ?? result;
            }

            foreach (var line in File.ReadAllLines(uploadedEmployeesFile))
            {
                var parts = line.Split('|');

                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    result[parts[0]] = new UploadedEmployeeState
                    {
                        Hash = parts[1],
                        UploadedAt = parts.Length >= 3 ? parts[2] : string.Empty,
                        Payload = new Dictionary<string, string>()
                    };
                }
            }

            return result;
        }

        static void SaveUploadedEmployees(Dictionary<string, UploadedEmployeeState> employees)
        {
            var json = JsonSerializer.Serialize(employees, TrackingJsonOptions);
            File.WriteAllText(uploadedEmployeesFile, json);
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

    class UploadedEmployeeState
    {
        public string Hash { get; set; } = string.Empty;
        public string UploadedAt { get; set; } = string.Empty;
        public Dictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();
    }
}
