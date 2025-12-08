using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation.Rules
{
    /// <summary>
    /// Validates rank structure: names, uniqueness, and basic integrity.
    /// Ensures all ranks have valid names and no duplicates exist.
    /// </summary>
    public class RankStructureRule : IValidationRule, ISingleRankValidationRule, IPropertyValidationRule
    {
        public string RuleId => "RANK_STRUCTURE";
        public string RuleName => "Rank Structure Validation";

        public ValidationContext[] ApplicableContexts => new[]
        {
            ValidationContext.Full,
            ValidationContext.Startup,
            ValidationContext.PreGenerate,
            ValidationContext.RealTime
        };

        public void Validate(
            List<RankHierarchy> rankHierarchies,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService)
        {
            // Flatten all ranks (including pay bands)
            var allRanks = FlattenRanks(rankHierarchies);

            // Check for empty names
            foreach (var rank in allRanks)
            {
                if (string.IsNullOrWhiteSpace(rank.Name))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankId = rank.Id,
                        Message = "Rank name cannot be empty.",
                        RuleId = RuleId,
                        SuggestedFix = "Enter a name for this rank.",
                        PropertyName = "Name"
                    });
                }
            }

            // Check for duplicate rank names
            var nameGroups = allRanks
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .GroupBy(r => r.Name, System.StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                var duplicateRanks = group.ToList();
                foreach (var rank in duplicateRanks)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Rank",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Duplicate rank name '{rank.Name}' found. Each rank should have a unique name.",
                        RuleId = RuleId,
                        SuggestedFix = "Rename one of the ranks to make it unique.",
                        PropertyName = "Name"
                    });
                }
            }

            // Validate parent rank structure
            foreach (var hierarchy in rankHierarchies.Where(h => h.IsParent))
            {
                // Empty parent name check
                if (string.IsNullOrWhiteSpace(hierarchy.Name))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankId = hierarchy.Id,
                        Message = "Parent rank name cannot be empty.",
                        RuleId = RuleId,
                        SuggestedFix = "Enter a name for this parent rank.",
                        PropertyName = "Name"
                    });
                }

                // Check pay band names
                foreach (var payBand in hierarchy.PayBands)
                {
                    if (string.IsNullOrWhiteSpace(payBand.Name))
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Error,
                            Category = "Rank",
                            RankName = hierarchy.Name,
                            RankId = payBand.Id,
                            Message = $"Pay band in '{hierarchy.Name}' has empty name.",
                            RuleId = RuleId,
                            SuggestedFix = "Enter a name for this pay band.",
                            PropertyName = "Name"
                        });
                    }
                }

                // Check for duplicate pay band names within parent
                var payBandNameGroups = hierarchy.PayBands
                    .Where(pb => !string.IsNullOrWhiteSpace(pb.Name))
                    .GroupBy(pb => pb.Name, System.StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1);

                foreach (var group in payBandNameGroups)
                {
                    var duplicates = group.ToList();
                    foreach (var payBand in duplicates)
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Category = "Rank",
                            RankName = hierarchy.Name,
                            RankId = payBand.Id,
                            Message = $"Duplicate pay band name '{payBand.Name}' in parent rank '{hierarchy.Name}'.",
                            RuleId = RuleId,
                            SuggestedFix = "Rename one of the pay bands to make it unique within this parent rank.",
                            PropertyName = "Name"
                        });
                    }
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
            // Check for empty name
            if (string.IsNullOrWhiteSpace(rank.Name))
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Rank",
                    RankId = rank.Id,
                    Message = "Rank name cannot be empty.",
                    RuleId = RuleId,
                    SuggestedFix = "Enter a name for this rank.",
                    PropertyName = "Name"
                });
            }

            // Check for duplicate name against other ranks
            var allFlattenedRanks = FlattenRanks(allRanks);
            var duplicates = allFlattenedRanks
                .Where(r => r.Id != rank.Id &&
                           !string.IsNullOrWhiteSpace(r.Name) &&
                           r.Name.Equals(rank.Name, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (duplicates.Any())
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Category = "Rank",
                    RankName = rank.Name,
                    RankId = rank.Id,
                    Message = $"Duplicate rank name '{rank.Name}'. Each rank should have a unique name.",
                    RuleId = RuleId,
                    SuggestedFix = "Choose a different name to make this rank unique.",
                    PropertyName = "Name"
                });
            }

            // Validate pay bands if parent
            if (rank.IsParent)
            {
                foreach (var payBand in rank.PayBands)
                {
                    if (string.IsNullOrWhiteSpace(payBand.Name))
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Error,
                            Category = "Rank",
                            RankName = rank.Name,
                            RankId = payBand.Id,
                            Message = "Pay band name cannot be empty.",
                            RuleId = RuleId,
                            SuggestedFix = "Enter a name for this pay band.",
                            PropertyName = "Name"
                        });
                    }
                }

                // Check for duplicate pay band names
                var payBandNameGroups = rank.PayBands
                    .Where(pb => !string.IsNullOrWhiteSpace(pb.Name))
                    .GroupBy(pb => pb.Name, System.StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1);

                foreach (var group in payBandNameGroups)
                {
                    foreach (var payBand in group)
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Category = "Rank",
                            RankName = rank.Name,
                            RankId = payBand.Id,
                            Message = $"Duplicate pay band name '{payBand.Name}' in this parent rank.",
                            RuleId = RuleId,
                            SuggestedFix = "Rename one of the pay bands to make it unique.",
                            PropertyName = "Name"
                        });
                    }
                }
            }
        }

        public void ValidateProperty(
            RankHierarchy rank,
            string propertyName,
            object value,
            List<RankHierarchy> allRanks,
            ValidationResult result,
            DataLoadingService dataLoadingService)
        {
            if (propertyName == "Name" && value is string name)
            {
                // Check for empty name
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankId = rank.Id,
                        Message = "Rank name cannot be empty.",
                        RuleId = RuleId,
                        SuggestedFix = "Enter a name for this rank.",
                        PropertyName = propertyName
                    });
                    return;
                }

                // Check for duplicate name
                var allFlattenedRanks = FlattenRanks(allRanks);
                var duplicates = allFlattenedRanks
                    .Where(r => r.Id != rank.Id &&
                               !string.IsNullOrWhiteSpace(r.Name) &&
                               r.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (duplicates.Any())
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Rank",
                        RankName = name,
                        RankId = rank.Id,
                        Message = $"Duplicate rank name '{name}'. Each rank should have a unique name.",
                        RuleId = RuleId,
                        SuggestedFix = "Choose a different name to make this rank unique.",
                        PropertyName = propertyName
                    });
                }
            }
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
