using CK_NA.Template.ConsoleApp;
//#if (includeExcel)
using OfficeOpenXml;
//#endif

try
{
//#if (includeExcel)
    ExcelPackage.License.SetNonCommercialOrganization("YourOrganization");
//#endif

    var initializer = new ProgramInitializer();
    initializer.Initialize();

    var process = new ApplicationProcess(initializer);
    await process.ExecuteAsync();
}
catch (Exception ex)
{
    var logger = LogManager.GetCurrentClassLogger();
    logger.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
