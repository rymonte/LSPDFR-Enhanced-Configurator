using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Parsers;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Helpers;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Integration
{
    /// <summary>
    /// Round-trip integration tests to verify data fidelity through generate → parse cycle.
    /// Ensures that data survives XML generation and parsing without loss or corruption.
    /// </summary>
    public class RanksXmlRoundTripTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        public void Dispose()
        {
            // Cleanup temp files after each test
            foreach (var file in _tempFiles)
            {
                TestHelpers.DeleteTempFile(file);
            }
        }

        private List<RankHierarchy> RoundTrip(List<RankHierarchy> originalRanks)
        {
            // Generate XML
            var xml = RanksXmlGenerator.GenerateXml(originalRanks);

            // Write to temp file
            var tempFile = TestHelpers.CreateTempXmlFile(xml);
            _tempFiles.Add(tempFile);

            // Parse back - RanksParser now returns List<RankHierarchy> directly
            var parsedRanks = RanksParser.ParseRanksFile(tempFile);
            return parsedRanks;
        }

        [Fact]
        public void RoundTrip_SimpleRank_MaintainsDataFidelity()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank("Officer", 100, 1500);

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var rank = result.Should().ContainSingle().Subject;
            rank.Name.Should().Be(original.Name);
            rank.RequiredPoints.Should().Be(original.RequiredPoints);
            rank.Salary.Should().Be(original.Salary);
        }

        [Fact]
        public void RoundTrip_RankWithMultipleStations_MaintainsAllStations()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Stations.Add(TestHelpers.CreateStationWithItems("Vespucci", styleID: 2));
            original.Stations.Add(TestHelpers.CreateStationWithItems("Davis", styleID: 3));

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var rank = result.Should().ContainSingle().Subject;
            rank.Stations.Should().HaveCount(3);
            rank.Stations[0].StationName.Should().Be("Mission Row");
            rank.Stations[1].StationName.Should().Be("Vespucci");
            rank.Stations[2].StationName.Should().Be("Davis");

            rank.Stations[0].StyleID.Should().Be(1);
            rank.Stations[1].StyleID.Should().Be(2);
            rank.Stations[2].StyleID.Should().Be(3);
        }

        [Fact]
        public void RoundTrip_RankWithGlobalVehicles_MaintainsVehicles()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Cruiser"));
            original.Vehicles.Add(TestHelpers.CreateVehicle("police2", "Police Interceptor"));

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var rank = result.Should().ContainSingle().Subject;
            rank.Vehicles.Should().HaveCount(2);
            rank.Vehicles[0].Model.Should().Be("police");
            rank.Vehicles[0].DisplayName.Should().Be("Police Cruiser");
            rank.Vehicles[1].Model.Should().Be("police2");
            rank.Vehicles[1].DisplayName.Should().Be("Police Interceptor");
        }

        [Fact]
        public void RoundTrip_RankWithGlobalOutfits_MaintainsOutfits()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Outfits.Add("LSPD_Standard_Uniform");
            original.Outfits.Add("LSPD_Tactical_Uniform");

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var rank = result.Should().ContainSingle().Subject;
            rank.Outfits.Should().HaveCount(2);
            rank.Outfits[0].Should().Be("LSPD_Standard_Uniform");
            rank.Outfits[1].Should().Be("LSPD_Tactical_Uniform");
        }

        [Fact]
        public void RoundTrip_RankWithStationVehicles_MaintainsStationVehicles()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Stations[0].Vehicles.Add(TestHelpers.CreateVehicle("police3", "Station Special"));
            original.Stations[0].Vehicles.Add(TestHelpers.CreateVehicle("police4", "Station SUV"));

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var station = result[0].Stations.Should().ContainSingle().Subject;
            station.Vehicles.Should().HaveCount(2);
            station.Vehicles[0].Model.Should().Be("police3");
            station.Vehicles[0].DisplayName.Should().Be("Station Special");
            station.Vehicles[1].Model.Should().Be("police4");
            station.Vehicles[1].DisplayName.Should().Be("Station SUV");

            // Verify NOT in global vehicles
            result[0].Vehicles.Should().BeEmpty("Station vehicles should not appear in global vehicles");
        }

        [Fact]
        public void RoundTrip_RankWithStationOutfits_MaintainsStationOutfits()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Stations[0].Outfits.Add("MissionRow_Uniform");
            original.Stations[0].Outfits.Add("MissionRow_Tactical");

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var station = result[0].Stations.Should().ContainSingle().Subject;
            station.Outfits.Should().HaveCount(2);
            station.Outfits[0].Should().Be("MissionRow_Uniform");
            station.Outfits[1].Should().Be("MissionRow_Tactical");

            // Verify NOT in global outfits
            result[0].Outfits.Should().BeEmpty("Station outfits should not appear in global outfits");
        }

        [Fact]
        public void RoundTrip_ComplexRank_MaintainsAllData()
        {
            // Arrange - Complex rank with both global and station-specific items
            var original = TestHelpers.CreateComplexRank();

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var rank = result.Should().ContainSingle().Subject;

            // Basic properties
            rank.Name.Should().Be("Senior Officer");
            rank.RequiredPoints.Should().Be(500);
            rank.Salary.Should().Be(2000);

            // Global vehicles
            rank.Vehicles.Should().HaveCount(2);
            rank.Vehicles.Should().Contain(v => v.Model == "police");
            rank.Vehicles.Should().Contain(v => v.Model == "police2");

            // Global outfits
            rank.Outfits.Should().HaveCount(2);
            rank.Outfits.Should().Contain("LSPD_Standard");
            rank.Outfits.Should().Contain("LSPD_Tactical");

            // Stations
            rank.Stations.Should().HaveCount(2);

            // First station
            var station1 = rank.Stations[0];
            station1.StationName.Should().Be("Mission Row");
            station1.Vehicles.Should().ContainSingle().Which.Model.Should().Be("police3");
            station1.Outfits.Should().ContainSingle().Which.Should().Be("MissionRow_Custom");
            station1.Zones.Should().HaveCount(2);
            station1.Zones.Should().Contain("Downtown");
            station1.Zones.Should().Contain("Little Seoul");

            // Second station
            var station2 = rank.Stations[1];
            station2.StationName.Should().Be("Vespucci");
            station2.Vehicles.Should().ContainSingle().Which.Model.Should().Be("police4");
            station2.Outfits.Should().ContainSingle().Which.Should().Be("Vespucci_Shorts");
            station2.Zones.Should().ContainSingle().Which.Should().Be("Vespucci Beach");
        }

        [Fact]
        public void RoundTrip_MultipleRanks_MaintainsAllRanks()
        {
            // Arrange
            var original = new List<RankHierarchy>
            {
                TestHelpers.CreateBasicRank("Rookie", 0, 1000),
                TestHelpers.CreateBasicRank("Officer", 100, 1500),
                TestHelpers.CreateBasicRank("Detective", 500, 2000)
            };

            // Act
            var result = RoundTrip(original);

            // Assert
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Rookie");
            result[0].RequiredPoints.Should().Be(0);
            result[1].Name.Should().Be("Officer");
            result[1].RequiredPoints.Should().Be(100);
            result[2].Name.Should().Be("Detective");
            result[2].RequiredPoints.Should().Be(500);
        }

        [Fact]
        public void RoundTrip_GenerateTwice_ProducesSameXml()
        {
            // Arrange
            var original = TestHelpers.CreateComplexRank();

            // Act
            var xml1 = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { original });
            var xml2 = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { original });

            // Assert
            xml1.Should().Be(xml2, "Generating XML twice from the same data should produce identical results (idempotency)");
        }

        [Fact]
        public void RoundTrip_StationZones_MaintainsZoneOrder()
        {
            // Arrange
            var original = TestHelpers.CreateBasicRank();
            original.Stations[0].Zones.Clear();
            original.Stations[0].Zones.Add("Zone1");
            original.Stations[0].Zones.Add("Zone2");
            original.Stations[0].Zones.Add("Zone3");

            // Act
            var result = RoundTrip(new List<RankHierarchy> { original });

            // Assert
            var station = result[0].Stations.Should().ContainSingle().Subject;
            station.Zones.Should().HaveCount(3);
            station.Zones[0].Should().Be("Zone1");
            station.Zones[1].Should().Be("Zone2");
            station.Zones[2].Should().Be("Zone3");
        }
    }
}
