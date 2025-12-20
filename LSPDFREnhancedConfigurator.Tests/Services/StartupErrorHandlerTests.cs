using System;
using System.IO;
using System.Xml;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class StartupErrorHandlerTests
    {
        #region GetUserFriendlyMessage Tests

        [Fact]
        public void GetUserFriendlyMessage_FileNotFoundException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found", "config.xml");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Required file not found");
            message.Should().Contain("config.xml");
            message.Should().Contain("LSPDFR Enhanced is properly installed");
        }

        [Fact]
        public void GetUserFriendlyMessage_FileNotFoundException_WithNullFileName_ReturnsUnknownFile()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Required file not found");
            message.Should().Contain("Unknown file");
        }

        [Fact]
        public void GetUserFriendlyMessage_DirectoryNotFoundException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ex = new DirectoryNotFoundException("Directory 'C:\\Missing' not found");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Required directory not found");
            message.Should().Contain("Directory 'C:\\Missing' not found");
            message.Should().Contain("LSPDFR Enhanced is properly installed");
        }

        [Fact]
        public void GetUserFriendlyMessage_InvalidDataException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ex = new InvalidDataException("Invalid data format in configuration");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Failed to read configuration file");
            message.Should().Contain("Invalid data format in configuration");
            message.Should().Contain("corrupted or malformed");
        }

        [Fact]
        public void GetUserFriendlyMessage_XmlException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ex = new XmlException("Invalid XML", null, 10, 5);

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Failed to parse XML configuration file");
            message.Should().Contain("Line 10, Position 5");
            message.Should().Contain("XML file may be malformed");
        }

        [Fact]
        public void GetUserFriendlyMessage_UnauthorizedAccessException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ex = new UnauthorizedAccessException("Access denied to file");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Access denied");
            message.Should().Contain("Access denied to file");
            message.Should().Contain("permission to read files");
            message.Should().Contain("running the application as administrator");
        }

        [Fact]
        public void GetUserFriendlyMessage_InvalidOperationException_WithSplitterDistance_ReturnsUILayoutError()
        {
            // Arrange
            var ex = new InvalidOperationException("SplitterDistance must be between 0 and 100");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("UI Layout Error");
            message.Should().Contain("Window size is too small");
            message.Should().Contain("maximize the window");
        }

        [Fact]
        public void GetUserFriendlyMessage_InvalidOperationException_WithoutSplitterDistance_ReturnsGenericMessage()
        {
            // Arrange
            var ex = new InvalidOperationException("Some operation failed");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("Operation failed");
            message.Should().Contain("Some operation failed");
            message.Should().Contain("application error");
        }

        [Fact]
        public void GetUserFriendlyMessage_ArgumentException_WithSplitterDistance_ReturnsUILayoutError()
        {
            // Arrange
            var ex = new ArgumentException("SplitterDistance must be between 0 and 100");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("UI Layout Error");
            message.Should().Contain("Window size is too small");
            message.Should().Contain("maximize the window");
        }

        [Fact]
        public void GetUserFriendlyMessage_UnhandledException_ReturnsGenericMessage()
        {
            // Arrange
            var ex = new NotImplementedException("Feature not implemented");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex);

            // Assert
            message.Should().Contain("An unexpected error occurred");
            message.Should().Contain("Feature not implemented");
            message.Should().Contain("check the log file");
        }

        [Fact]
        public void GetUserFriendlyMessage_WithContext_PrependsContext()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found");
            var context = "Loading configuration";

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex, context);

            // Assert
            message.Should().StartWith("Loading configuration");
            message.Should().Contain("Required file not found");
        }

        [Fact]
        public void GetUserFriendlyMessage_WithEmptyContext_DoesNotPrependContext()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex, "");

            // Assert
            message.Should().NotStartWith("\n\n");
            message.Should().StartWith("Required file not found");
        }

        [Fact]
        public void GetUserFriendlyMessage_WithNullContext_DoesNotPrependContext()
        {
            // Arrange
            var ex = new FileNotFoundException("File not found");

            // Act
            var message = StartupErrorHandler.GetUserFriendlyMessage(ex, null);

            // Assert
            message.Should().NotStartWith("\n\n");
            message.Should().StartWith("Required file not found");
        }

        #endregion

        #region GetDirectorySelectionError Tests

        [Fact]
        public void GetDirectorySelectionError_NullPath_ReturnsError()
        {
            // Act
            var error = StartupErrorHandler.GetDirectorySelectionError(null);

            // Assert
            error.Should().NotBeNull();
            error.Should().Contain("No directory was selected");
            error.Should().Contain("select your GTA V root directory");
        }

        [Fact]
        public void GetDirectorySelectionError_EmptyPath_ReturnsError()
        {
            // Act
            var error = StartupErrorHandler.GetDirectorySelectionError("");

            // Assert
            error.Should().NotBeNull();
            error.Should().Contain("No directory was selected");
            error.Should().Contain("select your GTA V root directory");
        }

        [Fact]
        public void GetDirectorySelectionError_NonExistentDirectory_ReturnsError()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act
            var error = StartupErrorHandler.GetDirectorySelectionError(nonExistentPath);

            // Assert
            error.Should().NotBeNull();
            error.Should().Contain("Directory does not exist");
            error.Should().Contain(nonExistentPath);
            error.Should().Contain("valid GTA V root directory");
        }

        [Fact]
        public void GetDirectorySelectionError_MissingGTA5Exe_ReturnsError()
        {
            // Arrange - use temp directory which exists but doesn't have GTA5.exe
            var tempPath = Path.GetTempPath();

            // Act
            var error = StartupErrorHandler.GetDirectorySelectionError(tempPath);

            // Assert
            error.Should().NotBeNull();
            error.Should().Contain("GTA5.exe not found");
            error.Should().Contain(tempPath);
            error.Should().Contain("where GTA5.exe is located");
        }

        [Fact]
        public void GetDirectorySelectionError_MissingLSPDFR_ReturnsError()
        {
            // Arrange - create temp directory with GTA5.exe but no LSPDFR
            var testDir = Path.Combine(Path.GetTempPath(), $"gtav_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(testDir);
            File.WriteAllText(Path.Combine(testDir, "GTA5.exe"), "fake exe");

            try
            {
                // Act
                var error = StartupErrorHandler.GetDirectorySelectionError(testDir);

                // Assert
                error.Should().NotBeNull();
                error.Should().Contain("LSPDFR not found");
                error.Should().Contain("plugins\\LSPDFR");
                error.Should().Contain("install it before using this configurator");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, true);
                }
            }
        }

        [Fact]
        public void GetDirectorySelectionError_MissingLSPDFREnhanced_ReturnsError()
        {
            // Arrange - create temp directory with GTA5.exe and LSPDFR but no LSPDFR Enhanced
            var testDir = Path.Combine(Path.GetTempPath(), $"gtav_test_{Guid.NewGuid()}");
            var lspdfr = Path.Combine(testDir, "plugins", "LSPDFR");
            Directory.CreateDirectory(lspdfr);
            File.WriteAllText(Path.Combine(testDir, "GTA5.exe"), "fake exe");

            try
            {
                // Act
                var error = StartupErrorHandler.GetDirectorySelectionError(testDir);

                // Assert
                error.Should().NotBeNull();
                error.Should().Contain("LSPDFR Enhanced not found");
                error.Should().Contain("LSPDFR Enhanced");
                error.Should().Contain("install it before using this configurator");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, true);
                }
            }
        }

        [Fact]
        public void GetDirectorySelectionError_ValidDirectory_ReturnsNull()
        {
            // Arrange - create complete directory structure
            var testDir = Path.Combine(Path.GetTempPath(), $"gtav_test_{Guid.NewGuid()}");
            var lspdfEnhanced = Path.Combine(testDir, "plugins", "LSPDFR", "LSPDFR Enhanced");
            Directory.CreateDirectory(lspdfEnhanced);
            File.WriteAllText(Path.Combine(testDir, "GTA5.exe"), "fake exe");

            try
            {
                // Act
                var error = StartupErrorHandler.GetDirectorySelectionError(testDir);

                // Assert
                error.Should().BeNull();
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, true);
                }
            }
        }

        #endregion
    }
}
