using NLog;

namespace PDI_SiteDataSync.Utility;

/// <summary>
/// Helper class to write messages to both NLog logger and console output.
/// </summary>
public static class LogHelper
{
    /// <summary>
    /// Logs an informational message to both logger and console.
    /// </summary>
    public static void LogInfo(Logger logger, string message)
    {
        logger.Info(message);
        Console.WriteLine(message);
    }

    /// <summary>
    /// Logs an informational message with parameters to both logger and console.
    /// </summary>
    public static void LogInfo(Logger logger, string message, params object[] args)
    {
        logger.Info(message, args);
        Console.WriteLine(string.Format(ConvertLogFormat(message), args));
    }

    /// <summary>
    /// Logs a debug message to both logger and console.
    /// </summary>
    public static void LogDebug(Logger logger, string message)
    {
        logger.Debug(message);
        Console.WriteLine($"[DEBUG] {message}");
    }

    /// <summary>
    /// Logs a debug message with parameters to both logger and console.
    /// </summary>
    public static void LogDebug(Logger logger, string message, params object[] args)
    {
        logger.Debug(message, args);
        Console.WriteLine($"[DEBUG] {string.Format(ConvertLogFormat(message), args)}");
    }

    /// <summary>
    /// Logs an error message to both logger and console.
    /// </summary>
    public static void LogError(Logger logger, string message)
    {
        logger.Error(message);
        Console.WriteLine($"ERROR: {message}");
    }

    /// <summary>
    /// Logs an error message with parameters to both logger and console.
    /// </summary>
    public static void LogError(Logger logger, string message, params object[] args)
    {
        logger.Error(message, args);
        Console.WriteLine($"ERROR: {string.Format(ConvertLogFormat(message), args)}");
    }

    /// <summary>
    /// Logs an error with exception to both logger and console.
    /// </summary>
    public static void LogError(Logger logger, Exception ex, string message, params object[] args)
    {
        logger.Error(ex, message, args);
        Console.WriteLine($"ERROR: {string.Format(ConvertLogFormat(message), args)}");
        Console.WriteLine($"Exception: {ex.Message}");
    }

    /// <summary>
    /// Logs a warning message to both logger and console.
    /// </summary>
    public static void LogWarning(Logger logger, string message)
    {
        logger.Warn(message);
        Console.WriteLine($"WARNING: {message}");
    }

    /// <summary>
    /// Logs a warning message with parameters to both logger and console.
    /// </summary>
    public static void LogWarning(Logger logger, string message, params object[] args)
    {
        logger.Warn(message, args);
        Console.WriteLine($"WARNING: {string.Format(ConvertLogFormat(message), args)}");
    }

    /// <summary>
    /// Logs a warning with exception to both logger and console.
    /// </summary>
    public static void LogWarning(Logger logger, Exception ex, string message, params object[] args)
    {
        logger.Warn(ex, message, args);
        Console.WriteLine($"WARNING: {string.Format(ConvertLogFormat(message), args)}");
        Console.WriteLine($"Exception: {ex.Message}");
    }

    /// <summary>
    /// Logs a fatal error to both logger and console.
    /// </summary>
    public static void LogFatal(Logger logger, Exception ex, string message)
    {
        logger.Fatal(ex, message);
        Console.WriteLine($"FATAL ERROR: {message}");
        Console.WriteLine($"Exception: {ex.Message}");
    }

    /// <summary>
    /// Converts NLog structured logging format ({PropertyName}) to String.Format style ({0}, {1}, etc.)
    /// </summary>
    private static string ConvertLogFormat(string nlogFormat)
    {
        // Simple conversion: replace {Name} with {0}, {1}, etc. based on occurrence order
        var result = nlogFormat;
        int index = 0;

        while (result.Contains('{'))
        {
            int start = result.IndexOf('{');
            int end = result.IndexOf('}', start);

            if (end > start)
            {
                result = result.Substring(0, start) + "{" + index + "}" + result.Substring(end + 1);
                index++;
            }
            else
            {
                break;
            }
        }

        return result;
    }
}
