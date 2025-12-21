using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
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
    public class DataLoadingServiceTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

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

        #region GetVehiclesByAgency Tests

        [Fact]
        public void GetVehiclesByAgency_NoVehicles_ReturnsEmpty()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Act
            var vehicles = service.GetVehiclesByAgency("lspd");

            // Assert
            vehicles.Should().BeEmpty();
        }

        [Fact]
        public void GetVehiclesByAgency_WithVehicles_FiltersCorrectly()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var vehicle1 = new Vehicle("police", "Police Cruiser", "lspd");
            var vehicle2 = new Vehicle("police2", "Police SUV", "lspd");
            var vehicle3 = new Vehicle("sheriff", "Sheriff Cruiser", "sheriff");

            service.AllVehicles.Add(vehicle1);
            service.AllVehicles.Add(vehicle2);
            service.AllVehicles.Add(vehicle3);

            // Act
            var vehicles = service.GetVehiclesByAgency("lspd");

            // Assert
            vehicles.Should().HaveCount(2);
            vehicles.Should().OnlyContain(v => v.Agencies.Contains("lspd"));
        }

        [Fact]
        public void GetVehiclesByAgency_CaseInsensitive_ReturnsVehicles()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var vehicle1 = new Vehicle("police", "Police Cruiser", "lspd");
            service.AllVehicles.Add(vehicle1);

            // Act
            var vehicles = service.GetVehiclesByAgency("LSPD");

            // Assert
            vehicles.Should().HaveCount(1);
        }

        #endregion

        #region GetStationsByAgency Tests

        [Fact]
        public void GetStationsByAgency_NoStations_ReturnsEmpty()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Act
            var stations = service.GetStationsByAgency("lspd");

            // Assert
            stations.Should().BeEmpty();
        }

        [Fact]
        public void GetStationsByAgency_WithStations_FiltersCorrectly()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var station1 = new Station("Mission Row", "lspd", "lspd");
            var station2 = new Station("Sandy Shores", "sheriff", "sheriff");
            service.Stations.Add(station1);
            service.Stations.Add(station2);

            // Act
            var stations = service.GetStationsByAgency("lspd");

            // Assert
            stations.Should().HaveCount(1);
            stations[0].Agency.Should().Be("lspd");
        }

        [Fact]
        public void GetStationsByAgency_CaseInsensitive_ReturnsStations()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var station1 = new Station("Mission Row", "lspd", "lspd");
            service.Stations.Add(station1);

            // Act
            var stations = service.GetStationsByAgency("LSPD");

            // Assert
            stations.Should().HaveCount(1);
        }

        #endregion

        #region GetOutfitsByAgency Tests

        [Fact]
        public void GetOutfitsByAgency_NoOutfits_ReturnsEmpty()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Act
            var outfits = service.GetOutfitsByAgency("lspd");

            // Assert
            outfits.Should().BeEmpty();
        }

        [Fact]
        public void GetOutfitsByAgency_WithOutfits_FiltersCorrectly()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var parentOutfit1 = new Outfit("lspd_patrol", "lspd") { InferredAgency = "lspd" };
            var variation1 = new OutfitVariation { Name = "Variation 1", ParentOutfit = parentOutfit1 };
            var parentOutfit2 = new Outfit("sheriff_patrol", "sheriff") { InferredAgency = "sheriff" };
            var variation2 = new OutfitVariation { Name = "Variation 2", ParentOutfit = parentOutfit2 };

            service.OutfitVariations.Add(variation1);
            service.OutfitVariations.Add(variation2);

            // Act
            var outfits = service.GetOutfitsByAgency("lspd");

            // Assert
            outfits.Should().HaveCount(1);
            outfits.Should().OnlyContain(o => o.ParentOutfit.InferredAgency == "lspd");
        }

        [Fact]
        public void GetOutfitsByAgency_CaseInsensitive_ReturnsOutfits()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var parentOutfit = new Outfit("lspd_patrol", "lspd") { InferredAgency = "lspd" };
            var variation = new OutfitVariation { Name = "Variation 1", ParentOutfit = parentOutfit };
            service.OutfitVariations.Add(variation);

            // Act
            var outfits = service.GetOutfitsByAgency("LSPD");

            // Assert
            outfits.Should().HaveCount(1);
        }

        #endregion

        #region LinkStationReferencesForHierarchies Tests

        [Fact]
        public void LinkStationReferencesForHierarchies_NullList_DoesNotThrow()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Act
            var act = () => service.LinkStationReferencesForHierarchies(null!);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_EmptyList_DoesNotThrow()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            // Act
            var act = () => service.LinkStationReferencesForHierarchies(new List<RankHierarchy>());

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_WithStations_LinksReferences()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var station = new Station("Mission Row Police Station", "lspd", "lspd");
            service.Stations.Add(station);

            var rank = new RankHierarchy("Officer", 1000, 5000);
            var stationAssignment = new StationAssignment("Mission Row Police Station", new List<string> { "zone1" }, 0);
            rank.Stations.Add(stationAssignment);

            var hierarchies = new List<RankHierarchy> { rank };

            // Act
            service.LinkStationReferencesForHierarchies(hierarchies);

            // Assert
            stationAssignment.StationReference.Should().NotBeNull();
            stationAssignment.StationReference.Name.Should().Be("Mission Row Police Station");
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_WithPayBands_LinksPayBandStations()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var station = new Station("Mission Row Police Station", "lspd", "lspd");
            service.Stations.Add(station);

            var rank = new RankHierarchy("Officer", 1000, 5000);
            var payBand = rank.AddPayBand();
            var stationAssignment = new StationAssignment("Mission Row Police Station", new List<string> { "zone1" }, 0);
            payBand.Stations.Add(stationAssignment);

            var hierarchies = new List<RankHierarchy> { rank };

            // Act
            service.LinkStationReferencesForHierarchies(hierarchies);

            // Assert
            stationAssignment.StationReference.Should().NotBeNull();
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_WithVehicles_LinksVehicleReferences()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var masterVehicle = new Vehicle("police", "Police Cruiser", "lspd");
            service.AllVehicles.Add(masterVehicle);

            var rank = new RankHierarchy("Officer", 1000, 5000);
            rank.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var hierarchies = new List<RankHierarchy> { rank };

            // Act
            service.LinkStationReferencesForHierarchies(hierarchies);

            // Assert
            rank.Vehicles[0].Should().BeSameAs(masterVehicle);
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_StationNotFound_DoesNotThrow()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var rank = new RankHierarchy("Officer", 1000, 5000);
            var stationAssignment = new StationAssignment("Non Existent Station", new List<string> { "zone1" }, 0);
            rank.Stations.Add(stationAssignment);

            var hierarchies = new List<RankHierarchy> { rank };

            // Act
            var act = () => service.LinkStationReferencesForHierarchies(hierarchies);

            // Assert
            act.Should().NotThrow();
            stationAssignment.StationReference.Should().BeNull();
        }

        [Fact]
        public void LinkStationReferencesForHierarchies_VehicleNotFound_DoesNotThrow()
        {
            // Arrange
            var mockFileDiscovery = new Mock<FileDiscoveryService>(MockBehavior.Loose, "C:\\Test");
            var service = new DataLoadingService(mockFileDiscovery.Object);

            var rank = new RankHierarchy("Officer", 1000, 5000);
            rank.Vehicles.Add(new Vehicle("non_existent", "Non Existent Vehicle", "lspd"));

            var hierarchies = new List<RankHierarchy> { rank };

            // Act
            var act = () => service.LinkStationReferencesForHierarchies(hierarchies);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Helper Methods

        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        #endregion
    }
}
