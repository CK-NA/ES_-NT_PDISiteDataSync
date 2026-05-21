# Debug Run Instructions - PDI_SiteDataSync

## What Was Done

### 1. Fixed Configuration Issue
- **File**: `appsettings.json`
- **Issue**: Worksheet name had trailing space: `"Store List "`
- **Fix**: Removed trailing space to match exact worksheet name: `"Store List"`

### 2. Added Debug Verification
- **File**: `ApplicationProcess.cs`
- Added console output to show:
  - Database operation results
  - Archive process start/completion
  - Success indicators (✓)

- **File**: `ArchiveManager.cs`
- Added console output to show:
  - Data file being archived
  - Data archive creation confirmation
  - Log archive creation status

### 3. Created Debug Helper Script
- **File**: `debug-run.ps1`
- Automated debug script that:
  - Reads configuration and shows folder paths
  - Creates required folders if missing
  - Checks for Excel input files
  - Runs the application
  - Verifies archival success by comparing before/after
  - Shows contents of created zip files
  - Displays recent log entries with color-coded output

## How to Run Debug Session

### Option 1: Using the Debug Helper Script (Recommended)
```powershell
.\debug-run.ps1
```

This will:
- Show all configuration settings
- Verify folders exist
- Check for input Excel files
- Run the application
- Automatically verify archival succeeded
- Show log file contents
- List contents of created zip archives

### Option 2: Manual Debug in Visual Studio
1. Place an Excel file with "Store List" worksheet in:
   `C:\applications\Development\NTBU_StoreUpdates\Input`

2. Press **F5** or click **Debug > Start Debugging**

3. Watch console output for:
   ```
   ✓ Database operations completed successfully
     - CK_Reporting: X sites/organizations added
     - Common: Y sites added to SiteXRef

   Starting archival process for: [filename.xlsx]
     Archiving data file: [filename.xlsx]
     ✓ Data archive created: Data_20260521123456.zip
     Archiving log files...
     ✓ Log archive created: Logs_20260521123456.zip
   ✓ Archival completed successfully
   ```

4. After run completes, verify:
   - Input Excel file is **GONE** from Input folder
   - New `Data_YYYYMMDDHHMMSS.zip` exists in Archive folder
   - New `Logs_YYYYMMDDHHMMSS.zip` exists in Archive folder
   - Data zip contains the Excel file

## What to Verify

### Success Criteria ✓
1. **Console shows**: "✓ Archival completed successfully"
2. **Input folder**: Excel file is deleted
3. **Archive folder**: New Data_*.zip created
4. **Archive folder**: New Logs_*.zip created
5. **Data zip**: Contains the original Excel file

### If Archival Fails ✗
Check the log file in `C:\applications\Development\NTBU_StoreUpdates\Logs` for:
- Archive-related ERROR or WARN messages
- File access/permission issues
- IOException messages

## Configuration Reference

**Folders** (from `appsettings.json`):
- Base: `C:\applications\Development\NTBU_StoreUpdates`
- Input: `C:\applications\Development\NTBU_StoreUpdates\Input`
- Logs: `C:\applications\Development\NTBU_StoreUpdates\Logs`
- Archive: `C:\applications\Development\NTBU_StoreUpdates\Archive`

**Excel Requirements**:
- Worksheet: `"Store List"` (exact, case-insensitive)
- Column: `"Store #"`

**Databases**:
- CK_Reporting → SSIS_AddHolidaySitesAndOrganizations
- Common → SSIS_AddSitesToSiteXRef

## Recent Fixes Applied

1. ✓ Case-insensitive worksheet lookup
2. ✓ SQL parameter corrected to `@siteList`
3. ✓ Archive retry logic (3 attempts with 500ms delays)
4. ✓ File existence validation before archival
5. ✓ Explicit delete after successful zip creation
6. ✓ Removed trailing space from worksheet name config

## Next Steps

Run the debug session and verify that:
1. The application completes successfully
2. The input Excel file is archived and removed
3. Both Data and Logs zip archives are created
4. The Data zip contains the Excel file

If any issues occur, the debug-run.ps1 script will highlight them with red warning messages.
