using System.Collections.Generic;
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
    [Trait("Component", "Validation")]
    public class RankStructureRuleTests
    {
        private readonly RankStructureRule _rule;
        private readonly Mock<DataLoadingService> _mockDataService;

        public RankStructureRuleTests()
        {
            _rule = new RankStructureRule();
            _mockDataService = new MockServiceBuilder().BuildMock();
        }

        #region Empty Name Validation

        [Fact]
        public void Validate_RankWithEmptyName_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("")
                .Build();
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        [Fact]
        public void Validate_RankWithValidName_PassesValidation()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i => i.Message.Contains("cannot be empty"));
        }

        #endregion

        #region Negative Values Validation

        [Fact]
        public void Validate_RankWithNegativeRequiredPoints_DoesNotValidate()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = -100;
            rank.Salary = 1000;
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - RankStructureRule does not validate numeric constraints
            // Those would be validated at the UI/property level, not in structural validation
            result.Issues.Should().NotContain(i => i.Message.Contains("Required points"));
        }

        [Fact]
        public void Validate_RankWithNegativeSalary_DoesNotValidate()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = -1000;
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - RankStructureRule does not validate numeric constraints
            // Those would be validated at the UI/property level, not in structural validation
            result.Issues.Should().NotContain(i => i.Message.Contains("Salary"));
        }

        #endregion

        #region Duplicate Name Validation

        [Fact]
        public void Validate_DuplicateRankNames_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            var rank2 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Duplicate names produce warnings, not errors
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("Duplicate") &&
                i.Message.Contains("Officer"));
        }

        [Fact]
        public void Validate_UniqueRankNames_PassesValidation()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i => i.Message.Contains("duplicate"));
        }

        #endregion

        #region Parent Rank Validation

        [Fact]
        public void Validate_ParentRankWithPayBands_PassesValidation()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand1 = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand1.RequiredPoints = 100;
            payBand1.Salary = 3000;
            payBand1.Parent = parent;
            parent.PayBands.Add(payBand1);

            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Validate_PayBandWithEmptyName_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand = new RankHierarchyBuilder()
                .WithName("")
                .Build();
            payBand.RequiredPoints = 100;
            payBand.Salary = 3000;
            payBand.Parent = parent;
            parent.PayBands.Add(payBand);

            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        #endregion

        #region Real-Time Context Validation

        [Fact]
        public void Validate_RealTimeContext_SkipsExpensiveChecks()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            var rank2 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert - real-time validation should skip duplicate name checks
            // so it should not find the duplicate error
            result.Issues.Should().NotContain(i => i.Message.Contains("duplicate"));
        }

        #endregion

        #region ValidateSingleRank Tests

        [Fact]
        public void ValidateSingleRank_EmptyName_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        [Fact]
        public void ValidateSingleRank_DuplicateName_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank1, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("Duplicate"));
        }

        [Fact]
        public void ValidateSingleRank_UniqueName_PassesValidation()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank1, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeFalse();
            result.Issues.Should().NotContain(i => i.Message.Contains("Duplicate"));
        }

        [Fact]
        public void ValidateSingleRank_ParentWithEmptyPayBandName_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Detective").Build();
            parent.IsParent = true;
            var payBand = new RankHierarchyBuilder().WithName("").Build();
            payBand.Parent = parent;
            parent.PayBands.Add(payBand);
            var allRanks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(parent, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("Pay band name cannot be empty"));
        }

        [Fact]
        public void ValidateSingleRank_ParentWithDuplicatePayBandNames_ReturnsWarning()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Detective").Build();
            parent.IsParent = true;
            var payBand1 = new RankHierarchyBuilder().WithName("Detective I").Build();
            var payBand2 = new RankHierarchyBuilder().WithName("Detective I").Build();
            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);
            var allRanks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(parent, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("Duplicate pay band"));
        }

        [Fact]
        public void ValidateSingleRank_CaseInsensitiveDuplicate_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("OFFICER").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank1, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
        }

        #endregion

        #region ValidateProperty Tests

        [Fact]
        public void ValidateProperty_NameProperty_EmptyValue_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "Name", "", allRanks, result, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        [Fact]
        public void ValidateProperty_NameProperty_WhitespaceValue_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "Name", "   ", allRanks, result, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        [Fact]
        public void ValidateProperty_NameProperty_DuplicateValue_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act - Change rank2's name to "Officer" (duplicate of rank1)
            _rule.ValidateProperty(rank2, "Name", "Officer", allRanks, result, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("Duplicate"));
        }

        [Fact]
        public void ValidateProperty_NameProperty_UniqueValue_PassesValidation()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act - Change rank2's name to "Sergeant" (unique)
            _rule.ValidateProperty(rank2, "Name", "Sergeant", allRanks, result, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
        }

        [Fact]
        public void ValidateProperty_NameProperty_CaseInsensitiveDuplicate_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act - Change rank2's name to "OFFICER" (case-insensitive duplicate)
            _rule.ValidateProperty(rank2, "Name", "OFFICER", allRanks, result, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("Duplicate"));
        }

        [Fact]
        public void ValidateProperty_NonNameProperty_DoesNotValidate()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act - Validate a non-Name property
            _rule.ValidateProperty(rank, "Salary", 5000, allRanks, result, _mockDataService.Object);

            // Assert - Should not add any issues
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
        }

        [Fact]
        public void ValidateProperty_NameProperty_NullValue_DoesNotThrow()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            var act = () => _rule.ValidateProperty(rank, "Name", null!, allRanks, result, _mockDataService.Object);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateProperty_NameProperty_NonStringValue_DoesNotValidate()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act - Pass a non-string value
            _rule.ValidateProperty(rank, "Name", 12345, allRanks, result, _mockDataService.Object);

            // Assert - Should not add any issues (type guard fails)
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
        }

        #endregion

        #region Parent Rank with Empty Name Tests

        [Fact]
        public void Validate_ParentRankWithEmptyName_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("").Build();
            parent.IsParent = true;
            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("cannot be empty"));
        }

        [Fact]
        public void Validate_ParentRankWithDuplicatePayBandNames_ReturnsWarning()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Detective").Build();
            parent.IsParent = true;
            var payBand1 = new RankHierarchyBuilder().WithName("Detective I").Build();
            var payBand2 = new RankHierarchyBuilder().WithName("Detective I").Build();
            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);
            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("Duplicate pay band"));
        }

        #endregion

        #region FlattenRanks Edge Cases

        [Fact]
        public void Validate_MixedParentAndRegularRanks_FlattenCorrectly()
        {
            // Arrange - Create a mix of parent and regular ranks
            var regularRank = new RankHierarchyBuilder().WithName("Officer").Build();
            regularRank.RequiredPoints = 0;
            regularRank.Salary = 1000;

            var parentRank = new RankHierarchyBuilder().WithName("Detective").Build();
            parentRank.IsParent = true;
            var payBand = new RankHierarchyBuilder().WithName("Detective I").Build();
            payBand.RequiredPoints = 100;
            payBand.Salary = 3000;
            parentRank.PayBands.Add(payBand);

            var ranks = new List<RankHierarchy> { regularRank, parentRank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Should validate both regular rank and pay band, not parent
            result.Issues.Should().NotContain(i => i.RankName == "Detective" && i.Message.Contains("duplicate"));
        }

        [Fact]
        public void Validate_ParentRankWithNoPayBands_IncludesParentInValidation()
        {
            // Arrange - Parent rank with IsParent=true but no pay bands
            var parentRank = new RankHierarchyBuilder().WithName("Detective").Build();
            parentRank.IsParent = true;
            parentRank.RequiredPoints = 100;
            parentRank.Salary = 3000;
            // No pay bands added

            var ranks = new List<RankHierarchy> { parentRank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Should include parent rank itself since it has no pay bands
            result.HasErrors.Should().BeFalse();
        }

        #endregion
    }
}
