using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation.Rules
{
    /// <summary>
    /// Validates that all referenced vehicles, stations, and outfits exist in the game data.
    /// Also ensures each rank has at least one station assigned.
    /// </summary>
    public class ReferenceValidationRule : IValidationRule, ISingleRankValidationRule
    {
        public string RuleId => "REFERENCE_VALIDATION";
        public string RuleName => "Reference Validation";

        public ValidationContext[] ApplicableContexts => new[]
        {
            ValidationContext.Full,
            ValidationContext.Startup,
            ValidationContext.PreGenerate
            // Not RealTime - too expensive to check references on every keystroke
        };

        public void Validate(
            List<RankHierarchy> rankHierarchies,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService)
        {
            // Get available references from data service
            var availableVehicles = dataLoadingService.AllVehicles
                .Select(v => v.Model)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            var availableStations = dataLoadingService.Stations
                .Select(s => s.Name)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            // Outfits use CombinedName format: "OutfitName.VariationName"
            var availableOutfits = dataLoadingService.OutfitVariations
                .Select(o => o.CombinedName)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            foreach (var rank in rankHierarchies)
            {
                // Validate pay bands (child ranks)
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Parent rank with pay bands - only validate references, not station count
                    ValidateRankReferences(
                        rank,
                        availableVehicles,
                        availableStations,
                        availableOutfits,
                        result,
                        skipStationCountCheck: true);

                    // Validate each pay band (these must have stations)
                    foreach (var payBand in rank.PayBands)
                    {
                        ValidateRankReferences(
                            payBand,
                            availableVehicles,
                            availableStations,
                            availableOutfits,
                            result,
                            skipStationCountCheck: false);
                    }
                }
                else
                {
                    // Standalone rank - must have stations
                    ValidateRankReferences(
                        rank,
                        availableVehicles,
                        availableStations,
                        availableOutfits,
                        result,
                        skipStationCountCheck: false);
                }
            }
        }

        public void ValidateSingleRank(
            RankHierarchy rank,
            List<RankHierarchy> allRanks,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService)
        {
            // Get available references
            var availableVehicles = dataLoadingService.AllVehicles
                .Select(v => v.Model)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            var availableStations = dataLoadingService.Stations
                .Select(s => s.Name)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            var availableOutfits = dataLoadingService.OutfitVariations
                .Select(o => o.CombinedName)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            // Validate references
            bool skipStationCountCheck = rank.IsParent && rank.PayBands.Count > 0;
            ValidateRankReferences(
                rank,
                availableVehicles,
                availableStations,
                availableOutfits,
                result,
                skipStationCountCheck);

            // If parent rank, validate pay bands
            if (rank.IsParent && rank.PayBands.Count > 0)
            {
                foreach (var payBand in rank.PayBands)
                {
                    ValidateRankReferences(
                        payBand,
                        availableVehicles,
                        availableStations,
                        availableOutfits,
                        result,
                        skipStationCountCheck: false);
                }
            }
        }

        private void ValidateRankReferences(
            RankHierarchy rank,
            HashSet<string> availableVehicles,
            HashSet<string> availableStations,
            HashSet<string> availableOutfits,
            ValidationResult result,
            bool skipStationCountCheck = false)
        {
            // CRITICAL: Validate that rank has at least one station (skip for parent ranks with pay bands)
            if (!skipStationCountCheck && rank.Stations.Count == 0)
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Station",
                    RankName = rank.Name,
                    RankId = rank.Id,
                    Message = $"Rank '{rank.Name}' must have at least one station assigned.",
                    RuleId = RuleId,
                    SuggestedFix = "Add at least one station to this rank."
                });
            }

            // Validate vehicles
            foreach (var vehicle in rank.Vehicles)
            {
                if (!availableVehicles.Contains(vehicle.Model))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Vehicle",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        ItemName = vehicle.Model,
                        Message = $"Rank '{rank.Name}': Vehicle '{vehicle.Model}' not found in game data.",
                        RuleId = RuleId,
                        SuggestedFix = "Remove this vehicle or verify the model name is correct.",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.Vehicles.Remove(vehicle)
                    });
                }
            }

            // Validate stations
            foreach (var station in rank.Stations)
            {
                if (!availableStations.Contains(station.StationName))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Station",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        ItemName = station.StationName,
                        Message = $"Rank '{rank.Name}': Station '{station.StationName}' not found in game data.",
                        RuleId = RuleId,
                        SuggestedFix = "Remove this station or verify the name is correct.",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.Stations.Remove(station)
                    });
                }
            }

            // Validate outfits
            foreach (var outfit in rank.Outfits)
            {
                if (!availableOutfits.Contains(outfit))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Outfit",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        ItemName = outfit,
                        Message = $"Rank '{rank.Name}': Outfit '{outfit}' not found in game data.",
                        RuleId = RuleId,
                        SuggestedFix = "Remove this outfit or verify the name is correct.",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.Outfits.Remove(outfit)
                    });
                }
            }
        }
    }
}
