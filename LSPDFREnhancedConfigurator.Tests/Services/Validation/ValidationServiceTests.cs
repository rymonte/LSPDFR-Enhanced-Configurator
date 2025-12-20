using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Rules;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services.Validation
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class ValidationServiceTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;
        private readonly ValidationService _validationService;

        public ValidationServiceTests()
        {
            _mockDataService = new MockServiceBuilder().BuildMock();
            _validationService = new ValidationService(_mockDataService.Object);
        }

        #region Constructor Tests

        [Fact]
        public void ValidationService_Constructor_RegistersDefaultRules()
        {
            // Arrange & Act
            var service = new ValidationService(_mockDataService.Object);
            var rules = service.GetRegisteredRules();

            // Assert
            rules.Should().NotBeEmpty();
            rules.Should().HaveCountGreaterOrEqualTo(4); // At least 4 default rules

            // Check for specific default rules by type
            rules.Should().Contain(r => r.GetType().Name == "RankStructureRule");
            rules.Should().Contain(r => r.GetType().Name == "RankProgressionRule");
            rules.Should().Contain(r => r.GetType().Name == "ReferenceValidationRule");
            rules.Should().Contain(r => r.GetType().Name == "AdvisoryRule");
        }

        #endregion

        #region ValidateRanks Tests

        [Fact]
        public void ValidateRanks_NullRanks_ReturnsEmptyResult()
        {
            // Act
            var result = _validationService.ValidateRanks(null, ValidationContext.Full);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateRanks_EmptyList_ReturnsResult()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var result = _validationService.ValidateRanks(ranks, ValidationContext.Full);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void ValidateRanks_WithRanks_ExecutesRules()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var result = _validationService.ValidateRanks(ranks, ValidationContext.Full);

            // Assert
            result.Should().NotBeNull();
            // Rules should have been executed (may or may not produce issues depending on data)
        }

        [Fact]
        public void ValidateRanks_DifferentContexts_FiltersRules()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act - Test different contexts
            var resultFull = _validationService.ValidateRanks(ranks, ValidationContext.Full);
            var resultRealTime = _validationService.ValidateRanks(ranks, ValidationContext.RealTime);
            var resultPreGenerate = _validationService.ValidateRanks(ranks, ValidationContext.PreGenerate);

            // Assert
            resultFull.Should().NotBeNull();
            resultRealTime.Should().NotBeNull();
            resultPreGenerate.Should().NotBeNull();
        }

        #endregion

        #region ValidateSingleRank Tests

        [Fact]
        public void ValidateSingleRank_NullRank_ReturnsEmptyResult()
        {
            // Arrange
            var allRanks = new List<RankHierarchy>();

            // Act
            var result = _validationService.ValidateSingleRank(null, allRanks, ValidationContext.RealTime);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateSingleRank_ValidRank_ExecutesRules()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var result = _validationService.ValidateSingleRank(rank, allRanks, ValidationContext.RealTime);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void ValidateSingleRank_DifferentContexts_FiltersRules()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var resultFull = _validationService.ValidateSingleRank(rank, allRanks, ValidationContext.Full);
            var resultRealTime = _validationService.ValidateSingleRank(rank, allRanks, ValidationContext.RealTime);

            // Assert
            resultFull.Should().NotBeNull();
            resultRealTime.Should().NotBeNull();
        }

        #endregion

        #region ValidateProperty Tests

        [Fact]
        public void ValidateProperty_NullRank_ReturnsEmptyResult()
        {
            // Arrange
            var allRanks = new List<RankHierarchy>();

            // Act
            var result = _validationService.ValidateProperty(null, "Name", "Test", allRanks);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_NullPropertyName_ReturnsEmptyResult()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var result = _validationService.ValidateProperty(rank, null, "Test", allRanks);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_EmptyPropertyName_ReturnsEmptyResult()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var result = _validationService.ValidateProperty(rank, "", "Test", allRanks);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_WhitespacePropertyName_ReturnsEmptyResult()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var result = _validationService.ValidateProperty(rank, "   ", "Test", allRanks);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_ValidInputs_ExecutesRules()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };

            // Act
            var result = _validationService.ValidateProperty(rank, "Name", "NewName", allRanks);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region RegisterRule Tests

        [Fact]
        public void RegisterRule_NullRule_DoesNotThrow()
        {
            // Act
            var act = () => _validationService.RegisterRule(null);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void RegisterRule_ValidRule_AddsToRules()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);
            var initialCount = service.GetRegisteredRules().Count;

            var mockRule = new Mock<IValidationRule>();
            mockRule.Setup(r => r.RuleId).Returns("TEST_RULE_001");
            mockRule.Setup(r => r.RuleName).Returns("Test Rule");
            mockRule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            // Act
            service.RegisterRule(mockRule.Object);

            // Assert
            var rules = service.GetRegisteredRules();
            rules.Should().HaveCount(initialCount + 1);
            rules.Should().Contain(mockRule.Object);
        }

        [Fact]
        public void RegisterRule_DuplicateRuleId_DoesNotAdd()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            var mockRule1 = new Mock<IValidationRule>();
            mockRule1.Setup(r => r.RuleId).Returns("DUPLICATE_RULE");
            mockRule1.Setup(r => r.RuleName).Returns("First Rule");
            mockRule1.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            var mockRule2 = new Mock<IValidationRule>();
            mockRule2.Setup(r => r.RuleId).Returns("DUPLICATE_RULE"); // Same ID
            mockRule2.Setup(r => r.RuleName).Returns("Second Rule");
            mockRule2.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            service.RegisterRule(mockRule1.Object);
            var countAfterFirst = service.GetRegisteredRules().Count;

            // Act
            service.RegisterRule(mockRule2.Object);

            // Assert
            var rules = service.GetRegisteredRules();
            rules.Should().HaveCount(countAfterFirst); // Count should not increase
            rules.Should().Contain(mockRule1.Object);
            rules.Should().NotContain(mockRule2.Object);
        }

        #endregion

        #region UnregisterRule Tests

        [Fact]
        public void UnregisterRule_ExistingRule_RemovesAndReturnsTrue()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            var mockRule = new Mock<IValidationRule>();
            mockRule.Setup(r => r.RuleId).Returns("REMOVE_ME");
            mockRule.Setup(r => r.RuleName).Returns("Removable Rule");
            mockRule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            service.RegisterRule(mockRule.Object);
            var countBefore = service.GetRegisteredRules().Count;

            // Act
            var result = service.UnregisterRule("REMOVE_ME");

            // Assert
            result.Should().BeTrue();
            var rules = service.GetRegisteredRules();
            rules.Should().HaveCount(countBefore - 1);
            rules.Should().NotContain(mockRule.Object);
        }

        [Fact]
        public void UnregisterRule_NonExistentRule_ReturnsFalse()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);
            var countBefore = service.GetRegisteredRules().Count;

            // Act
            var result = service.UnregisterRule("NON_EXISTENT_RULE");

            // Assert
            result.Should().BeFalse();
            service.GetRegisteredRules().Should().HaveCount(countBefore);
        }

        [Fact]
        public void UnregisterRule_NullOrEmptyRuleId_ReturnsFalse()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            // Act & Assert
            service.UnregisterRule(null).Should().BeFalse();
            service.UnregisterRule("").Should().BeFalse();
        }

        #endregion

        #region GetRegisteredRules Tests

        [Fact]
        public void GetRegisteredRules_ReturnsReadOnlyList()
        {
            // Act
            var rules = _validationService.GetRegisteredRules();

            // Assert
            rules.Should().NotBeNull();
            rules.Should().BeAssignableTo<IReadOnlyList<IValidationRule>>();
        }

        [Fact]
        public void GetRegisteredRules_IncludesAllRegisteredRules()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);
            var defaultCount = service.GetRegisteredRules().Count;

            var mockRule = new Mock<IValidationRule>();
            mockRule.Setup(r => r.RuleId).Returns("TEST_RULE");
            mockRule.Setup(r => r.RuleName).Returns("Test Rule");
            mockRule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            service.RegisterRule(mockRule.Object);

            // Act
            var rules = service.GetRegisteredRules();

            // Assert
            rules.Should().HaveCount(defaultCount + 1);
            rules.Should().Contain(mockRule.Object);
        }

        #endregion

        #region GetRulesForContext Tests

        [Fact]
        public void GetRulesForContext_ReturnsOnlyApplicableRules()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            var fullContextRule = new Mock<IValidationRule>();
            fullContextRule.Setup(r => r.RuleId).Returns("FULL_ONLY");
            fullContextRule.Setup(r => r.RuleName).Returns("Full Context Rule");
            fullContextRule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            var realTimeRule = new Mock<IValidationRule>();
            realTimeRule.Setup(r => r.RuleId).Returns("REALTIME_ONLY");
            realTimeRule.Setup(r => r.RuleName).Returns("RealTime Rule");
            realTimeRule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.RealTime });

            service.RegisterRule(fullContextRule.Object);
            service.RegisterRule(realTimeRule.Object);

            // Act
            var fullRules = service.GetRulesForContext(ValidationContext.Full);
            var realTimeRules = service.GetRulesForContext(ValidationContext.RealTime);

            // Assert
            fullRules.Should().Contain(fullContextRule.Object);
            fullRules.Should().NotContain(realTimeRule.Object);

            realTimeRules.Should().Contain(realTimeRule.Object);
            realTimeRules.Should().NotContain(fullContextRule.Object);
        }

        [Fact]
        public void GetRulesForContext_ReturnsReadOnlyList()
        {
            // Act
            var rules = _validationService.GetRulesForContext(ValidationContext.Full);

            // Assert
            rules.Should().NotBeNull();
            rules.Should().BeAssignableTo<IReadOnlyList<IValidationRule>>();
        }

        [Fact]
        public void GetRulesForContext_MultipleContextRule_IncludedInBoth()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            var multiContextRule = new Mock<IValidationRule>();
            multiContextRule.Setup(r => r.RuleId).Returns("MULTI_CONTEXT");
            multiContextRule.Setup(r => r.RuleName).Returns("Multi Context Rule");
            multiContextRule.Setup(r => r.ApplicableContexts).Returns(new[]
            {
                ValidationContext.Full,
                ValidationContext.RealTime
            });

            service.RegisterRule(multiContextRule.Object);

            // Act
            var fullRules = service.GetRulesForContext(ValidationContext.Full);
            var realTimeRules = service.GetRulesForContext(ValidationContext.RealTime);

            // Assert
            fullRules.Should().Contain(multiContextRule.Object);
            realTimeRules.Should().Contain(multiContextRule.Object);
        }

        [Fact]
        public void GetRulesForContext_NoMatchingRules_ReturnsEmpty()
        {
            // Arrange
            var service = new ValidationService(_mockDataService.Object);

            var rule = new Mock<IValidationRule>();
            rule.Setup(r => r.RuleId).Returns("FULL_ONLY");
            rule.Setup(r => r.RuleName).Returns("Full Only Rule");
            rule.Setup(r => r.ApplicableContexts).Returns(new[] { ValidationContext.Full });

            service.RegisterRule(rule.Object);

            // Unregister all default rules to get clean test
            var defaultRules = service.GetRegisteredRules().ToList();
            foreach (var defaultRule in defaultRules)
            {
                service.UnregisterRule(defaultRule.RuleId);
            }

            service.RegisterRule(rule.Object);

            // Act
            var startupRules = service.GetRulesForContext(ValidationContext.Startup);

            // Assert
            startupRules.Should().BeEmpty();
        }

        #endregion
    }
}
