using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation.Rules
{
    /// <summary>
    /// Validates rank progression rules: XP progression, salary progression, pay band structure.
    /// Ensures ranks have logical progression and no negative values.
    /// </summary>
    public class RankProgressionRule : IValidationRule, ISingleRankValidationRule, IPropertyValidationRule
    {
        public string RuleId => "RANK_PROGRESSION";
        public string RuleName => "Rank Progression Validation";

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
            if (rankHierarchies.Count == 0)
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Structure",
                    Message = "No ranks defined. At least one rank is required.",
                    RuleId = RuleId,
                    SuggestedFix = "Add at least one rank to the configuration."
                });
                return;
            }

            // Flatten all ranks (including pay bands)
            var allRanks = FlattenRanks(rankHierarchies);

            // Check first rank starts at 0
            if (allRanks[0].RequiredPoints != 0)
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Rank",
                    RankName = allRanks[0].Name,
                    RankId = allRanks[0].Id,
                    Message = $"First rank '{allRanks[0].Name}' must start at XP 0, not {allRanks[0].RequiredPoints}.",
                    RuleId = RuleId,
                    SuggestedFix = "Set Required Points to 0 for the first rank.",
                    IsAutoFixable = true,
                    AutoFixAction = () => allRanks[0].RequiredPoints = 0
                });
            }

            // Check for XP progression - each rank must have RequiredPoints greater than previous
            for (int i = 0; i < allRanks.Count - 1; i++)
            {
                var current = allRanks[i];
                var next = allRanks[i + 1];

                if (next.RequiredPoints <= current.RequiredPoints)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = next.Name,
                        RankId = next.Id,
                        Message = $"Rank '{next.Name}' (XP: {next.RequiredPoints}) must have Required Points greater than '{current.Name}' (XP: {current.RequiredPoints}).",
                        RuleId = RuleId,
                        SuggestedFix = $"Set Required Points to at least {current.RequiredPoints + 1}.",
                        PropertyName = "RequiredPoints"
                    });
                }
            }

            // Check for negative values
            foreach (var rank in allRanks)
            {
                if (rank.RequiredPoints < 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' has negative Required Points ({rank.RequiredPoints}).",
                        RuleId = RuleId,
                        SuggestedFix = "Set Required Points to 0 or higher.",
                        PropertyName = "RequiredPoints",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.RequiredPoints = 0
                    });
                }

                if (rank.Salary < 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Rank '{rank.Name}' has negative Salary ({rank.Salary}).",
                        RuleId = RuleId,
                        SuggestedFix = "Set Salary to 0 or higher.",
                        PropertyName = "Salary",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.Salary = 0
                    });
                }
            }

            // Salary progression warnings (advisory in RealTime, warning in Full/Startup)
            for (int i = 0; i < allRanks.Count - 1; i++)
            {
                var current = allRanks[i];
                var next = allRanks[i + 1];

                if (next.Salary < current.Salary)
                {
                    var severity = context == ValidationContext.RealTime
                        ? ValidationSeverity.Advisory
                        : ValidationSeverity.Warning;

                    result.AddIssue(new ValidationIssue
                    {
                        Severity = severity,
                        Category = "Rank",
                        RankName = next.Name,
                        RankId = next.Id,
                        Message = $"Rank '{next.Name}' has lower salary (${next.Salary}) than previous rank '{current.Name}' (${current.Salary}).",
                        RuleId = RuleId,
                        SuggestedFix = "Consider increasing the salary to maintain progression.",
                        PropertyName = "Salary"
                    });
                }
            }

            // Validate pay band structure for parent ranks
            foreach (var hierarchy in rankHierarchies.Where(h => h.IsParent))
            {
                if (hierarchy.PayBands.Count < 2)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = hierarchy.Name,
                        RankId = hierarchy.Id,
                        Message = $"Parent rank '{hierarchy.Name}' must have at least 2 pay bands.",
                        RuleId = RuleId,
                        SuggestedFix = "Add at least one more pay band or convert to a standalone rank."
                    });
                }

                // Validate progression within pay bands
                for (int i = 0; i < hierarchy.PayBands.Count - 1; i++)
                {
                    var current = hierarchy.PayBands[i];
                    var next = hierarchy.PayBands[i + 1];

                    if (next.RequiredPoints <= current.RequiredPoints)
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Error,
                            Category = "Rank",
                            RankName = next.Name,
                            RankId = next.Id,
                            Message = $"Pay band '{next.Name}' (XP: {next.RequiredPoints}) must have higher XP than '{current.Name}' (XP: {current.RequiredPoints}).",
                            RuleId = RuleId,
                            SuggestedFix = $"Set Required Points to at least {current.RequiredPoints + 1}.",
                            PropertyName = "RequiredPoints"
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
            // Skip validation for parent ranks with pay bands (their XP/Salary are derived from children)
            bool isParentWithPayBands = rank.Parent == null && rank.PayBands.Count > 0;

            if (!isParentWithPayBands)
            {
                // Negative value checks for single rank
                if (rank.RequiredPoints < 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Required Points cannot be negative ({rank.RequiredPoints}).",
                        RuleId = RuleId,
                        SuggestedFix = "Set Required Points to 0 or higher.",
                        PropertyName = "RequiredPoints",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.RequiredPoints = 0
                    });
                }

                if (rank.Salary < 0)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Rank",
                        RankName = rank.Name,
                        RankId = rank.Id,
                        Message = $"Salary cannot be negative ({rank.Salary}).",
                        RuleId = RuleId,
                        SuggestedFix = "Set Salary to 0 or higher.",
                        PropertyName = "Salary",
                        IsAutoFixable = true,
                        AutoFixAction = () => rank.Salary = 0
                    });
                }

                // Check XP and salary progression against previous rank
                var flattenedRanks = FlattenRanks(allRanks);
                var rankIndex = flattenedRanks.IndexOf(rank);

                if (rankIndex > 0)
                {
                    var previousRank = flattenedRanks[rankIndex - 1];

                    // Check XP progression
                    if (rank.RequiredPoints <= previousRank.RequiredPoints)
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Error,
                            Category = "Rank",
                            RankName = rank.Name,
                            RankId = rank.Id,
                            Message = $"Required Points ({rank.RequiredPoints}) must be greater than previous rank '{previousRank.Name}' ({previousRank.RequiredPoints}).",
                            RuleId = RuleId,
                            SuggestedFix = $"Set Required Points to at least {previousRank.RequiredPoints + 1}.",
                            PropertyName = "RequiredPoints"
                        });
                    }

                    // Check salary progression
                    if (rank.Salary < previousRank.Salary)
                    {
                        var severity = context == ValidationContext.RealTime
                            ? ValidationSeverity.Advisory
                            : ValidationSeverity.Warning;

                        result.AddIssue(new ValidationIssue
                        {
                            Severity = severity,
                            Category = "Rank",
                            RankName = rank.Name,
                            RankId = rank.Id,
                            Message = $"Salary is lower than previous ({previousRank.Salary})",
                            RuleId = RuleId,
                            SuggestedFix = "Consider increasing the salary to maintain progression.",
                            PropertyName = "Salary"
                        });
                    }
                }
            }

            // Pay band structure check (applies to parent ranks with pay bands)
            if (rank.IsParent && rank.PayBands.Count < 2)
            {
                result.AddIssue(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Rank",
                    RankName = rank.Name,
                    RankId = rank.Id,
                    Message = "Parent rank must have at least 2 pay bands.",
                    RuleId = RuleId,
                    SuggestedFix = "Add at least one more pay band or convert to a standalone rank."
                });
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
            switch (propertyName)
            {
                case "RequiredPoints":
                    if (value is int requiredPoints)
                    {
                        if (requiredPoints < 0)
                        {
                            result.AddIssue(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Error,
                                Category = "Rank",
                                RankName = rank.Name,
                                RankId = rank.Id,
                                Message = "Required Points cannot be negative.",
                                RuleId = RuleId,
                                SuggestedFix = "Set Required Points to 0 or higher.",
                                PropertyName = propertyName
                            });
                        }
                    }
                    break;

                case "Salary":
                    if (value is int salary)
                    {
                        if (salary < 0)
                        {
                            result.AddIssue(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Error,
                                Category = "Rank",
                                RankName = rank.Name,
                                RankId = rank.Id,
                                Message = "Salary cannot be negative.",
                                RuleId = RuleId,
                                SuggestedFix = "Set Salary to 0 or higher.",
                                PropertyName = propertyName
                            });
                        }
                    }
                    break;
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
