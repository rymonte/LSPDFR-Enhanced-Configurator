using System.Linq;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation
{
    /// <summary>
    /// Extension methods for ValidationResult to simplify common validation query patterns
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Gets the first advisory or warning message for a specific category and rank
        /// </summary>
        /// <param name="result">The validation result to query</param>
        /// <param name="rankId">The ID of the rank to filter by</param>
        /// <param name="category">The category to filter by (e.g., "Station", "Vehicle", "Outfit")</param>
        /// <returns>The first advisory/warning message found, or empty string if none</returns>
        public static string GetFirstAdvisoryMessage(
            this ValidationResult result,
            string rankId,
            string category)
        {
            var advisory = result.Issues
                .Where(i => (i.Severity == ValidationSeverity.Advisory || i.Severity == ValidationSeverity.Warning) &&
                           i.RankId == rankId &&
                           i.Category == category)
                .FirstOrDefault();

            return advisory?.Message ?? string.Empty;
        }

        /// <summary>
        /// Gets the first advisory or warning message for a specific property and rank
        /// </summary>
        /// <param name="result">The validation result to query</param>
        /// <param name="rankId">The ID of the rank to filter by</param>
        /// <param name="propertyName">The property name to filter by (e.g., "Salary", "RequiredPoints")</param>
        /// <returns>The first advisory/warning message found, or empty string if none</returns>
        public static string GetFirstAdvisoryForProperty(
            this ValidationResult result,
            string rankId,
            string propertyName)
        {
            var advisory = result.Issues
                .Where(i => (i.Severity == ValidationSeverity.Advisory || i.Severity == ValidationSeverity.Warning) &&
                           i.RankId == rankId &&
                           (i.PropertyName == propertyName || i.Message.Contains(propertyName)))
                .FirstOrDefault();

            return advisory?.Message ?? string.Empty;
        }
    }
}
