using System;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Tests for ValidationDismissalService - dismissal of validation warnings and advisories
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class ValidationDismissalServiceTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly SettingsManager _settingsManager;
        private readonly ValidationDismissalService _service;

        public ValidationDismissalServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"ValidationDismissalTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);

            var settingsPath = Path.Combine(_tempDirectory, "test_settings.ini");
            _settingsManager = new SettingsManager(settingsPath);
            _service = new ValidationDismissalService(_settingsManager);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, recursive: true);
                }
            }
            catch { /* Best effort cleanup */ }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            // Act
            var service = new ValidationDismissalService(_settingsManager);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region IsDismissed Tests

        [Fact]
        public void IsDismissed_WithNewIssue_ReturnsFalse()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = "rank1",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            // Act
            var isDismissed = _service.IsDismissed(issue);

            // Assert
            isDismissed.Should().BeFalse();
        }

        [Fact]
        public void IsDismissed_AfterDismissal_ReturnsTrue()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = "rank1",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            // Act
            _service.Dismiss(issue);
            var isDismissed = _service.IsDismissed(issue);

            // Assert
            isDismissed.Should().BeTrue();
        }

        [Fact]
        public void IsDismissed_WithNullRankId_ReturnsFalse()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = null,
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            // Act
            var isDismissed = _service.IsDismissed(issue);

            // Assert
            isDismissed.Should().BeFalse("issues without RankId cannot be dismissed");
        }

        #endregion

        #region Dismiss Tests

        [Fact]
        public void Dismiss_MarksIssueAsDismissed()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = "rank2",
                Category = "Salary",
                Message = "Salary is low",
                Severity = ValidationSeverity.Advisory
            };

            // Act
            _service.Dismiss(issue);

            // Assert
            _service.IsDismissed(issue).Should().BeTrue();
        }

        [Fact]
        public void Dismiss_PersistsAcrossInstances()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = "rank3",
                Category = "XP",
                Message = "XP progression issue",
                Severity = ValidationSeverity.Warning
            };

            // Act
            _service.Dismiss(issue);
            _settingsManager.Save();

            // Create new instance with same settings
            var newService = new ValidationDismissalService(_settingsManager);

            // Assert
            newService.IsDismissed(issue).Should().BeTrue("dismissal should persist");
        }

        [Fact]
        public void Dismiss_WithNullRankId_DoesNotCrash()
        {
            // Arrange
            var issue = new ValidationIssue
            {
                RankId = null,
                Category = "XP",
                Message = "Test",
                Severity = ValidationSeverity.Warning
            };

            // Act
            var act = () => _service.Dismiss(issue);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Dismiss_WithComponentParameters_Works()
        {
            // Arrange
            string rankId = "rank4";
            string category = "Vehicles";
            string itemName = "police";
            string message = "Vehicle not found";

            // Act
            _service.Dismiss(rankId, category, itemName, message);

            // Verify by creating matching issue
            var issue = new ValidationIssue
            {
                RankId = rankId,
                Category = category,
                ItemName = itemName,
                Message = message,
                Severity = ValidationSeverity.Warning
            };

            // Assert
            _service.IsDismissed(issue).Should().BeTrue();
        }

        #endregion

        #region ClearAll Tests

        [Fact]
        public void ClearAll_RemovesAllDismissals()
        {
            // Arrange
            var issue1 = new ValidationIssue
            {
                RankId = "rank5",
                Category = "XP",
                Message = "Issue 1",
                Severity = ValidationSeverity.Warning
            };
            var issue2 = new ValidationIssue
            {
                RankId = "rank6",
                Category = "Salary",
                Message = "Issue 2",
                Severity = ValidationSeverity.Advisory
            };

            _service.Dismiss(issue1);
            _service.Dismiss(issue2);

            // Act
            _service.ClearAll();

            // Assert
            _service.IsDismissed(issue1).Should().BeFalse();
            _service.IsDismissed(issue2).Should().BeFalse();
        }

        #endregion

        #region Unique Key Generation Tests

        [Fact]
        public void IsDismissed_WithSameIssueContent_ReturnsSameResult()
        {
            // Arrange - Create two issues with identical content
            var issue1 = new ValidationIssue
            {
                RankId = "rank7",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            var issue2 = new ValidationIssue
            {
                RankId = "rank7",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            // Act
            _service.Dismiss(issue1);

            // Assert
            _service.IsDismissed(issue2).Should().BeTrue(
                "issues with identical content should be treated as the same");
        }

        [Fact]
        public void IsDismissed_WithDifferentRank_ReturnsDifferentResult()
        {
            // Arrange
            var issue1 = new ValidationIssue
            {
                RankId = "rank8",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };
            var issue2 = new ValidationIssue
            {
                RankId = "rank9",
                Category = "XP",
                Message = "XP is 0",
                Severity = ValidationSeverity.Warning
            };

            // Act
            _service.Dismiss(issue1);

            // Assert
            _service.IsDismissed(issue2).Should().BeFalse(
                "issues for different ranks should be treated separately");
        }

        [Fact]
        public void IsDismissed_WithDifferentCategory_ReturnsDifferentResult()
        {
            // Arrange
            var issue1 = new ValidationIssue
            {
                RankId = "rank10",
                Category = "XP",
                Message = "Issue",
                Severity = ValidationSeverity.Warning
            };
            var issue2 = new ValidationIssue
            {
                RankId = "rank10",
                Category = "Salary",
                Message = "Issue",
                Severity = ValidationSeverity.Warning
            };

            // Act
            _service.Dismiss(issue1);

            // Assert
            _service.IsDismissed(issue2).Should().BeFalse(
                "issues for different categories should be treated separately");
        }

        #endregion
    }
}
