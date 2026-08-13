# iLearn employee integration (file source)

Posts employee records to the iLearn API from a **JSON or CSV file**, not from SQL Server.

## What changed

- `GetEmployeesFromDatabase()` and `DbConnection` were removed.
- Employees are loaded from `EmployeesFile` (or a path passed as the first argument).
- Each changed record is still sent with `HttpClient.PostAsync` as JSON.
- Hash-based skip logic is unchanged: if any mapped field changes, the full record is posted.

## Configure

In `App.config`:

- `ApiUrl`, `ApiUsername`, `ApiPassword`
- `EmployeesFile` — path to `employees.json` or `employees.csv`
- `UploadedEmployeesFile`, `LogPath`, `MaxRetries`

Copy a sample file:

```bash
cp employees.sample.json employees.json
```

## Run

```bash
dotnet run --project integrations/iLearnDataIntegration/iLearnDataIntegration.csproj
```

Or pass a file path:

```bash
dotnet run --project integrations/iLearnDataIntegration/iLearnDataIntegration.csproj -- /path/to/employees.csv
```

## File columns

JSON array objects or CSV headers should use these names:

`EmployeeNumber`, `First_name`, `Last_name`, `Email`, `Department`, `Division`, `JobTitle`, `DutyStation`, `Area`, `Gender`, `EmpGroup`, `EmpSubGroup`, `EntryOfDuty`, `Grade`, `IsActive`

`DutyStation` and `Area` are sent as separate fields. `IsActive` defaults to `Active` when omitted.
