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
    public class RankProgressionRuleTests
    {
        private readonly RankProgressionRule _rule;
        private readonly Mock<DataLoadingService> _mockDataService;

        public RankProgressionRuleTests()
        {
            _rule = new RankProgressionRule();
            _mockDataService = new MockServiceBuilder().BuildMock();
        }

        #region Empty Ranks Validation

        [Fact]
        public void Validate_EmptyRanksList_ReturnsError()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("No ranks defined") &&
                i.Severity == ValidationSeverity.Error);
        }

        #endregion

        #region First Rank Validation

        [Fact]
        public void Validate_FirstRankNotStartingAtZero_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 100;
            rank.Salary = 1000;
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("must start at XP 0") &&
                i.RankName == "Officer" &&
                i.IsAutoFixable == true);
        }

        [Fact]
        public void Validate_FirstRankStartingAtZero_PassesValidation()
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
            result.Issues.Should().NotContain(i => i.Message.Contains("must start at XP 0"));
        }

        #endregion

        #region XP Progression Validation

        [Fact]
        public void Validate_NonIncreasingXP_ReturnsError()
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
            rank2.RequiredPoints = 0; // Same as previous
            rank2.Salary = 2000;

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("must have Required Points greater than") &&
                i.RankName == "Detective" &&
                i.PropertyName == "RequiredPoints");
        }

        [Fact]
        public void Validate_DecreasingXP_ReturnsError()
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
            rank2.RequiredPoints = -50; // Less than previous
            rank2.Salary = 2000;

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("must have Required Points greater than"));
        }

        [Fact]
        public void Validate_IncreasingXP_PassesValidation()
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
            result.Issues.Should().NotContain(i => i.Message.Contains("must have Required Points greater than"));
        }

        #endregion

        #region Negative Values Validation

        [Fact]
        public void Validate_NegativeRequiredPoints_ReturnsError()
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

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("negative Required Points") &&
                i.RankName == "Officer" &&
                i.PropertyName == "RequiredPoints" &&
                i.IsAutoFixable == true);
        }

        [Fact]
        public void Validate_NegativeSalary_ReturnsError()
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

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("negative Salary") &&
                i.RankName == "Officer" &&
                i.PropertyName == "Salary" &&
                i.IsAutoFixable == true);
        }

        [Fact]
        public void Validate_PositiveValues_PassesValidation()
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
            result.Issues.Should().NotContain(i => i.Message.Contains("negative"));
        }

        #endregion

        #region Salary Progression Validation

        [Fact]
        public void Validate_DecreasingSalaryFullContext_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 2000;

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 1000; // Lower than previous

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("lower salary") &&
                i.RankName == "Detective" &&
                i.PropertyName == "Salary");
        }

        [Fact]
        public void Validate_DecreasingSalaryRealTimeContext_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 2000;

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 1000; // Lower than previous

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Message.Contains("lower salary") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_IncreasingSalary_PassesValidation()
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
            result.Issues.Should().NotContain(i => i.Message.Contains("lower salary"));
        }

        #endregion

        #region Pay Band Structure Validation

        [Fact]
        public void Validate_ParentRankWithLessThanTwoPayBands_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand.RequiredPoints = 0;
            payBand.Salary = 3000;
            payBand.Parent = parent;
            parent.PayBands.Add(payBand);

            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("must have at least 2 pay bands") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_ParentRankWithTwoPayBands_PassesValidation()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand1 = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand1.RequiredPoints = 0;
            payBand1.Salary = 3000;
            payBand1.Parent = parent;

            var payBand2 = new RankHierarchyBuilder()
                .WithName("Detective II")
                .Build();
            payBand2.RequiredPoints = 100;
            payBand2.Salary = 4000;
            payBand2.Parent = parent;

            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);

            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i => i.Message.Contains("must have at least 2 pay bands"));
        }

        [Fact]
        public void Validate_PayBandsWithNonIncreasingXP_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand1 = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand1.RequiredPoints = 0;
            payBand1.Salary = 3000;
            payBand1.Parent = parent;

            var payBand2 = new RankHierarchyBuilder()
                .WithName("Detective II")
                .Build();
            payBand2.RequiredPoints = 0; // Same as previous
            payBand2.Salary = 4000;
            payBand2.Parent = parent;

            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);

            var ranks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("must have higher XP than") &&
                i.RankName == "Detective II" &&
                i.PropertyName == "RequiredPoints");
        }

        #endregion

        #region ValidateSingleRank Tests

        [Fact]
        public void ValidateSingleRank_NegativeRequiredPoints_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = -100;
            rank.Salary = 1000;
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("cannot be negative") &&
                i.PropertyName == "RequiredPoints" &&
                i.IsAutoFixable == true);
        }

        [Fact]
        public void ValidateSingleRank_NegativeSalary_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = -1000;
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("cannot be negative") &&
                i.PropertyName == "Salary" &&
                i.IsAutoFixable == true);
        }

        [Fact]
        public void ValidateSingleRank_XPNotGreaterThanPrevious_ReturnsError()
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
            rank2.RequiredPoints = 0; // Same as previous
            rank2.Salary = 2000;

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("must be greater than previous rank") &&
                i.PropertyName == "RequiredPoints");
        }

        [Fact]
        public void ValidateSingleRank_SalaryLowerThanPreviousFullContext_ReturnsWarning()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 2000;

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 1000; // Lower than previous

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasWarnings.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Warning &&
                i.Message.Contains("lower than previous") &&
                i.PropertyName == "Salary");
        }

        [Fact]
        public void ValidateSingleRank_SalaryLowerThanPreviousRealTimeContext_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 2000;

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 1000; // Lower than previous

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.RealTime, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Message.Contains("lower than previous") &&
                i.PropertyName == "Salary");
        }

        [Fact]
        public void ValidateSingleRank_ParentRankWithLessThanTwoPayBands_ReturnsError()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand.RequiredPoints = 0;
            payBand.Salary = 3000;
            payBand.Parent = parent;
            parent.PayBands.Add(payBand);

            var allRanks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(parent, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("must have at least 2 pay bands"));
        }

        [Fact]
        public void ValidateSingleRank_ParentRankWithPayBands_SkipsNumericValidation()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;
            parent.RequiredPoints = -100; // Negative value
            parent.Salary = -1000; // Negative value

            var payBand1 = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand1.RequiredPoints = 0;
            payBand1.Salary = 3000;
            payBand1.Parent = parent;

            var payBand2 = new RankHierarchyBuilder()
                .WithName("Detective II")
                .Build();
            payBand2.RequiredPoints = 100;
            payBand2.Salary = 4000;
            payBand2.Parent = parent;

            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);

            var allRanks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(parent, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Should not validate negative values for parent ranks with pay bands
            result.Issues.Should().NotContain(i => i.Message.Contains("cannot be negative"));
        }

        #endregion

        #region ValidateProperty Tests

        [Fact]
        public void ValidateProperty_NegativeRequiredPoints_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "RequiredPoints", -100, allRanks, result, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("cannot be negative") &&
                i.PropertyName == "RequiredPoints");
        }

        [Fact]
        public void ValidateProperty_NegativeSalary_ReturnsError()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "Salary", -1000, allRanks, result, _mockDataService.Object);

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i =>
                i.Message.Contains("cannot be negative") &&
                i.PropertyName == "Salary");
        }

        [Fact]
        public void ValidateProperty_PositiveRequiredPoints_PassesValidation()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "RequiredPoints", 100, allRanks, result, _mockDataService.Object);

            // Assert
            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_PositiveSalary_PassesValidation()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "Salary", 1000, allRanks, result, _mockDataService.Object);

            // Assert
            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void ValidateProperty_UnrelatedProperty_DoesNotValidate()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateProperty(rank, "SomeOtherProperty", "value", allRanks, result, _mockDataService.Object);

            // Assert
            result.Issues.Should().BeEmpty();
        }

        #endregion
    }
}
