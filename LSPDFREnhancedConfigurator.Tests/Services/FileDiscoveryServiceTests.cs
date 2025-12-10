using System;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Tests for FileDiscoveryService - discovering LSPDFR XML files
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class FileDiscoveryServiceTests : IDisposable
    {
        private readonly string _tempDirectory;

        public FileDiscoveryServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"FileDiscoveryTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
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

        #region FindAgencyFiles Tests

        [Fact]
        public void FindAgencyFiles_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindAgencyFiles();

            // Assert
            files.Should().BeEmpty("no agency files exist");
        }

        [Fact]
        public void FindAgencyFiles_WithAgencyXml_FindsFile()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "agency.xml"), "<?xml version=\"1.0\"?><Agencies></Agencies>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindAgencyFiles();

            // Assert
            files.Should().HaveCount(1, "one agency file exists");
            files[0].Should().EndWith("agency.xml");
        }

        [Fact]
        public void FindAgencyFiles_WithMultipleAgencyFiles_FindsAll()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "agency.xml"), "<?xml version=\"1.0\"?><Agencies></Agencies>");
            File.WriteAllText(Path.Combine(lspdfData, "agency_custom.xml"), "<?xml version=\"1.0\"?><Agencies></Agencies>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindAgencyFiles();

            // Assert
            files.Should().HaveCountGreaterOrEqualTo(2, "multiple agency files exist");
        }

        [Fact]
        public void FindAgencyFiles_WithSubdirectories_FindsRecursively()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            var customDir = Path.Combine(lspdfData, "custom");
            Directory.CreateDirectory(customDir);
            File.WriteAllText(Path.Combine(customDir, "agency_custom.xml"), "<?xml version=\"1.0\"?><Agencies></Agencies>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindAgencyFiles();

            // Assert
            files.Should().HaveCount(1, "agency file in subdirectory should be found");
            files[0].Should().Contain("custom");
        }

        #endregion

        #region FindStationFiles Tests

        [Fact]
        public void FindStationFiles_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindStationFiles();

            // Assert
            files.Should().BeEmpty("no station files exist");
        }

        [Fact]
        public void FindStationFiles_WithStationsXml_FindsFile()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "stations.xml"), "<?xml version=\"1.0\"?><Stations></Stations>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindStationFiles();

            // Assert
            files.Should().HaveCount(1);
            files[0].Should().EndWith("stations.xml");
        }

        #endregion

        #region FindOutfitFiles Tests

        [Fact]
        public void FindOutfitFiles_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindOutfitFiles();

            // Assert
            files.Should().BeEmpty("no outfit files exist");
        }

        [Fact]
        public void FindOutfitFiles_WithOutfitsXml_FindsFile()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "outfits.xml"), "<?xml version=\"1.0\"?><Outfits></Outfits>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindOutfitFiles();

            // Assert
            files.Should().HaveCount(1);
            files[0].Should().EndWith("outfits.xml");
        }

        #endregion

        #region FindDutySelectionFiles Tests

        [Fact]
        public void FindDutySelectionFiles_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindDutySelectionFiles();

            // Assert
            files.Should().BeEmpty("no duty selection files exist");
        }

        [Fact]
        public void FindDutySelectionFiles_WithDutySelectionXml_FindsFile()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "duty_selection.xml"), "<?xml version=\"1.0\"?><Descriptions></Descriptions>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var files = service.FindDutySelectionFiles();

            // Assert
            files.Should().HaveCount(1);
            files[0].Should().EndWith("duty_selection.xml");
        }

        #endregion

        #region FindRanksFile Tests

        [Fact]
        public void FindRanksFile_WithNoRanksFile_ReturnsNull()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var file = service.FindRanksFile();

            // Assert
            file.Should().BeNull("no Ranks.xml exists");
        }

        [Fact]
        public void FindRanksFile_WithRanksXml_FindsFile()
        {
            // Arrange
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "Ranks.xml"), "<?xml version=\"1.0\"?><Ranks></Ranks>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var file = service.FindRanksFile();

            // Assert
            file.Should().NotBeNull();
            file.Should().EndWith("Ranks.xml");
        }

        [Fact]
        public void FindRanksFile_WithLegacyPath_FindsFile()
        {
            // Arrange
            var lspdfFolder = Path.Combine(_tempDirectory, "LSPD First Response");
            Directory.CreateDirectory(lspdfFolder);
            File.WriteAllText(Path.Combine(lspdfFolder, "Ranks.xml"), "<?xml version=\"1.0\"?><Ranks></Ranks>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var file = service.FindRanksFile();

            // Assert
            file.Should().NotBeNull("legacy Ranks.xml path should be found");
            file.Should().EndWith("Ranks.xml");
        }

        [Fact]
        public void FindRanksFile_PrefersEnhancedPathOverLegacy()
        {
            // Arrange - Create both paths
            var lspdfData = Path.Combine(_tempDirectory, "lspdfr", "data");
            Directory.CreateDirectory(lspdfData);
            File.WriteAllText(Path.Combine(lspdfData, "Ranks.xml"), "<?xml version=\"1.0\"?><Ranks></Ranks>");

            var lspdfFolder = Path.Combine(_tempDirectory, "LSPD First Response");
            Directory.CreateDirectory(lspdfFolder);
            File.WriteAllText(Path.Combine(lspdfFolder, "Ranks.xml"), "<?xml version=\"1.0\"?><Ranks></Ranks>");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var file = service.FindRanksFile();

            // Assert
            file.Should().NotBeNull();
            file.Should().Contain("lspdfr", "enhanced path should be preferred");
        }

        #endregion

        #region IsValidGTAVRoot Tests

        [Fact]
        public void IsValidGTAVRoot_WithNoGtaFiles_ReturnsFalse()
        {
            // Arrange
            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var isValid = service.IsValidGTAVRoot();

            // Assert
            isValid.Should().BeFalse("no GTA V indicators exist");
        }

        [Fact]
        public void IsValidGTAVRoot_WithGTA5Exe_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_tempDirectory, "GTA5.exe"), "dummy");

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var isValid = service.IsValidGTAVRoot();

            // Assert
            isValid.Should().BeTrue("GTA5.exe exists");
        }

        [Fact]
        public void IsValidGTAVRoot_WithLspdfFolder_ReturnsTrue()
        {
            // Arrange
            Directory.CreateDirectory(Path.Combine(_tempDirectory, "lspdfr"));

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var isValid = service.IsValidGTAVRoot();

            // Assert
            isValid.Should().BeTrue("lspdfr folder exists");
        }

        [Fact]
        public void IsValidGTAVRoot_WithLegacyLspdfFolder_ReturnsTrue()
        {
            // Arrange
            Directory.CreateDirectory(Path.Combine(_tempDirectory, "LSPD First Response"));

            var service = new FileDiscoveryService(_tempDirectory);

            // Act
            var isValid = service.IsValidGTAVRoot();

            // Assert
            isValid.Should().BeTrue("LSPD First Response folder exists");
        }

        #endregion
    }
}
