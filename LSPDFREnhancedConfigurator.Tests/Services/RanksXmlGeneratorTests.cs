using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Helpers;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Unit tests for RanksXmlGenerator to verify correct XML generation.
    /// Covers: rank properties, stations, global vehicles/outfits, station-specific vehicles/outfits, parent ranks.
    /// </summary>
    public class RanksXmlGeneratorTests
    {
        #region A. Basic XML Generation (5 tests)

        [Fact]
        public void GenerateXml_EmptyRankList_ReturnsEmptyRanksElement()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(ranks);
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            doc.Root.Name.LocalName.Should().Be("Ranks");
            doc.Root.Elements("Rank").Should().BeEmpty("Empty rank list should produce empty Ranks element");
        }

        [Fact]
        public void GenerateXml_SingleBasicRank_GeneratesCorrectXmlStructure()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank("Officer", 100, 1500);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            rankElement.Element("Name")?.Value.Should().Be("Officer");
            rankElement.Element("RequiredPoints")?.Value.Should().Be("100");
            rankElement.Element("Salary")?.Value.Should().Be("1500");
        }

        [Fact]
        public void GenerateXml_MultipleRanks_GeneratesAllRanksInOrder()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                TestHelpers.CreateBasicRank("Rookie", 0, 1000),
                TestHelpers.CreateBasicRank("Officer", 100, 1500),
                TestHelpers.CreateBasicRank("Detective", 500, 2000)
            };

            // Act
            var xml = RanksXmlGenerator.GenerateXml(ranks);
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElements = doc.Descendants("Rank").ToList();
            rankElements.Should().HaveCount(3);
            rankElements[0].Element("Name")?.Value.Should().Be("Rookie");
            rankElements[1].Element("Name")?.Value.Should().Be("Officer");
            rankElements[2].Element("Name")?.Value.Should().Be("Detective");
        }

        [Fact]
        public void GenerateXml_ValidXml_StartsWithUtf8Declaration()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });

            // Assert
            TestHelpers.VerifyUtf8Declaration(xml);
        }

        [Fact]
        public void GenerateXml_ValidXml_IsWellFormedAndIndented()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });

            // Assert
            var doc = TestHelpers.ParseAndValidateXml(xml);
            xml.Should().Contain("  ", "XML should be indented");
            doc.Root.Should().NotBeNull();
        }

        #endregion

        #region B. Station Generation (6 tests)

        [Fact]
        public void GenerateXml_RankWithSingleStation_GeneratesStationElement()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            var stationsElement = rankElement.Element("Stations");
            stationsElement.Should().NotBeNull();
            stationsElement!.Elements("Station").Should().ContainSingle();
        }

        [Fact]
        public void GenerateXml_RankWithMultipleStations_GeneratesAllStations()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations.Add(TestHelpers.CreateStationWithItems("Vespucci", styleID: 2));
            rank.Stations.Add(TestHelpers.CreateStationWithItems("Davis", styleID: 3));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            var stations = rankElement.Descendants("Station").ToList();
            stations.Should().HaveCount(3);
        }

        [Fact]
        public void GenerateXml_Station_ContainsStationName()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = TestHelpers.GetStationElement(TestHelpers.GetRankElement(doc));
            stationElement.Element("StationName")?.Value.Should().Be("Mission Row");
        }

        [Fact]
        public void GenerateXml_Station_ContainsStyleID()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = TestHelpers.GetStationElement(TestHelpers.GetRankElement(doc));
            stationElement.Element("StyleID")?.Value.Should().Be("1");
        }

        [Fact]
        public void GenerateXml_StationWithZones_GeneratesZonesElement()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = TestHelpers.GetStationElement(TestHelpers.GetRankElement(doc));
            var zonesElement = stationElement.Element("Zones");
            zonesElement.Should().NotBeNull();
            zonesElement!.Elements("Zone").Should().ContainSingle()
                .Which.Value.Should().Be("Downtown");
        }

        [Fact]
        public void GenerateXml_StationWithNoZones_OmitsZonesElement()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations.Clear();
            rank.Stations.Add(TestHelpers.CreateStationWithItems("Vespucci", zones: new List<string>()));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = TestHelpers.GetStationElement(TestHelpers.GetRankElement(doc));
            stationElement.Element("Zones").Should().BeNull("Empty zones list should not generate Zones element");
        }

        #endregion

        #region C. Global Vehicles (4 tests)

        [Fact]
        public void GenerateXml_RankWithGlobalVehicles_GeneratesVehiclesAtRankLevel()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Cruiser"));
            rank.Vehicles.Add(TestHelpers.CreateVehicle("police2", "Police Interceptor"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            var vehiclesElement = rankElement.Element("Vehicles");
            vehiclesElement.Should().NotBeNull("Rank with global vehicles should have Vehicles element");
            vehiclesElement!.Elements("Vehicle").Should().HaveCount(2);
        }

        [Fact]
        public void GenerateXml_GlobalVehicle_ContainsModelAttribute()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Cruiser"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var vehicleElement = TestHelpers.GetRankElement(doc).Element("Vehicles")!.Element("Vehicle");
            TestHelpers.GetRequiredAttribute(vehicleElement!, "model").Should().Be("police");
        }

        [Fact]
        public void GenerateXml_GlobalVehicle_ContainsDisplayNameAsText()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Cruiser"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var vehicleElement = TestHelpers.GetRankElement(doc).Element("Vehicles")!.Element("Vehicle");
            vehicleElement!.Value.Should().Be("Police Cruiser");
        }

        [Fact]
        public void GenerateXml_RankWithNoGlobalVehicles_OmitsVehiclesElement()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            rankElement.Element("Vehicles").Should().BeNull("Rank without global vehicles should not have Vehicles element");
        }

        #endregion

        #region D. Global Outfits (3 tests)

        [Fact]
        public void GenerateXml_RankWithGlobalOutfits_GeneratesOutfitsAtRankLevel()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Outfits.Add("LSPD_Standard_Uniform");
            rank.Outfits.Add("LSPD_Tactical_Uniform");

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            var outfitsElement = rankElement.Element("Outfits");
            outfitsElement.Should().NotBeNull("Rank with global outfits should have Outfits element");
            outfitsElement!.Elements("Outfit").Should().HaveCount(2);
        }

        [Fact]
        public void GenerateXml_GlobalOutfit_ContainsOutfitName()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Outfits.Add("LSPD_Standard_Uniform");

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var outfitElement = TestHelpers.GetRankElement(doc).Element("Outfits")!.Element("Outfit");
            outfitElement!.Value.Should().Be("LSPD_Standard_Uniform");
        }

        [Fact]
        public void GenerateXml_RankWithNoGlobalOutfits_OmitsOutfitsElement()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            rankElement.Element("Outfits").Should().BeNull("Rank without global outfits should not have Outfits element");
        }

        #endregion

        #region E. CRITICAL: Station-Specific Vehicles (5 tests)

        [Fact]
        public void GenerateXml_StationWithVehicles_GeneratesVehiclesInsideStation()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            var station = rank.Stations[0];
            station.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Cruiser"));
            station.Vehicles.Add(TestHelpers.CreateVehicle("police2", "Police Interceptor"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = doc.Descendants("Station").First();
            var vehiclesElement = stationElement.Element("Vehicles");

            vehiclesElement.Should().NotBeNull("Vehicles should be inside Station");
            vehiclesElement!.Elements("Vehicle").Should().HaveCount(2);

            // Verify NOT at rank level
            var rankVehicles = doc.Descendants("Rank").First().Element("Vehicles");
            rankVehicles.Should().BeNull("Rank should not have Vehicles when only station vehicles exist");
        }

        [Fact]
        public void GenerateXml_StationVehicle_ContainsModelAttributeAndDisplayName()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations[0].Vehicles.Add(TestHelpers.CreateVehicle("police3", "Station Special"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationVehicle = doc.Descendants("Station").First().Element("Vehicles")!.Element("Vehicle");
            TestHelpers.GetRequiredAttribute(stationVehicle!, "model").Should().Be("police3");
            stationVehicle!.Value.Should().Be("Station Special");
        }

        [Fact]
        public void GenerateXml_StationWithNoVehicles_OmitsVehiclesElementFromStation()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = doc.Descendants("Station").First();
            stationElement.Element("Vehicles").Should().BeNull("Station without vehicles should not have Vehicles element");
        }

        [Fact]
        public void GenerateXml_RankWithBothGlobalAndStationVehicles_GeneratesBothSeparately()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Vehicles.Add(TestHelpers.CreateVehicle("police", "Global Car"));
            rank.Stations[0].Vehicles.Add(TestHelpers.CreateVehicle("police2", "Station Car"));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert - Global vehicles at rank level
            var rankVehicles = doc.Descendants("Rank").First().Element("Vehicles");
            rankVehicles.Should().NotBeNull();
            rankVehicles!.Elements("Vehicle").Should().ContainSingle()
                .Which.Attribute("model")!.Value.Should().Be("police");

            // Assert - Station vehicles inside Station
            var stationVehicles = doc.Descendants("Station").First().Element("Vehicles");
            stationVehicles.Should().NotBeNull();
            stationVehicles!.Elements("Vehicle").Should().ContainSingle()
                .Which.Attribute("model")!.Value.Should().Be("police2");
        }

        [Fact]
        public void GenerateXml_MultipleStationsWithDifferentVehicles_GeneratesCorrectly()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations[0].Vehicles.Add(TestHelpers.CreateVehicle("police", "Mission Row Car"));

            var station2 = TestHelpers.CreateStationWithItems("Vespucci", styleID: 2);
            station2.Vehicles.Add(TestHelpers.CreateVehicle("police2", "Vespucci Car"));
            rank.Stations.Add(station2);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stations = doc.Descendants("Station").ToList();
            stations.Should().HaveCount(2);

            stations[0].Element("Vehicles")!.Element("Vehicle")!.Attribute("model")!.Value.Should().Be("police");
            stations[1].Element("Vehicles")!.Element("Vehicle")!.Attribute("model")!.Value.Should().Be("police2");
        }

        #endregion

        #region F. CRITICAL: Station-Specific Outfits (4 tests)

        [Fact]
        public void GenerateXml_StationWithOutfits_GeneratesOutfitsInsideStation()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations[0].Outfits.Add("MissionRow_Uniform");
            rank.Stations[0].Outfits.Add("MissionRow_Tactical");

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = doc.Descendants("Station").First();
            var outfitsElement = stationElement.Element("Outfits");

            outfitsElement.Should().NotBeNull("Outfits should be inside Station");
            outfitsElement!.Elements("Outfit").Should().HaveCount(2);

            // Verify NOT at rank level
            var rankOutfits = doc.Descendants("Rank").First().Element("Outfits");
            rankOutfits.Should().BeNull("Rank should not have Outfits when only station outfits exist");
        }

        [Fact]
        public void GenerateXml_StationWithNoOutfits_OmitsOutfitsElementFromStation()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stationElement = doc.Descendants("Station").First();
            stationElement.Element("Outfits").Should().BeNull("Station without outfits should not have Outfits element");
        }

        [Fact]
        public void GenerateXml_RankWithBothGlobalAndStationOutfits_GeneratesBothSeparately()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Outfits.Add("LSPD_Global_Uniform");
            rank.Stations[0].Outfits.Add("MissionRow_Uniform");

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert - Global outfits at rank level
            var rankOutfits = doc.Descendants("Rank").First().Element("Outfits");
            rankOutfits.Should().NotBeNull();
            rankOutfits!.Elements("Outfit").Should().ContainSingle()
                .Which.Value.Should().Be("LSPD_Global_Uniform");

            // Assert - Station outfits inside Station
            var stationOutfits = doc.Descendants("Station").First().Element("Outfits");
            stationOutfits.Should().NotBeNull();
            stationOutfits!.Elements("Outfit").Should().ContainSingle()
                .Which.Value.Should().Be("MissionRow_Uniform");
        }

        [Fact]
        public void GenerateXml_MultipleStationsWithDifferentOutfits_GeneratesCorrectly()
        {
            // Arrange
            var rank = TestHelpers.CreateBasicRank();
            rank.Stations[0].Outfits.Add("MissionRow_Uniform");

            var station2 = TestHelpers.CreateStationWithItems("Vespucci", styleID: 2);
            station2.Outfits.Add("Vespucci_Uniform");
            rank.Stations.Add(station2);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var stations = doc.Descendants("Station").ToList();
            stations.Should().HaveCount(2);

            stations[0].Element("Outfits")!.Element("Outfit")!.Value.Should().Be("MissionRow_Uniform");
            stations[1].Element("Outfits")!.Element("Outfit")!.Value.Should().Be("Vespucci_Uniform");
        }

        #endregion

        #region G. Parent Ranks with Pay Bands (4 tests)

        [Fact]
        public void GenerateXml_ParentRankWithPayBands_GeneratesOnlyPayBands()
        {
            // Arrange
            var parentRank = new RankHierarchy("Officer", 100, 1500);
            parentRank.IsParent = true;

            var payBand1 = new RankHierarchy("Officer I", 100, 1500);
            var payBand2 = new RankHierarchy("Officer II", 200, 1800);
            parentRank.PayBands.Add(payBand1);
            parentRank.PayBands.Add(payBand2);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { parentRank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElements = doc.Descendants("Rank").ToList();
            rankElements.Should().HaveCount(2, "Parent rank should generate pay bands only");
            rankElements[0].Element("Name")?.Value.Should().Be("Officer I");
            rankElements[1].Element("Name")?.Value.Should().Be("Officer II");
        }

        [Fact]
        public void GenerateXml_PayBands_GenerateAsIndependentRanks()
        {
            // Arrange
            var parentRank = new RankHierarchy("Officer", 100, 1500);
            parentRank.IsParent = true;

            var payBand = new RankHierarchy("Officer I", 100, 1500);
            payBand.Vehicles.Add(TestHelpers.CreateVehicle("police", "Police Car"));
            parentRank.PayBands.Add(payBand);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { parentRank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            rankElement.Element("Name")?.Value.Should().Be("Officer I");
            rankElement.Element("Vehicles").Should().NotBeNull("Pay band should have its own vehicles");
        }

        [Fact]
        public void GenerateXml_PayBandWithStations_GeneratesStationElements()
        {
            // Arrange
            var parentRank = new RankHierarchy("Officer", 100, 1500);
            parentRank.IsParent = true;

            var payBand = TestHelpers.CreateBasicRank("Officer I", 100, 1500);
            parentRank.PayBands.Add(payBand);

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { parentRank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);
            rankElement.Element("Stations").Should().NotBeNull();
            rankElement.Descendants("Station").Should().ContainSingle();
        }

        [Fact]
        public void GenerateXml_MixedParentAndStandaloneRanks_GeneratesAllCorrectly()
        {
            // Arrange
            var standaloneRank = TestHelpers.CreateBasicRank("Rookie", 0, 1000);

            var parentRank = new RankHierarchy("Officer", 100, 1500);
            parentRank.IsParent = true;
            parentRank.PayBands.Add(new RankHierarchy("Officer I", 100, 1500));
            parentRank.PayBands.Add(new RankHierarchy("Officer II", 200, 1800));

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { standaloneRank, parentRank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElements = doc.Descendants("Rank").ToList();
            rankElements.Should().HaveCount(3, "Should have 1 standalone + 2 pay bands");
            rankElements[0].Element("Name")?.Value.Should().Be("Rookie");
            rankElements[1].Element("Name")?.Value.Should().Be("Officer I");
            rankElements[2].Element("Name")?.Value.Should().Be("Officer II");
        }

        #endregion

        #region H. Complex Integration (2 tests)

        [Fact]
        public void GenerateXml_ComplexHierarchy_GeneratesAllElementsInCorrectOrder()
        {
            // Arrange
            var rank = TestHelpers.CreateComplexRank();

            // Act
            var xml = RanksXmlGenerator.GenerateXml(new List<RankHierarchy> { rank });
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            var rankElement = TestHelpers.GetRankElement(doc);

            // Verify order: Name, RequiredPoints, Salary, Stations, Vehicles, Outfits
            var children = rankElement.Elements().Select(e => e.Name.LocalName).ToList();
            children.IndexOf("Name").Should().BeLessThan(children.IndexOf("RequiredPoints"));
            children.IndexOf("Stations").Should().BeLessThan(children.IndexOf("Vehicles"));
            children.IndexOf("Vehicles").Should().BeLessThan(children.IndexOf("Outfits"));

            // Verify all data present
            rankElement.Element("Name")?.Value.Should().Be("Senior Officer");
            rankElement.Element("Stations")!.Elements("Station").Should().HaveCount(2);
            rankElement.Element("Vehicles")!.Elements("Vehicle").Should().HaveCount(2);
            rankElement.Element("Outfits")!.Elements("Outfit").Should().HaveCount(2);

            // Verify station-specific items
            var station1 = doc.Descendants("Station").First();
            station1.Element("Vehicles")!.Elements("Vehicle").Should().ContainSingle();
            station1.Element("Outfits")!.Elements("Outfit").Should().ContainSingle();
        }

        [Fact]
        public void GenerateXml_RealWorldScenario_GeneratesValidGameReadableXml()
        {
            // Arrange - Real-world scenario with multiple ranks, stations, vehicles, outfits
            var ranks = new List<RankHierarchy>
            {
                TestHelpers.CreateBasicRank("Cadet", 0, 800),
                TestHelpers.CreateComplexRank()
            };

            // Add more detail to first rank
            ranks[0].Vehicles.Add(TestHelpers.CreateVehicle("police", "Standard Cruiser"));
            ranks[0].Outfits.Add("LSPD_Cadet_Uniform");

            // Act
            var xml = RanksXmlGenerator.GenerateXml(ranks);
            var doc = TestHelpers.ParseAndValidateXml(xml);

            // Assert
            TestHelpers.VerifyUtf8Declaration(xml);
            doc.Root.Name.LocalName.Should().Be("Ranks");
            doc.Descendants("Rank").Should().HaveCount(2);

            // Verify structure is complete and valid
            foreach (var rankElement in doc.Descendants("Rank"))
            {
                rankElement.Element("Name").Should().NotBeNull();
                rankElement.Element("RequiredPoints").Should().NotBeNull();
                rankElement.Element("Salary").Should().NotBeNull();
            }
        }

        #endregion
    }
}
