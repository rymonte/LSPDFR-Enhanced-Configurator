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
        public void Validate_RankWithNegativeRequiredPoints_ReturnsError()
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
            result.Issues.Should().Contain(i => i.Message.Contains("Required points cannot be negative"));
        }

        [Fact]
        public void Validate_RankWithNegativeSalary_ReturnsError()
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
            result.Issues.Should().Contain(i => i.Message.Contains("Salary cannot be negative"));
        }

        #endregion

        #region Duplicate Name Validation

        [Fact]
        public void Validate_DuplicateRankNames_ReturnsError()
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

            // Assert
            result.HasErrors.Should().BeTrue();
            result.Issues.Should().Contain(i => i.Message.Contains("duplicate") && i.Message.Contains("Officer"));
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
    }
}
