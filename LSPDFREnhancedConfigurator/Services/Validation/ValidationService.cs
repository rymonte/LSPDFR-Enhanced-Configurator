using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Rules;

namespace LSPDFREnhancedConfigurator.Services.Validation
{
    /// <summary>
    /// Core validation service implementation.
    /// Orchestrates all validation rules and provides a unified validation interface.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly DataLoadingService _dataLoadingService;
        private readonly List<IValidationRule> _rules = new List<IValidationRule>();

        public ValidationService(DataLoadingService dataLoadingService)
        {
            _dataLoadingService = dataLoadingService;

            // Register default rules
            RegisterDefaultRules();
        }

        private void RegisterDefaultRules()
        {
            // Order matters: run structural validation before reference validation
            RegisterRule(new RankStructureRule());
            RegisterRule(new RankProgressionRule());
            RegisterRule(new ReferenceValidationRule());
            RegisterRule(new AdvisoryRule());
        }

        public ValidationResult ValidateRanks(List<RankHierarchy> rankHierarchies, ValidationContext context)
        {
            var result = new ValidationResult();

            if (rankHierarchies == null)
            {
                Logger.Warn("[VALIDATION] Null rank hierarchies provided to ValidateRanks");
                return result;
            }

            var applicableRules = GetRulesForContext(context);

            Logger.Debug($"[VALIDATION] Validating {rankHierarchies.Count} rank(s) in {context} context with {applicableRules.Count} rule(s)");

            // Execute all applicable rules
            foreach (var rule in applicableRules)
            {
                Logger.Debug($"[VALIDATION] Executing rule: {rule.RuleName} ({rule.RuleId})");
                rule.Validate(rankHierarchies, result, context, _dataLoadingService);
            }

            Logger.Info($"[VALIDATION] Validation complete: {result.GetCompactSummary()}");

            return result;
        }

        public ValidationResult ValidateSingleRank(
            RankHierarchy rank,
            List<RankHierarchy> allRanks,
            ValidationContext context)
        {
            var result = new ValidationResult();

            if (rank == null)
            {
                Logger.Warn("[VALIDATION] Null rank provided to ValidateSingleRank");
                return result;
            }

            var applicableRules = GetRulesForContext(context)
                .OfType<ISingleRankValidationRule>()
                .ToList();

            Logger.Debug($"[VALIDATION] Validating single rank '{rank.Name}' in {context} context with {applicableRules.Count} rule(s)");

            // Execute all applicable single-rank rules
            foreach (var rule in applicableRules)
            {
                Logger.Debug($"[VALIDATION] Executing single-rank rule: {rule.RuleName} ({rule.RuleId})");
                rule.ValidateSingleRank(rank, allRanks, result, context, _dataLoadingService);
            }

            Logger.Debug($"[VALIDATION] Single rank validation complete: {result.GetCompactSummary()}");

            return result;
        }

        public ValidationResult ValidateProperty(
            RankHierarchy rank,
            string propertyName,
            object value,
            List<RankHierarchy> allRanks)
        {
            var result = new ValidationResult();

            if (rank == null || string.IsNullOrWhiteSpace(propertyName))
            {
                Logger.Warn("[VALIDATION] Invalid parameters provided to ValidateProperty");
                return result;
            }

            // Property validation always uses RealTime context
            var context = ValidationContext.RealTime;

            var applicableRules = GetRulesForContext(context)
                .OfType<IPropertyValidationRule>()
                .ToList();

            Logger.Debug($"[VALIDATION] Validating property '{propertyName}' for rank '{rank.Name}' with {applicableRules.Count} rule(s)");

            // Execute all applicable property validation rules
            foreach (var rule in applicableRules)
            {
                Logger.Debug($"[VALIDATION] Executing property rule: {rule.RuleName} ({rule.RuleId})");
                rule.ValidateProperty(rank, propertyName, value, allRanks, result, _dataLoadingService);
            }

            Logger.Debug($"[VALIDATION] Property validation complete: {result.GetCompactSummary()}");

            return result;
        }

        public void RegisterRule(IValidationRule rule)
        {
            if (rule == null)
            {
                Logger.Warn("[VALIDATION] Attempted to register null rule");
                return;
            }

            // Check if rule with same ID already exists
            if (_rules.Any(r => r.RuleId == rule.RuleId))
            {
                Logger.Warn($"[VALIDATION] Rule with ID '{rule.RuleId}' is already registered. Skipping.");
                return;
            }

            _rules.Add(rule);
            Logger.Debug($"[VALIDATION] Registered rule: {rule.RuleName} ({rule.RuleId})");
        }

        public bool UnregisterRule(string ruleId)
        {
            var rule = _rules.FirstOrDefault(r => r.RuleId == ruleId);
            if (rule != null)
            {
                _rules.Remove(rule);
                Logger.Debug($"[VALIDATION] Unregistered rule: {rule.RuleName} ({ruleId})");
                return true;
            }

            Logger.Warn($"[VALIDATION] Cannot unregister rule '{ruleId}': not found");
            return false;
        }

        public IReadOnlyList<IValidationRule> GetRegisteredRules()
        {
            return _rules.AsReadOnly();
        }

        public IReadOnlyList<IValidationRule> GetRulesForContext(ValidationContext context)
        {
            var applicableRules = _rules
                .Where(r => r.ApplicableContexts.Contains(context))
                .ToList();

            return applicableRules.AsReadOnly();
        }
    }
}
