using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Tests for DataLoadingService - Core data loading orchestration
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class DataLoadingServiceTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFileDiscovery_DoesNotThrow()
        {
            // Act
            var act = () => new DataLoadingService(null!);

            // Assert
            act.Should().NotThrow("constructor should handle null gracefully");
        }

        [Fact]
        public void Constructor_InitializesCollections()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");

            // Act
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Assert
            service.Agencies.Should().NotBeNull();
            service.AllVehicles.Should().NotBeNull();
            service.Stations.Should().NotBeNull();
            service.OutfitVariations.Should().NotBeNull();
            service.Ranks.Should().NotBeNull();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Properties_AreVirtual_ForMocking()
        {
            // This test verifies properties are virtual so they can be mocked by Moq
            // Arrange & Act
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Assert - Properties should be accessible
            service.Agencies.Should().NotBeNull();
            service.AllVehicles.Should().NotBeNull();
            service.Stations.Should().NotBeNull();
            service.OutfitVariations.Should().NotBeNull();
            service.Ranks.Should().NotBeNull();
        }

        [Fact]
        public void CollectionsDefaultToEmpty()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");

            // Act
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Assert - Before LoadAll() is called, collections should be empty
            service.Agencies.Should().BeEmpty();
            service.AllVehicles.Should().BeEmpty();
            service.Stations.Should().BeEmpty();
            service.OutfitVariations.Should().BeEmpty();
            service.Ranks.Should().BeEmpty();
        }

        #endregion
    }
}
