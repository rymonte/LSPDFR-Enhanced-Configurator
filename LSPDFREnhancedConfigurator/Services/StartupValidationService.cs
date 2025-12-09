using System.Collections.Generic;
using System.IO;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Validates loaded Ranks.xml file against available game data and validates GTA V directory.
    /// Uses the unified ValidationService for all rank validation.
    /// </summary>
    public class StartupValidationService
    {
        private readonly ValidationService _validationService;

        public StartupValidationService(DataLoadingService dataService)
        {
            _validationService = new ValidationService(dataService);
        }

        /// <summary>
        /// Validates that the GTA V directory is correct and has required files/folders
        /// </summary>
        public static GtaDirectoryValidation ValidateGtaDirectory(string? gtaPath)
        {
            var validation = new GtaDirectoryValidation();

            if (string.IsNullOrWhiteSpace(gtaPath))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Error;
                validation.ErrorMessage = "GTA V directory is not configured.";
                return validation;
            }

            if (!Directory.Exists(gtaPath))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Error;
                validation.ErrorMessage = $"GTA V directory does not exist:\n{gtaPath}";
                return validation;
            }

            // Check for GTA5.exe (CRITICAL - must have)
            var gta5ExePath = Path.Combine(gtaPath, "GTA5.exe");
            if (!File.Exists(gta5ExePath))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Error;
                validation.ErrorMessage = $"GTA5.exe not found in directory:\n{gtaPath}\n\nThis does not appear to be a valid GTA V installation.";
                validation.MissingFiles.Add("GTA5.exe");
                return validation;
            }

            // Check for plugins folder (RAGE Plugin Hook requirement)
            var pluginsPath = Path.Combine(gtaPath, "plugins");
            if (!Directory.Exists(pluginsPath))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins' folder not found in:\n{gtaPath}\n\nRAGE Plugin Hook does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without RAGE Plugin Hook and LSPDFR Enhanced installed.\n\nPlease install RAGE Plugin Hook and LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins");
                return validation;
            }

            // Check for LSPDFR folder
            var lspdfr​Path = Path.Combine(pluginsPath, "LSPDFR");
            if (!Directory.Exists(lspdfr​Path))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins\\LSPDFR' folder not found in:\n{gtaPath}\n\nLSPDFR does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without LSPDFR and LSPDFR Enhanced installed.\n\nPlease install LSPDFR and LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins\\LSPDFR");
                return validation;
            }

            // Check for LSPDFR Enhanced folder
            var lspdfr​EnhancedPath = Path.Combine(lspdfr​Path, "LSPDFR Enhanced");
            if (!Directory.Exists(lspdfr​EnhancedPath))
            {
                validation.IsValid = false;
                validation.Severity = GtaValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins\\LSPDFR\\LSPDFR Enhanced' folder not found in:\n{gtaPath}\n\nLSPDFR Enhanced does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without LSPDFR Enhanced installed.\n\nPlease install LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins\\LSPDFR\\LSPDFR Enhanced");
                return validation;
            }

            // All checks passed
            validation.IsValid = true;
            validation.Severity = GtaValidationSeverity.Success;
            return validation;
        }

        /// <summary>
        /// Validates ranks using the ValidationService.
        /// Returns a ValidationResult with all issues found.
        /// </summary>
        /// <param name="ranks">Rank hierarchies to validate</param>
        /// <returns>Validation result containing errors, warnings, and advisories</returns>
        public ValidationResult ValidateRanks(List<RankHierarchy> ranks)
        {
            return _validationService.ValidateRanks(ranks, ValidationContext.Startup);
        }

        /// <summary>
        /// Provides access to the underlying validation service for advanced scenarios
        /// </summary>
        public ValidationService ValidationService => _validationService;
    }

    /// <summary>
    /// Result of GTA V directory validation
    /// </summary>
    public class GtaDirectoryValidation
    {
        public bool IsValid { get; set; } = true;
        public GtaValidationSeverity Severity { get; set; } = GtaValidationSeverity.Success;
        public string? ErrorMessage { get; set; }
        public List<string> MissingFiles { get; set; } = new List<string>();
        public List<string> MissingFolders { get; set; } = new List<string>();
    }

    /// <summary>
    /// Severity level for GTA V directory validation
    /// NOTE: This is separate from ValidationSeverity which is used for rank validation
    /// </summary>
    public enum GtaValidationSeverity
    {
        Success,
        Warning,
        Error
    }
}
