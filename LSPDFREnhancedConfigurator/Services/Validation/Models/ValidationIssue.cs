using System;

namespace LSPDFREnhancedConfigurator.Services.Validation.Models
{
    /// <summary>
    /// Represents a single validation issue with contextual information.
    /// Provides details about what failed validation, where, and how to fix it.
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Severity level of the issue (Error, Warning, Advisory)
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// Category of the issue (e.g., "Rank", "Vehicle", "Station", "Outfit", "Structure", "Directory")
        /// Used for grouping and filtering validation results.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Rank name this issue is associated with (if applicable)
        /// </summary>
        public string? RankName { get; set; }

        /// <summary>
        /// Rank ID for programmatic identification and lookup
        /// </summary>
        public string? RankId { get; set; }

        /// <summary>
        /// Item name (vehicle model, outfit name, station name, etc.) if applicable
        /// </summary>
        public string? ItemName { get; set; }

        /// <summary>
        /// Human-readable error message describing the issue
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Property name that failed validation (for real-time property validation)
        /// Examples: "RequiredPoints", "Salary", "Name"
        /// </summary>
        public string? PropertyName { get; set; }

        /// <summary>
        /// Unique identifier of the rule that generated this issue
        /// Examples: "RANK_PROGRESSION", "REFERENCE_VALIDATION"
        /// </summary>
        public string RuleId { get; set; } = string.Empty;

        /// <summary>
        /// Suggested fix or action to resolve this issue (optional)
        /// Examples: "Set Required Points to 0 or higher", "Remove invalid vehicle"
        /// </summary>
        public string? SuggestedFix { get; set; }

        /// <summary>
        /// Whether this issue can be automatically fixed
        /// If true, AutoFixAction should be provided
        /// </summary>
        public bool IsAutoFixable { get; set; }

        /// <summary>
        /// Action to automatically fix this issue (if IsAutoFixable is true)
        /// Example: () => rank.Vehicles.Remove(invalidVehicle)
        /// </summary>
        public Action? AutoFixAction { get; set; }

        /// <summary>
        /// Creates a formatted string representation of this validation issue
        /// </summary>
        public override string ToString()
        {
            var prefix = Severity switch
            {
                ValidationSeverity.Error => "❌ ERROR",
                ValidationSeverity.Warning => "⚠️ WARNING",
                ValidationSeverity.Advisory => "ℹ️ ADVISORY",
                _ => "INFO"
            };

            return $"{prefix}: {Message}";
        }
    }
}
