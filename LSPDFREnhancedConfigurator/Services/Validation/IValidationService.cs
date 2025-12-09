using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Rules;

namespace LSPDFREnhancedConfigurator.Services.Validation
{
    /// <summary>
    /// Core validation service that orchestrates all validation rules.
    /// Provides a unified interface for validating ranks in different contexts.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates all ranks according to the specified context.
        /// Runs all applicable rules and aggregates results.
        /// </summary>
        /// <param name="rankHierarchies">All rank hierarchies to validate</param>
        /// <param name="context">Validation context (Full, RealTime, PreGenerate, etc.)</param>
        /// <returns>Validation result containing all issues found</returns>
        ValidationResult ValidateRanks(List<RankHierarchy> rankHierarchies, ValidationContext context);

        /// <summary>
        /// Validates a single rank in the context of all ranks.
        /// Useful for real-time validation during editing.
        /// </summary>
        /// <param name="rank">The rank to validate</param>
        /// <param name="allRanks">All ranks for context (checking duplicates, progression, etc.)</param>
        /// <param name="context">Validation context</param>
        /// <returns>Validation result containing issues for this rank</returns>
        ValidationResult ValidateSingleRank(
            RankHierarchy rank,
            List<RankHierarchy> allRanks,
            ValidationContext context);

        /// <summary>
        /// Validates a single property change in real-time.
        /// Provides immediate feedback during editing without full rank validation.
        /// </summary>
        /// <param name="rank">The rank being edited</param>
        /// <param name="propertyName">Name of the property being validated</param>
        /// <param name="value">The new property value</param>
        /// <param name="allRanks">All ranks for context</param>
        /// <returns>Validation result containing issues for this property</returns>
        ValidationResult ValidateProperty(
            RankHierarchy rank,
            string propertyName,
            object value,
            List<RankHierarchy> allRanks);

        /// <summary>
        /// Registers a custom validation rule.
        /// Allows extending validation with plugin or custom rules.
        /// </summary>
        /// <param name="rule">The rule to register</param>
        void RegisterRule(IValidationRule rule);

        /// <summary>
        /// Unregisters a validation rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule to unregister</param>
        /// <returns>True if rule was found and removed, false otherwise</returns>
        bool UnregisterRule(string ruleId);

        /// <summary>
        /// Gets all currently registered rules.
        /// </summary>
        IReadOnlyList<IValidationRule> GetRegisteredRules();

        /// <summary>
        /// Gets rules applicable to a specific context.
        /// </summary>
        /// <param name="context">The validation context</param>
        IReadOnlyList<IValidationRule> GetRulesForContext(ValidationContext context);
    }
}
