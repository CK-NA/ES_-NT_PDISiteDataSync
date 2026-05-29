using PDI_SiteDataSync;

// Set the license context for EPPlus (required for version 5.0+)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Initialize program and configuration
ProgramInitializer? initializer = null;

try
{
	initializer = new ProgramInitializer();
	initializer.Initialize();

	// Create and execute the application process
	var applicationProcess = new ApplicationProcess(initializer);

	await applicationProcess.ExecuteAsync();
}
catch (Exception ex)
{
	if (initializer?.Logger != null)
	{
		initializer.Logger.Fatal(ex, "Application terminated unexpectedly");
		Console.WriteLine($"FATAL ERROR: Application terminated unexpectedly - {ex.Message}");
	}
	else
	{
		Console.WriteLine($"Fatal Error: {ex.Message}");
	}
	throw;
}
finally
{
	initializer?.Logger?.Info("Application shutting down");
	LogManager.Shutdown();
}