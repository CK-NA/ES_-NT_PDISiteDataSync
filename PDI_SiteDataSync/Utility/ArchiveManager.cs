using System.IO.Compression;

using PDI_SiteDataSync.Configuration;

namespace PDI_SiteDataSync.Utility;

public class ArchiveManager : IArchiveManager
{
	private readonly Logger _logger;
	private readonly ILoggerFactory _loggerFactory;
	private readonly ArchiveConfiguration _configuration;

	public ArchiveManager(Logger logger, ILoggerFactory loggerFactory, ArchiveConfiguration configuration)
	{
		_logger = logger;
		_loggerFactory = loggerFactory;
		_configuration = configuration;
	}

	/// <summary>
	/// Archives the processed Excel file to a timestamped zip file and deletes the original.
	/// </summary>
	/// <param name="excelFilePath">Full path to the Excel file to archive</param>
	/// <param name="timestamp">Timestamp string for the archive file name</param>
	/// <returns>Path to the created archive file</returns>
	public string ArchiveDataFile(string excelFilePath, string timestamp)
	{
		try
		{
			_logger.Info("Archiving data file: {ExcelFile}", excelFilePath);

			// Verify file exists before attempting to archive
			if (!File.Exists(excelFilePath))
			{
				_logger.Error("Cannot archive data file - file does not exist: {ExcelFile}", excelFilePath);
				throw new FileNotFoundException($"Excel file not found: {excelFilePath}");
			}

			string dataZipFileName = $"Data_{timestamp}.zip";
			string dataZipPath = Path.Combine(_configuration.ArchiveFolder, dataZipFileName);

			// Retry logic to handle potential file locks from EPPlus
			int retryCount = 0;
			int maxRetries = 3;
			while (retryCount < maxRetries)
			{
				try
				{
					using (var dataZip = ZipFile.Open(dataZipPath, ZipArchiveMode.Create))
					{
						string fileName = Path.GetFileName(excelFilePath);
						dataZip.CreateEntryFromFile(excelFilePath, fileName, System.IO.Compression.CompressionLevel.Optimal);
						_logger.Debug("Added Excel file to data archive: {FileName}", fileName);
					}
					break; // Success, exit retry loop
				}
				catch (IOException ex) when (retryCount < maxRetries - 1)
				{
					retryCount++;
					_logger.Warn("Failed to archive file (attempt {Attempt} of {MaxRetries}): {Error}", retryCount, maxRetries, ex.Message);
					System.Threading.Thread.Sleep(500); // Wait 500ms before retry
				}
			}

			// Delete the original Excel file after archiving
			File.Delete(excelFilePath);
			_logger.Info("Data archive created: {DataArchive}", dataZipPath);
			_logger.Info("Original data file deleted: {ExcelFile}", excelFilePath);

			return dataZipPath;
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Failed to archive data file: {ExcelFile}", excelFilePath);
			throw;
		}
	}

	/// <summary>
	/// Archives all log files from the logs folder to a timestamped zip file and deletes the originals.
	/// </summary>
	/// <param name="timestamp">Timestamp string for the archive file name</param>
	/// <returns>Path to the created log archive file, or null if no logs were found</returns>
	public string? ArchiveLogFiles(string timestamp)
	{
		// Log final messages before shutting down logger
		_logger.Info("Preparing to archive log files");

		string logZipFileName = $"Log_{timestamp}.zip";
		string logZipPath = Path.Combine(_configuration.ArchiveFolder, logZipFileName);

		var logFiles = Directory.GetFiles(_configuration.LogsFolder, "*.log");
		if (logFiles.Length == 0)
		{
			_logger.Info("No log files found to archive");
			return null;
		}

		_logger.Info("Archiving {LogFileCount} log files", logFiles.Length);

		// Shutdown logger to release file handles
		_loggerFactory.ShutdownLogs();

		using (var logZip = ZipFile.Open(logZipPath, ZipArchiveMode.Create))
		{
			foreach (var logFile in logFiles)
			{
				string logFileName = Path.GetFileName(logFile);
				logZip.CreateEntryFromFile(logFile, logFileName, System.IO.Compression.CompressionLevel.Optimal);
			}
		}

		// Delete archived log files
		DeleteArchivedLogFiles(logFiles);

		return logZipPath;
	}

	/// <summary>
	/// Archives both data and log files with the same timestamp.
	/// </summary>
	/// <param name="excelFilePath">Full path to the Excel file to archive</param>
	/// <returns>Tuple containing paths to the data and log archives (log archive may be null)</returns>
	public (string DataArchive, string? LogArchive) ArchiveAll(string excelFilePath)
	{
		_logger.Info("Starting archive process for data and logs");
		Console.WriteLine($"  Archiving data file: {Path.GetFileName(excelFilePath)}");
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

		string dataArchive = ArchiveDataFile(excelFilePath, timestamp);
		Console.WriteLine($"  ✓ Data archive created: {Path.GetFileName(dataArchive)}");
		_logger.Info("Data archival completed successfully");

		Console.WriteLine($"  Archiving log files...");
		string? logArchive = ArchiveLogFiles(timestamp);
		if (logArchive != null)
		{
			Console.WriteLine($"  ✓ Log archive created: {Path.GetFileName(logArchive)}");
			_logger.Info("Log archival completed successfully");
		}
		else
		{
			Console.WriteLine($"  (No log files to archive)");
		}

		return (dataArchive, logArchive);
	}

	/// <summary>
	/// Archives only log files when no data file is present to process.
	/// Used when the program runs but finds no input files.
	/// </summary>
	public void ArchiveLogsOnly()
	{
		_logger.Info("Archiving logs from this run");
		_logger.Info("Processing completed successfully with no input file");

		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		ArchiveLogFiles(timestamp);
	}

	private void DeleteArchivedLogFiles(string[] logFiles)
	{
		foreach (var logFile in logFiles)
		{
			try
			{
				File.Delete(logFile);
				_logger.Debug("Deleted archived log file: {LogFile}", logFile);
			}
			catch (Exception ex)
			{
				_logger.Warn(ex, "Could not delete log file: {LogFile}", logFile);
			}
		}
	}
}
