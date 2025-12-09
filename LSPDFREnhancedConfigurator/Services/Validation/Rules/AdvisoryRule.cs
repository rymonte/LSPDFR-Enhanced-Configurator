using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation.Rules
{
    /// <summary>
    /// Provides advisory (non-blocking) suggestions for rank configuration.
    /// Examples: "previous rank had more vehicles", "station count decreased", etc.
    /// These are helpful hints that don't block generation.
    /// </summary>
    public class AdvisoryRule : IValidationRule, ISingleRankValidationRule
    {
        public string RuleId => "ADVISORY_CHECKS";
        public string RuleName => "Advisory Checks";

        public ValidationContext[] ApplicableContexts => new[]
        {
            ValidationContext.Full,
            ValidationContext.Startup,
            ValidationContext.AdvisoryOnly,
            ValidationContext.RealTime
            // Not PreGenerate - advisory checks don't block generation
        };

        public void Validate(
            List<RankHierarchy> rankHierarchies,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService)
        {
            // Flatten all ranks (including pay bands)
            var allRanks = FlattenRanks(rankHierarchies);

            if (allRanks.Count == 0)
            {
                return;
            }

            // Compare ranks (need at least 2 for comparisons)
            if (allRanks.Count >= 2)
            {
                for (int i = 1; i < allRanks.Count; i++)
            {
                var current = allRanks[i];
                var previous = allRanks[i - 1];

                // Check if station count decreased
                if (current.Stations.Count < previous.Stations.Count)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Station",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' has {current.Stations.Count} station(s), " +
                                 $"but previous rank '{previous.Name}' had {previous.Stations.Count} station(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if vehicles were removed
                var previousVehicleModels = previous.Vehicles.Select(v => v.Model).ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var currentVehicleModels = current.Vehicles.Select(v => v.Model).ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var removedVehicles = previousVehicleModels.Except(currentVehicleModels).ToList();

                if (removedVehicles.Count > 0)
                {
                    var vehicleList = string.Join(", ", removedVehicles.Take(3));
                    if (removedVehicles.Count > 3)
                    {
                        vehicleList += $" and {removedVehicles.Count - 3} more";
                    }

                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' is missing {removedVehicles.Count} vehicle(s) " +
                                 $"that were available in '{previous.Name}': {vehicleList}. " +
                                 $"Consider if this is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if total vehicle count decreased
                if (current.Vehicles.Count < previous.Vehicles.Count && removedVehicles.Count == 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' has {current.Vehicles.Count} vehicle(s), " +
                                 $"but previous rank '{previous.Name}' had {previous.Vehicles.Count} vehicle(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if outfits were removed
                var previousOutfits = previous.Outfits.ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var currentOutfits = current.Outfits.ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var removedOutfits = previousOutfits.Except(currentOutfits).ToList();

                if (removedOutfits.Count > 0)
                {
                    var outfitList = string.Join(", ", removedOutfits.Take(3));
                    if (removedOutfits.Count > 3)
                    {
                        outfitList += $" and {removedOutfits.Count - 3} more";
                    }

                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' is missing {removedOutfits.Count} outfit(s) " +
                                 $"that were available in '{previous.Name}': {outfitList}. " +
                                 $"Consider if this is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if total outfit count decreased
                if (current.Outfits.Count < previous.Outfits.Count && removedOutfits.Count == 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' has {current.Outfits.Count} outfit(s), " +
                                 $"but previous rank '{previous.Name}' had {current.Outfits.Count} outfit(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if rank has no vehicles at all (advisory, not error)
                if (!HasAnyVehicles(current))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' has no vehicles assigned (global or station-specific). " +
                                 $"Consider adding at least one vehicle for this rank.",
                        RuleId = RuleId
                    });
                }

                // Check if rank has no outfits at all (advisory, not error)
                if (!HasAnyOutfits(current))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = current.Name,
                        RankId = current.Id,
                        Message = $"Rank '{current.Name}' has no outfits assigned (global or station-specific). " +
                                 $"Consider adding at least one outfit for this rank.",
                        RuleId = RuleId
                    });
                }
            }
            }

            // Check first rank for missing vehicles/outfits
            if (allRanks.Count > 0)
            {
                var firstRank = allRanks[0];

                if (!HasAnyVehicles(firstRank))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = firstRank.Name,
                        RankId = firstRank.Id,
                        Message = $"First rank '{firstRank.Name}' has no vehicles assigned (global or station-specific). " +
                                 $"Consider adding at least one vehicle for this rank.",
                        RuleId = RuleId
                    });
                }

                if (!HasAnyOutfits(firstRank))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = firstRank.Name,
                        RankId = firstRank.Id,
                        Message = $"First rank '{firstRank.Name}' has no outfits assigned (global or station-specific). " +
                                 $"Consider adding at least one outfit for this rank.",
                        RuleId = RuleId
                    });
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
            // Skip advisory checks for parent ranks with pay bands
            if (rank.Parent == null && rank.PayBands.Count > 0)
            {
                return;
            }

            // Flatten ranks to find previous rank
            var flattenedRanks = FlattenRanks(allRanks);
            var rankIndex = flattenedRanks.IndexOf(rank);

            if (rankIndex < 0)
            {
                return; // Rank not in flattened list
            }

            // Check if rank has no vehicles at all
            if (!HasAnyVehicles(rank))
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Advisory,
                    Category = "Vehicle",
                    RankName = rank.Name,
                    RankId = rank.Id,
                    Message = $"Rank '{rank.Name}' has no vehicles assigned (global or station-specific). " +
                             $"Consider adding at least one vehicle for this rank.",
                    RuleId = RuleId
                });
            }

            // Check if rank has no outfits at all
            if (!HasAnyOutfits(rank))
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Advisory,
                    Category = "Outfit",
                    RankName = rank.Name,
                    RankId = rank.Id,
                    Message = $"Rank '{rank.Name}' has no outfits assigned (global or station-specific). " +
                             $"Consider adding at least one outfit for this rank.",
                    RuleId = RuleId
                });
            }

            // If not first rank, compare with previous rank
            if (rankIndex > 0)
            {
                var previous = flattenedRanks[rankIndex - 1];

                // Check if station count decreased
                if (rank.Stations.Count < previous.Stations.Count)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Station",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' has {rank.Stations.Count} station(s), " +
                                 $"but previous rank '{previous.Name}' had {previous.Stations.Count} station(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if vehicles were removed
                var previousVehicleModels = previous.Vehicles.Select(v => v.Model).ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var currentVehicleModels = rank.Vehicles.Select(v => v.Model).ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var removedVehicles = previousVehicleModels.Except(currentVehicleModels).ToList();

                if (removedVehicles.Count > 0)
                {
                    var vehicleList = string.Join(", ", removedVehicles.Take(3));
                    if (removedVehicles.Count > 3)
                    {
                        vehicleList += $" and {removedVehicles.Count - 3} more";
                    }

                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' is missing {removedVehicles.Count} vehicle(s) " +
                                 $"that were available in '{previous.Name}': {vehicleList}. " +
                                 $"Consider if this is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if total vehicle count decreased (without specific vehicles removed)
                if (rank.Vehicles.Count < previous.Vehicles.Count && removedVehicles.Count == 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Vehicle",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' has {rank.Vehicles.Count} vehicle(s), " +
                                 $"but previous rank '{previous.Name}' had {previous.Vehicles.Count} vehicle(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if outfits were removed
                var previousOutfits = previous.Outfits.ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var currentOutfits = rank.Outfits.ToHashSet(System.StringComparer.OrdinalIgnoreCase);
                var removedOutfits = previousOutfits.Except(currentOutfits).ToList();

                if (removedOutfits.Count > 0)
                {
                    var outfitList = string.Join(", ", removedOutfits.Take(3));
                    if (removedOutfits.Count > 3)
                    {
                        outfitList += $" and {removedOutfits.Count - 3} more";
                    }

                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' is missing {removedOutfits.Count} outfit(s) " +
                                 $"that were available in '{previous.Name}': {outfitList}. " +
                                 $"Consider if this is intentional.",
                        RuleId = RuleId
                    });
                }

                // Check if total outfit count decreased (without specific outfits removed)
                if (rank.Outfits.Count < previous.Outfits.Count && removedOutfits.Count == 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Advisory,
                        Category = "Outfit",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' has {rank.Outfits.Count} outfit(s), " +
                                 $"but previous rank '{previous.Name}' had {rank.Outfits.Count} outfit(s). " +
                                 $"Consider if this reduction is intentional.",
                        RuleId = RuleId
                    });
                }
            }
        }

        /// <summary>
        /// Checks if a rank has any vehicles assigned (global or station-specific)
        /// </summary>
        private bool HasAnyVehicles(RankHierarchy rank)
        {
            // Check global vehicles
            if (rank.Vehicles.Count > 0)
                return true;

            // Check station-specific vehicles
            return rank.Stations.Any(s => s.Vehicles.Count > 0);
        }

        /// <summary>
        /// Checks if a rank has any outfits assigned (global or station-specific)
        /// </summary>
        private bool HasAnyOutfits(RankHierarchy rank)
        {
            // Check global outfits
            if (rank.Outfits.Count > 0)
                return true;

            // Check station-specific outfits
            return rank.Stations.Any(s => s.Outfits.Count > 0);
        }

        private List<RankHierarchy> FlattenRanks(List<RankHierarchy> rankHierarchies)
        {
            var allRanks = new List<RankHierarchy>();
            foreach (var hierarchy in rankHierarchies)
            {
                if (hierarchy.IsParent && hierarchy.PayBands.Count > 0)
                {
                    allRanks.AddRange(hierarchy.PayBands);
                }
                else
                {
                    allRanks.Add(hierarchy);
                }
            }
            return allRanks;
        }
    }
}
