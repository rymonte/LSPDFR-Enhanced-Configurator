using Avalonia;
using LSPDFREnhancedConfigurator.Services;
using System;

namespace LSPDFREnhancedConfigurator;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Initialize logging first
        Logger.Initialize();
        Logger.Info("Application starting...");

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            Logger.Info("Application closed normally");
        }
        catch (Exception ex)
        {
            Logger.Error("Fatal application error", ex);
            throw;
        }
        finally
        {
            Logger.Close();
        }
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
