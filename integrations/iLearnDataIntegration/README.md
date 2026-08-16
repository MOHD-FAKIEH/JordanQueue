# iLearn employee integration

Posts employee records to the iLearn API from **SQL Server**. Process output is written to a **log file**, not to the database.

## Data vs log

| What | Where |
|---|---|
| Employee records | Database view `[IH_MasterData].[HR].[v_iLearn]` |
| Process log | File at `LogPath` (default `logs\ilearn-integration.log`) |
| Last-upload hashes | File at `UploadedEmployeesFile` |

`PostAsync` sends mapped employee JSON from the database. If any mapped field changes, the full record is posted.

When a record changes, the log file lists each mapped field that differs, with the previous and new values:

```
Employee 10001 - DATA CHANGED (2 field(s))
Employee 10001 - FIELD JobTitle: 'HR Officer' -> 'HR Specialist'
Employee 10001 - FIELD DutyStation: 'Amman' -> 'Aqaba'
```

The first successful run after this change stores the last posted values in `UploadedEmployeesFile`. Field-level diffs appear on later runs. Old hash-only tracking files are still read; they are rewritten as JSON after the next successful upload.

Logs are only appended to the log file. Nothing is inserted into a log table.

## Configure

In `App.config`:

- `DbConnection` — SQL Server connection string
- `ApiUrl`, `ApiUsername`, `ApiPassword`
- `LogPath` — log file path
- `UploadedEmployeesFile`, `MaxRetries`

## Run

```bash
dotnet run --project integrations/iLearnDataIntegration/iLearnDataIntegration.csproj
```

`DutyStation` and `Area` are sent as separate fields from the view columns of the same names.
