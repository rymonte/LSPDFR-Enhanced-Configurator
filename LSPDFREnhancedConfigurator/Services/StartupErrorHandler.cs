using System;
using System.IO;

namespace LSPDFREnhancedConfigurator.Services
{
    public static class StartupErrorHandler
    {
        public static string GetUserFriendlyMessage(Exception ex, string context = "")
        {
            var baseMessage = string.IsNullOrEmpty(context) ? "" : $"{context}\n\n";

            switch (ex)
            {
                case FileNotFoundException fileNotFound:
                    return baseMessage + "Required file not found:\n" +
                           $"{fileNotFound.FileName ?? "Unknown file"}\n\n" +
                           "Please ensure LSPDFR Enhanced is properly installed in your GTA V directory.";

                case DirectoryNotFoundException dirNotFound:
                    return baseMessage + "Required directory not found:\n" +
                           $"{dirNotFound.Message}\n\n" +
                           "Please ensure LSPDFR Enhanced is properly installed in your GTA V directory.";

                case InvalidDataException invalidData:
                    return baseMessage + "Failed to read configuration file:\n" +
                           $"{invalidData.Message}\n\n" +
                           "The file may be corrupted or malformed. Please verify your LSPDFR Enhanced installation.";

                case System.Xml.XmlException xmlEx:
                    return baseMessage + "Failed to parse XML configuration file:\n" +
                           $"Line {xmlEx.LineNumber}, Position {xmlEx.LinePosition}\n" +
                           $"{xmlEx.Message}\n\n" +
                           "The XML file may be malformed. Please verify your LSPDFR Enhanced installation.";

                case UnauthorizedAccessException accessEx:
                    return baseMessage + "Access denied:\n" +
                           $"{accessEx.Message}\n\n" +
                           "Please ensure you have permission to read files in the GTA V directory, " +
                           "or try running the application as administrator.";

                case InvalidOperationException invalidOp when invalidOp.Message.Contains("SplitterDistance"):
                    return baseMessage + "UI Layout Error:\n" +
                           "Window size is too small to display all controls.\n\n" +
                           "Please maximize the window or increase its size.";

                case InvalidOperationException invalidOp:
                    return baseMessage + "Operation failed:\n" +
                           $"{invalidOp.Message}\n\n" +
                           "This appears to be an application error. Please report this issue.";

                case ArgumentException argEx when argEx.Message.Contains("SplitterDistance"):
                    return baseMessage + "UI Layout Error:\n" +
                           "Window size is too small to display all controls.\n\n" +
                           "Please maximize the window or increase its size.";

                default:
                    // Log the full exception for debugging
                    Logger.Error($"Unhandled exception during startup: {ex.GetType().Name}", ex);

                    return baseMessage + "An unexpected error occurred:\n" +
                           $"{ex.Message}\n\n" +
                           "This appears to be an application error. Please check the log file at:\n" +
                           $"{Logger.GetLogFilePath()}";
            }
        }

        public static string GetDirectorySelectionError(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                return "No directory was selected.\n\n" +
                       "Please select your GTA V root directory to continue.";
            }

            if (!Directory.Exists(selectedPath))
            {
                return $"Directory does not exist:\n{selectedPath}\n\n" +
                       "Please select a valid GTA V root directory.";
            }

            if (!File.Exists(Path.Combine(selectedPath, "GTA5.exe")))
            {
                return $"GTA5.exe not found in:\n{selectedPath}\n\n" +
                       "Please select the GTA V root directory (where GTA5.exe is located).";
            }

            var lspdfr = Path.Combine(selectedPath, "plugins", "LSPDFR");
            if (!Directory.Exists(lspdfr))
            {
                return "LSPDFR not found in the selected directory.\n\n" +
                       "Please ensure LSPDFR is installed in:\n" +
                       $"{lspdfr}\n\n" +
                       "If LSPDFR is not installed, please install it before using this configurator.";
            }

            var lspdfEnhanced = Path.Combine(selectedPath, "plugins", "LSPDFR", "LSPDFR Enhanced");
            if (!Directory.Exists(lspdfEnhanced))
            {
                return "LSPDFR Enhanced not found in the selected directory.\n\n" +
                       "Please ensure LSPDFR Enhanced is installed in:\n" +
                       $"{lspdfEnhanced}\n\n" +
                       "If LSPDFR Enhanced is not installed, please install it before using this configurator.";
            }

            return null; // Valid directory
        }
    }
}
