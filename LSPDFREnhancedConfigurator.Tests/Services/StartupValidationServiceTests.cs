using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class StartupValidationServiceTests : IDisposable
    {
        private readonly List<string> _tempDirectories = new List<string>();

        public void Dispose()
        {
            // Cleanup temp directories
            foreach (var dir in _tempDirectories)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        private string CreateTempGtaDirectory(bool includeGta5Exe = true, bool includePlugins = true,
            bool includeLspdfr = true, bool includeLspdf​rEnhanced = true)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"gta_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            _tempDirectories.Add(tempDir);

            if (includeGta5Exe)
            {
                File.WriteAllText(Path.Combine(tempDir, "GTA5.exe"), "fake exe");
            }

            if (includePlugins)
            {
                var pluginsDir = Path.Combine(tempDir, "plugins");
                Directory.CreateDirectory(pluginsDir);

                if (includeLspdfr)
                {
                    var lspdfr​Dir = Path.Combine(pluginsDir, "LSPDFR");
                    Directory.CreateDirectory(lspdfr​Dir);

                    if (includeLspdf​rEnhanced)
                    {
                        var enhancedDir = Path.Combine(lspdfr​Dir, "LSPDFR Enhanced");
                        Directory.CreateDirectory(enhancedDir);
                    }
                }
            }

            return tempDir;
        }

        #region ValidateGtaDirectory Tests

        [Fact]
        public void ValidateGtaDirectory_NullPath_ReturnsError()
        {
            // Act
            var result = StartupValidationService.ValidateGtaDirectory(null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Error);
            result.ErrorMessage.Should().Contain("not configured");
        }

        [Fact]
        public void ValidateGtaDirectory_EmptyPath_ReturnsError()
        {
            // Act
            var result = StartupValidationService.ValidateGtaDirectory("");

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Error);
            result.ErrorMessage.Should().Contain("not configured");
        }

        [Fact]
        public void ValidateGtaDirectory_WhitespacePath_ReturnsError()
        {
            // Act
            var result = StartupValidationService.ValidateGtaDirectory("   ");

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Error);
            result.ErrorMessage.Should().Contain("not configured");
        }

        [Fact]
        public void ValidateGtaDirectory_NonExistentPath_ReturnsError()
        {
            // Arrange
            var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(fakePath);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Error);
            result.ErrorMessage.Should().Contain("does not exist");
            result.ErrorMessage.Should().Contain(fakePath);
        }

        [Fact]
        public void ValidateGtaDirectory_MissingGTA5Exe_ReturnsError()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory(includeGta5Exe: false);

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Error);
            result.ErrorMessage.Should().Contain("GTA5.exe not found");
            result.MissingFiles.Should().Contain("GTA5.exe");
        }

        [Fact]
        public void ValidateGtaDirectory_MissingPluginsFolder_ReturnsWarning()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory(includePlugins: false);

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Warning);
            result.ErrorMessage.Should().Contain("plugins");
            result.ErrorMessage.Should().Contain("RAGE Plugin Hook");
            result.MissingFolders.Should().Contain("plugins");
        }

        [Fact]
        public void ValidateGtaDirectory_MissingLSPDFRFolder_ReturnsWarning()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory(includeLspdfr: false);

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Warning);
            result.ErrorMessage.Should().Contain("LSPDFR");
            result.ErrorMessage.Should().Contain("plugins\\LSPDFR");
            result.MissingFolders.Should().Contain("plugins\\LSPDFR");
        }

        [Fact]
        public void ValidateGtaDirectory_MissingLSPDFREnhancedFolder_ReturnsWarning()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory(includeLspdf​rEnhanced: false);

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(GtaValidationSeverity.Warning);
            result.ErrorMessage.Should().Contain("LSPDFR Enhanced");
            result.ErrorMessage.Should().Contain("plugins\\LSPDFR\\LSPDFR Enhanced");
            result.MissingFolders.Should().Contain("plugins\\LSPDFR\\LSPDFR Enhanced");
        }

        [Fact]
        public void ValidateGtaDirectory_ValidDirectory_ReturnsSuccess()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory();

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Severity.Should().Be(GtaValidationSeverity.Success);
            result.ErrorMessage.Should().BeNull();
            result.MissingFiles.Should().BeEmpty();
            result.MissingFolders.Should().BeEmpty();
        }

        [Fact]
        public void ValidateGtaDirectory_ValidDirectory_NoErrorMessage()
        {
            // Arrange
            var gtaDir = CreateTempGtaDirectory();

            // Act
            var result = StartupValidationService.ValidateGtaDirectory(gtaDir);

            // Assert
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        #endregion

        #region GtaDirectoryValidation Model Tests

        [Fact]
        public void GtaDirectoryValidation_DefaultConstructor_SetsDefaults()
        {
            // Act
            var validation = new GtaDirectoryValidation();

            // Assert
            validation.IsValid.Should().BeTrue();
            validation.Severity.Should().Be(GtaValidationSeverity.Success);
            validation.ErrorMessage.Should().BeNull();
            validation.MissingFiles.Should().NotBeNull();
            validation.MissingFolders.Should().NotBeNull();
        }

        [Fact]
        public void GtaDirectoryValidation_MissingFiles_IsModifiable()
        {
            // Arrange
            var validation = new GtaDirectoryValidation();

            // Act
            validation.MissingFiles.Add("test.exe");

            // Assert
            validation.MissingFiles.Should().Contain("test.exe");
        }

        [Fact]
        public void GtaDirectoryValidation_MissingFolders_IsModifiable()
        {
            // Arrange
            var validation = new GtaDirectoryValidation();

            // Act
            validation.MissingFolders.Add("test_folder");

            // Assert
            validation.MissingFolders.Should().Contain("test_folder");
        }

        #endregion

        #region ValidateRanks Tests

        [Fact]
        public void ValidateRanks_EmptyList_CallsValidationService()
        {
            // Arrange
            var mockDataService = new MockServiceBuilder().BuildMock();
            var service = new StartupValidationService(mockDataService.Object);
            var ranks = new List<RankHierarchy>();

            // Act
            var result = service.ValidateRanks(ranks);

            // Assert - should not throw and should return a result
            result.Should().NotBeNull();
        }

        [Fact]
        public void ValidateRanks_WithRanks_CallsValidationService()
        {
            // Arrange
            var mockDataService = new MockServiceBuilder().BuildMock();
            var service = new StartupValidationService(mockDataService.Object);
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var result = service.ValidateRanks(ranks);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void ValidationService_Property_ReturnsValidationService()
        {
            // Arrange
            var mockDataService = new MockServiceBuilder().BuildMock();
            var service = new StartupValidationService(mockDataService.Object);

            // Act
            var validationService = service.ValidationService;

            // Assert
            validationService.Should().NotBeNull();
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDataService_CreatesInstance()
        {
            // Arrange
            var mockDataService = new MockServiceBuilder().BuildMock();

            // Act
            var service = new StartupValidationService(mockDataService.Object);

            // Assert
            service.Should().NotBeNull();
            service.ValidationService.Should().NotBeNull();
        }

        #endregion

        #region GtaValidationSeverity Enum Tests

        [Fact]
        public void GtaValidationSeverity_HasExpectedValues()
        {
            // Assert
            Enum.GetNames(typeof(GtaValidationSeverity)).Should().Contain(new[] { "Success", "Warning", "Error" });
        }

        [Fact]
        public void GtaValidationSeverity_Success_IsZero()
        {
            // Assert
            ((int)GtaValidationSeverity.Success).Should().Be(0);
        }

        [Fact]
        public void GtaValidationSeverity_Warning_IsOne()
        {
            // Assert
            ((int)GtaValidationSeverity.Warning).Should().Be(1);
        }

        [Fact]
        public void GtaValidationSeverity_Error_IsTwo()
        {
            // Assert
            ((int)GtaValidationSeverity.Error).Should().Be(2);
        }

        #endregion
    }
}
