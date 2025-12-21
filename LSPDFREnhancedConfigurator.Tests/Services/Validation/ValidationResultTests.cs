using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services.Validation
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Validation")]
    public class ValidationResultTests
    {
        #region Property Tests

        [Fact]
        public void IsValid_NoErrors_ReturnsTrue()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));

            // Act & Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithErrors_ReturnsFalse()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act & Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void HasErrors_WithErrors_ReturnsTrue()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act & Assert
            result.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void HasWarnings_WithWarnings_ReturnsTrue()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));

            // Act & Assert
            result.HasWarnings.Should().BeTrue();
        }

        [Fact]
        public void HasAdvisories_WithAdvisories_ReturnsTrue()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act & Assert
            result.HasAdvisories.Should().BeTrue();
        }

        [Fact]
        public void HasIssues_WithAnyIssue_ReturnsTrue()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act & Assert
            result.HasIssues.Should().BeTrue();
        }

        [Fact]
        public void HasIssues_NoIssues_ReturnsFalse()
        {
            // Arrange
            var result = new ValidationResult();

            // Act & Assert
            result.HasIssues.Should().BeFalse();
        }

        [Fact]
        public void ErrorCount_ReturnsCorrectCount()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));

            // Act & Assert
            result.ErrorCount.Should().Be(2);
        }

        [Fact]
        public void WarningCount_ReturnsCorrectCount()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act & Assert
            result.WarningCount.Should().Be(3);
        }

        [Fact]
        public void AdvisoryCount_ReturnsCorrectCount()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act & Assert
            result.AdvisoryCount.Should().Be(2);
        }

        [Fact]
        public void Errors_ReturnsOnlyErrors()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            var errors = result.Errors.ToList();

            // Assert
            errors.Should().HaveCount(1);
            errors[0].Severity.Should().Be(ValidationSeverity.Error);
        }

        [Fact]
        public void Warnings_ReturnsOnlyWarnings()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));

            // Act
            var warnings = result.Warnings.ToList();

            // Assert
            warnings.Should().HaveCount(2);
            warnings.Should().OnlyContain(w => w.Severity == ValidationSeverity.Warning);
        }

        [Fact]
        public void Advisories_ReturnsOnlyAdvisories()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act
            var advisories = result.Advisories.ToList();

            // Assert
            advisories.Should().HaveCount(2);
            advisories.Should().OnlyContain(a => a.Severity == ValidationSeverity.Advisory);
        }

        #endregion

        #region FilterBySeverity Tests

        [Fact]
        public void FilterBySeverity_Error_ReturnsOnlyErrors()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            var filtered = result.FilterBySeverity(ValidationSeverity.Error);

            // Assert
            filtered.Issues.Should().HaveCount(1);
            filtered.Issues[0].Severity.Should().Be(ValidationSeverity.Error);
        }

        [Fact]
        public void FilterBySeverity_Warning_IncludesErrorsAndWarnings()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            var filtered = result.FilterBySeverity(ValidationSeverity.Warning);

            // Assert
            filtered.Issues.Should().HaveCount(2);
            filtered.HasErrors.Should().BeTrue();
            filtered.HasWarnings.Should().BeTrue();
            filtered.HasAdvisories.Should().BeFalse();
        }

        [Fact]
        public void FilterBySeverity_Advisory_ReturnsAll()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            var filtered = result.FilterBySeverity(ValidationSeverity.Advisory);

            // Assert
            filtered.Issues.Should().HaveCount(3);
        }

        #endregion

        #region GetIssuesForRank Tests

        [Fact]
        public void GetIssuesForRank_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, rankId: "rank1"));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning, rankId: "rank1"));
            result.AddIssue(CreateIssue(ValidationSeverity.Error, rankId: "rank2"));

            // Act
            var rankIssues = result.GetIssuesForRank("rank1").ToList();

            // Assert
            rankIssues.Should().HaveCount(2);
            rankIssues.Should().OnlyContain(i => i.RankId == "rank1");
        }

        [Fact]
        public void GetIssuesForRank_NoMatchingIssues_ReturnsEmpty()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, rankId: "rank1"));

            // Act
            var rankIssues = result.GetIssuesForRank("rank999").ToList();

            // Assert
            rankIssues.Should().BeEmpty();
        }

        #endregion

        #region GetIssuesByCategory Tests

        [Fact]
        public void GetIssuesByCategory_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, category: "Rank"));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning, category: "Vehicle"));
            result.AddIssue(CreateIssue(ValidationSeverity.Error, category: "Rank"));

            // Act
            var categoryIssues = result.GetIssuesByCategory("Rank").ToList();

            // Assert
            categoryIssues.Should().HaveCount(2);
            categoryIssues.Should().OnlyContain(i => i.Category == "Rank");
        }

        [Fact]
        public void GetIssuesByCategory_CaseInsensitive_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, category: "Rank"));

            // Act
            var categoryIssues = result.GetIssuesByCategory("RANK").ToList();

            // Assert
            categoryIssues.Should().HaveCount(1);
        }

        #endregion

        #region GetIssuesByRule Tests

        [Fact]
        public void GetIssuesByRule_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, ruleId: "RULE_001"));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning, ruleId: "RULE_002"));
            result.AddIssue(CreateIssue(ValidationSeverity.Error, ruleId: "RULE_001"));

            // Act
            var ruleIssues = result.GetIssuesByRule("RULE_001").ToList();

            // Assert
            ruleIssues.Should().HaveCount(2);
            ruleIssues.Should().OnlyContain(i => i.RuleId == "RULE_001");
        }

        [Fact]
        public void GetIssuesByRule_CaseInsensitive_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, ruleId: "RULE_001"));

            // Act
            var ruleIssues = result.GetIssuesByRule("rule_001").ToList();

            // Assert
            ruleIssues.Should().HaveCount(1);
        }

        #endregion

        #region GetAutoFixableIssues Tests

        [Fact]
        public void GetAutoFixableIssues_ReturnsOnlyAutoFixable()
        {
            // Arrange
            var result = new ValidationResult();
            var fixable = CreateIssue(ValidationSeverity.Error);
            fixable.IsAutoFixable = true;
            fixable.AutoFixAction = () => { };
            result.AddIssue(fixable);
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act
            var autoFixable = result.GetAutoFixableIssues().ToList();

            // Assert
            autoFixable.Should().HaveCount(1);
            autoFixable[0].IsAutoFixable.Should().BeTrue();
            autoFixable[0].AutoFixAction.Should().NotBeNull();
        }

        [Fact]
        public void GetAutoFixableIssues_RequiresBothFlagAndAction_FiltersCorrectly()
        {
            // Arrange
            var result = new ValidationResult();
            var onlyFlag = CreateIssue(ValidationSeverity.Error);
            onlyFlag.IsAutoFixable = true; // Flag set but no action
            result.AddIssue(onlyFlag);

            // Act
            var autoFixable = result.GetAutoFixableIssues().ToList();

            // Assert
            autoFixable.Should().BeEmpty(); // Needs both flag AND action
        }

        #endregion

        #region Merge Tests

        [Fact]
        public void Merge_CombinesIssues()
        {
            // Arrange
            var result1 = new ValidationResult();
            result1.AddIssue(CreateIssue(ValidationSeverity.Error));
            result1.AddIssue(CreateIssue(ValidationSeverity.Warning));

            var result2 = new ValidationResult();
            result2.AddIssue(CreateIssue(ValidationSeverity.Error));
            result2.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            result1.Merge(result2);

            // Assert
            result1.Issues.Should().HaveCount(4);
            result1.ErrorCount.Should().Be(2);
            result1.WarningCount.Should().Be(1);
            result1.AdvisoryCount.Should().Be(1);
        }

        #endregion

        #region GetSummary Tests

        [Fact]
        public void GetSummary_NoIssues_ReturnsSuccessMessage()
        {
            // Arrange
            var result = new ValidationResult();

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("No validation issues");
        }

        [Fact]
        public void GetSummary_WithErrors_IncludesErrorSection()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error, message: "Test error 1"));
            result.AddIssue(CreateIssue(ValidationSeverity.Error, message: "Test error 2"));

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("2 Error(s) Found");
            summary.Should().Contain("Test error 1");
            summary.Should().Contain("Test error 2");
        }

        [Fact]
        public void GetSummary_WithWarnings_IncludesWarningSection()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Warning, message: "Test warning"));

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("1 Warning(s) Found");
            summary.Should().Contain("Test warning");
        }

        [Fact]
        public void GetSummary_WithAdvisories_IncludesAdvisorySection()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory, message: "Test advisory"));

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("1 Advisory Notice(s)");
            summary.Should().Contain("Test advisory");
        }

        [Fact]
        public void GetSummary_MoreThan10Errors_ShowsTruncationMessage()
        {
            // Arrange
            var result = new ValidationResult();
            for (int i = 0; i < 15; i++)
            {
                result.AddIssue(CreateIssue(ValidationSeverity.Error, message: $"Error {i}"));
            }

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("15 Error(s) Found");
            summary.Should().Contain("and 5 more error(s)");
        }

        [Fact]
        public void GetSummary_MoreThan5Advisories_ShowsTruncationMessage()
        {
            // Arrange
            var result = new ValidationResult();
            for (int i = 0; i < 8; i++)
            {
                result.AddIssue(CreateIssue(ValidationSeverity.Advisory, message: $"Advisory {i}"));
            }

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("8 Advisory Notice(s)");
            summary.Should().Contain("and 3 more advisory notice(s)");
        }

        #endregion

        #region GetCompactSummary Tests

        [Fact]
        public void GetCompactSummary_NoIssues_ReturnsNoIssues()
        {
            // Arrange
            var result = new ValidationResult();

            // Act
            var summary = result.GetCompactSummary();

            // Assert
            summary.Should().Be("No issues");
        }

        [Fact]
        public void GetCompactSummary_OnlyErrors_ShowsErrors()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Error));

            // Act
            var summary = result.GetCompactSummary();

            // Assert
            summary.Should().Be("2 error(s)");
        }

        [Fact]
        public void GetCompactSummary_MultipleTypes_ShowsAll()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddIssue(CreateIssue(ValidationSeverity.Error));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Warning));
            result.AddIssue(CreateIssue(ValidationSeverity.Advisory));

            // Act
            var summary = result.GetCompactSummary();

            // Assert
            summary.Should().Be("1 error(s), 2 warning(s), 1 advisory notice(s)");
        }

        #endregion

        #region Helper Methods

        private ValidationIssue CreateIssue(
            ValidationSeverity severity,
            string message = "Test message",
            string rankId = "rank1",
            string category = "Test",
            string ruleId = "TEST_RULE")
        {
            return new ValidationIssue
            {
                Severity = severity,
                Message = message,
                RankId = rankId,
                Category = category,
                RuleId = ruleId
            };
        }

        #endregion
    }
}
