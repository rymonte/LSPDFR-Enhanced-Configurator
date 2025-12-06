using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Validates loaded Ranks.xml file against available game data
    /// Detects orphaned references and progression issues
    /// </summary>
    public class StartupValidationService
    {
        private readonly DataLoadingService _dataService;

        public StartupValidationService(DataLoadingService dataService)
        {
            _dataService = dataService;
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
                validation.Severity = ValidationSeverity.Error;
                validation.ErrorMessage = "GTA V directory is not configured.";
                return validation;
            }

            if (!Directory.Exists(gtaPath))
            {
                validation.IsValid = false;
                validation.Severity = ValidationSeverity.Error;
                validation.ErrorMessage = $"GTA V directory does not exist:\n{gtaPath}";
                return validation;
            }

            // Check for GTA5.exe (CRITICAL - must have)
            var gta5ExePath = Path.Combine(gtaPath, "GTA5.exe");
            if (!File.Exists(gta5ExePath))
            {
                validation.IsValid = false;
                validation.Severity = ValidationSeverity.Error;
                validation.ErrorMessage = $"GTA5.exe not found in directory:\n{gtaPath}\n\nThis does not appear to be a valid GTA V installation.";
                validation.MissingFiles.Add("GTA5.exe");
                return validation;
            }

            // Check for plugins folder (RAGE Plugin Hook requirement)
            var pluginsPath = Path.Combine(gtaPath, "plugins");
            if (!Directory.Exists(pluginsPath))
            {
                validation.IsValid = false;
                validation.Severity = ValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins' folder not found in:\n{gtaPath}\n\nRAGE Plugin Hook does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without RAGE Plugin Hook and LSPDFR Enhanced installed.\n\nPlease install RAGE Plugin Hook and LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins");
                return validation;
            }

            // Check for LSPDFR folder
            var lspdfr​Path = Path.Combine(pluginsPath, "LSPDFR");
            if (!Directory.Exists(lspdfr​Path))
            {
                validation.IsValid = false;
                validation.Severity = ValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins\\LSPDFR' folder not found in:\n{gtaPath}\n\nLSPDFR does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without LSPDFR and LSPDFR Enhanced installed.\n\nPlease install LSPDFR and LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins\\LSPDFR");
                return validation;
            }

            // Check for LSPDFR Enhanced folder
            var lspdfr​EnhancedPath = Path.Combine(lspdfr​Path, "LSPDFR Enhanced");
            if (!Directory.Exists(lspdfr​EnhancedPath))
            {
                validation.IsValid = false;
                validation.Severity = ValidationSeverity.Warning;
                validation.ErrorMessage = $"'plugins\\LSPDFR\\LSPDFR Enhanced' folder not found in:\n{gtaPath}\n\nLSPDFR Enhanced does not appear to be installed.\n\nThis application is designed to configure LSPDFR Enhanced files.\nThe application will not function without LSPDFR Enhanced installed.\n\nPlease install LSPDFR Enhanced before using this configurator.";
                validation.MissingFolders.Add("plugins\\LSPDFR\\LSPDFR Enhanced");
                return validation;
            }

            // All checks passed
            validation.IsValid = true;
            validation.Severity = ValidationSeverity.Success;
            return validation;
        }

        public ValidationReport ValidateRanks(List<RankHierarchy> ranks)
        {
            var report = new ValidationReport();

            if (ranks.Count == 0)
            {
                return report; // No ranks to validate
            }

            // Validate rank progression
            ValidateRankProgression(ranks, report);

            // Validate references (vehicles, stations, outfits)
            ValidateReferences(ranks, report);

            return report;
        }

        private void ValidateRankProgression(List<RankHierarchy> ranks, ValidationReport report)
        {
            for (int i = 0; i < ranks.Count; i++)
            {
                var rank = ranks[i];

                // CRITICAL: Validate rank name is not empty
                if (string.IsNullOrWhiteSpace(rank.Name))
                {
                    report.AddError($"Rank #{i + 1}: Rank name cannot be empty");
                }

                // CRITICAL: Validate Required Points and Salary values for standalone ranks and pay bands
                if (!rank.IsParent || rank.PayBands.Count == 0)
                {
                    // Standalone rank validations
                    if (rank.RequiredPoints < 0)
                    {
                        report.AddError($"Rank '{rank.Name}': Required Points cannot be negative (current: {rank.RequiredPoints})");
                    }
                    if (rank.Salary < 0)
                    {
                        report.AddError($"Rank '{rank.Name}': Salary cannot be negative (current: {rank.Salary})");
                    }
                }
                else
                {
                    // Parent rank with pay bands - validate each pay band
                    foreach (var payBand in rank.PayBands)
                    {
                        if (string.IsNullOrWhiteSpace(payBand.Name))
                        {
                            report.AddError($"Pay band in rank '{rank.Name}': Pay band name cannot be empty");
                        }
                        if (payBand.RequiredPoints < 0)
                        {
                            report.AddError($"Rank '{payBand.Name}': Required Points cannot be negative (current: {payBand.RequiredPoints})");
                        }
                        if (payBand.Salary < 0)
                        {
                            report.AddError($"Rank '{payBand.Name}': Salary cannot be negative (current: {payBand.Salary})");
                        }
                    }
                }

                // Validate individual rank XP progression
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Validate pay bands progression
                    for (int j = 0; j < rank.PayBands.Count; j++)
                    {
                        var payBand = rank.PayBands[j];

                        // Check progression between pay bands
                        if (j > 0)
                        {
                            var prevPayBand = rank.PayBands[j - 1];
                            if (payBand.RequiredPoints <= prevPayBand.RequiredPoints)
                            {
                                report.AddError($"Rank '{payBand.Name}': Required Points ({payBand.RequiredPoints}) must be greater than previous pay band '{prevPayBand.Name}' ({prevPayBand.RequiredPoints})");
                            }
                        }
                    }

                    // Check minimum pay bands
                    if (rank.PayBands.Count < 2)
                    {
                        report.AddError($"Rank '{rank.Name}': Parent rank must have at least 2 pay bands (has {rank.PayBands.Count})");
                    }
                }

                // Check progression between ranks (only for standalone ranks and first pay bands)
                // Skip this check for parent ranks with pay bands - they derive XP from children
                if (i > 0 && !(rank.IsParent && rank.PayBands.Count > 0))
                {
                    var prevRank = ranks[i - 1];
                    int prevRequiredPoints = prevRank.IsParent && prevRank.PayBands.Count > 0
                        ? prevRank.PayBands.Max(p => p.RequiredPoints)
                        : prevRank.RequiredPoints;

                    int currentRequiredPoints = rank.IsParent && rank.PayBands.Count > 0
                        ? rank.PayBands.Min(p => p.RequiredPoints)
                        : rank.RequiredPoints;

                    if (currentRequiredPoints <= prevRequiredPoints)
                    {
                        report.AddError($"Rank '{rank.Name}': Required Points ({currentRequiredPoints}) must be greater than previous rank '{prevRank.Name}' ({prevRequiredPoints})");
                    }
                }

                // For parent ranks with pay bands, validate that first pay band meets minimum XP
                if (i > 0 && rank.IsParent && rank.PayBands.Count > 0)
                {
                    var prevRank = ranks[i - 1];
                    int prevRequiredPoints = prevRank.IsParent && prevRank.PayBands.Count > 0
                        ? prevRank.PayBands.Max(p => p.RequiredPoints)
                        : prevRank.RequiredPoints;

                    var firstPayBand = rank.PayBands[0];
                    if (firstPayBand.RequiredPoints <= prevRequiredPoints)
                    {
                        report.AddError($"Rank '{firstPayBand.Name}': Required Points ({firstPayBand.RequiredPoints}) must be greater than previous rank '{prevRank.Name}' ({prevRequiredPoints})");
                    }
                }

                // First rank should start at 0
                if (i == 0)
                {
                    int firstRequiredPoints = rank.IsParent && rank.PayBands.Count > 0
                        ? rank.PayBands.Min(p => p.RequiredPoints)
                        : rank.RequiredPoints;

                    if (firstRequiredPoints != 0)
                    {
                        report.AddWarning($"Rank '{rank.Name}': First rank should start at XP 0, not {firstRequiredPoints}");
                    }
                }
            }
        }

        private void ValidateReferences(List<RankHierarchy> ranks, ValidationReport report)
        {
            // Get available references from data service
            var availableVehicles = _dataService.AllVehicles.Select(v => v.Model).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var availableStations = _dataService.Stations.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var availableOutfits = _dataService.OutfitVariations
                .Select(o => o.ParentOutfit.Name)
                .Distinct()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var rank in ranks)
            {
                // Validate pay bands (child ranks)
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Parent rank with pay bands - only validate references, not station count
                    ValidateRankReferences(rank, availableVehicles, availableStations, availableOutfits, report, skipStationCountCheck: true);

                    // Validate each pay band (these must have stations)
                    foreach (var payBand in rank.PayBands)
                    {
                        ValidateRankReferences(payBand, availableVehicles, availableStations, availableOutfits, report, skipStationCountCheck: false);
                    }
                }
                else
                {
                    // Standalone rank - must have stations
                    ValidateRankReferences(rank, availableVehicles, availableStations, availableOutfits, report, skipStationCountCheck: false);
                }
            }
        }

        private void ValidateRankReferences(
            RankHierarchy rank,
            HashSet<string> availableVehicles,
            HashSet<string> availableStations,
            HashSet<string> availableOutfits,
            ValidationReport report,
            bool skipStationCountCheck = false)
        {
            // CRITICAL: Validate that rank has at least one station (skip for parent ranks with pay bands)
            if (!skipStationCountCheck && rank.Stations.Count == 0)
            {
                report.AddError($"Rank '{rank.Name}': At least one station required per rank");
            }

            // Validate vehicles
            foreach (var vehicle in rank.Vehicles)
            {
                if (!availableVehicles.Contains(vehicle.Model))
                {
                    report.AddWarning($"Rank '{rank.Name}': Vehicle '{vehicle.Model}' not found in game data");
                }
            }

            // Validate stations
            foreach (var station in rank.Stations)
            {
                if (!availableStations.Contains(station.StationName))
                {
                    report.AddWarning($"Rank '{rank.Name}': Station '{station.StationName}' not found in game data");
                }
            }

            // Validate outfits
            foreach (var outfit in rank.Outfits)
            {
                if (!availableOutfits.Contains(outfit))
                {
                    report.AddWarning($"Rank '{rank.Name}': Outfit '{outfit}' not found in game data");
                }
            }
        }
    }

    /// <summary>
    /// Report of validation issues found during startup
    /// </summary>
    public class ValidationReport
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool HasIssues => Errors.Count > 0 || Warnings.Count > 0;
        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void AddError(string message)
        {
            Errors.Add(message);
            Logger.Error($"[STARTUP VALIDATION] ERROR: {message}");
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
            Logger.Warn($"[STARTUP VALIDATION] WARNING: {message}");
        }

        public string GetSummary()
        {
            if (!HasIssues)
                return "No issues found.";

            var summary = "";
            if (HasErrors)
            {
                summary += $"{Errors.Count} error(s) found:\n";
                foreach (var error in Errors.Take(10))
                {
                    summary += $"  ❌ {error}\n";
                }
                if (Errors.Count > 10)
                {
                    summary += $"  ... and {Errors.Count - 10} more error(s)\n";
                }
                summary += "\n";
            }

            if (HasWarnings)
            {
                summary += $"{Warnings.Count} warning(s) found:\n";
                foreach (var warning in Warnings.Take(10))
                {
                    summary += $"  ⚠️ {warning}\n";
                }
                if (Warnings.Count > 10)
                {
                    summary += $"  ... and {Warnings.Count - 10} more warning(s)\n";
                }
            }

            return summary.TrimEnd();
        }
    }

    /// <summary>
    /// Result of GTA V directory validation
    /// </summary>
    public class GtaDirectoryValidation
    {
        public bool IsValid { get; set; } = true;
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Success;
        public string? ErrorMessage { get; set; }
        public List<string> MissingFiles { get; set; } = new List<string>();
        public List<string> MissingFolders { get; set; } = new List<string>();
    }

    /// <summary>
    /// Severity level of validation issue
    /// </summary>
    public enum ValidationSeverity
    {
        Success,
        Warning,
        Error
    }
}
