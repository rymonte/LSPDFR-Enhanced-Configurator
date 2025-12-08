using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services.Validation.Rules
{
    /// <summary>
    /// Base interface for all validation rules.
    /// Rules are self-contained, testable units that validate specific aspects of ranks.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Unique identifier for this rule (e.g., "RANK_PROGRESSION", "REFERENCE_VALIDATION")
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Human-readable name for this rule
        /// </summary>
        string RuleName { get; }

        /// <summary>
        /// Contexts in which this rule should be executed.
        /// Rules can opt-in to multiple contexts (e.g., Full and Startup but not RealTime).
        /// </summary>
        ValidationContext[] ApplicableContexts { get; }

        /// <summary>
        /// Validates the provided rank hierarchies and adds any issues to the result.
        /// </summary>
        /// <param name="rankHierarchies">All rank hierarchies to validate</param>
        /// <param name="result">Validation result to add issues to</param>
        /// <param name="context">Current validation context</param>
        /// <param name="dataLoadingService">Service for loading reference data (vehicles, stations, outfits)</param>
        void Validate(
            List<RankHierarchy> rankHierarchies,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService);
    }

    /// <summary>
    /// Optional interface for rules that validate individual ranks in isolation.
    /// Useful for real-time validation of single rank edits without revalidating all ranks.
    /// </summary>
    public interface ISingleRankValidationRule : IValidationRule
    {
        /// <summary>
        /// Validates a single rank and adds any issues to the result.
        /// </summary>
        /// <param name="rank">The rank to validate</param>
        /// <param name="allRanks">All ranks for context (e.g., checking duplicates)</param>
        /// <param name="result">Validation result to add issues to</param>
        /// <param name="context">Current validation context</param>
        /// <param name="dataLoadingService">Service for loading reference data</param>
        void ValidateSingleRank(
            RankHierarchy rank,
            List<RankHierarchy> allRanks,
            ValidationResult result,
            ValidationContext context,
            DataLoadingService dataLoadingService);
    }

    /// <summary>
    /// Optional interface for rules that can validate individual properties in real-time.
    /// Useful for immediate feedback during editing without full rank validation.
    /// </summary>
    public interface IPropertyValidationRule : IValidationRule
    {
        /// <summary>
        /// Validates a single property value and adds any issues to the result.
        /// </summary>
        /// <param name="rank">The rank being edited</param>
        /// <param name="propertyName">Name of the property being validated (e.g., "RequiredPoints", "Salary")</param>
        /// <param name="value">The new property value</param>
        /// <param name="allRanks">All ranks for context</param>
        /// <param name="result">Validation result to add issues to</param>
        /// <param name="dataLoadingService">Service for loading reference data</param>
        void ValidateProperty(
            RankHierarchy rank,
            string propertyName,
            object value,
            List<RankHierarchy> allRanks,
            ValidationResult result,
            DataLoadingService dataLoadingService);
    }
}
