using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Parsers;
using LSPDFREnhancedConfigurator.Tests.Helpers;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Parsers
{
    /// <summary>
    /// Unit tests for RanksParser to verify correct XML parsing.
    /// Covers: rank properties, stations, global vehicles/outfits, station-specific vehicles/outfits, error handling.
    /// </summary>
    public class RanksParserTests : IDisposable
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

        private string CreateTempXmlFileAndTrack(string xmlContent)
        {
            var tempFile = TestHelpers.CreateTempXmlFile(xmlContent);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        #region A. Basic Parsing (5 tests)

        [Fact]
        public void ParseRanksFile_EmptyRanksElement_ReturnsEmptyList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            ranks.Should().BeEmpty("Empty Ranks element should return empty list");
        }

        [Fact]
        public void ParseRanksFile_SingleBasicRank_ParsesCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>100</RequiredPoints>
    <Salary>1500</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Name.Should().Be("Officer");
            rank.RequiredPoints.Should().Be(100);
            rank.Salary.Should().Be(1500);
        }

        [Fact]
        public void ParseRanksFile_MultipleRanks_ParsesAllInOrder()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Rookie</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>100</RequiredPoints>
    <Salary>1500</Salary>
  </Rank>
  <Rank>
    <Name>Detective</Name>
    <RequiredPoints>500</RequiredPoints>
    <Salary>2000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            ranks.Should().HaveCount(3);
            ranks[0].Name.Should().Be("Rookie");
            ranks[1].Name.Should().Be("Officer");
            ranks[2].Name.Should().Be("Detective");
        }

        [Fact]
        public void ParseRanksFile_NonExistentFile_ThrowsException()
        {
            // Arrange
            var nonExistentFile = "C:\\NonExistent\\File.xml";

            // Act & Assert
            Action act = () => RanksParser.ParseRanksFile(nonExistentFile);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ParseRanksFile_InvalidXml_ThrowsException()
        {
            // Arrange
            var invalidXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Broken
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(invalidXml);

            // Act & Assert
            Action act = () => RanksParser.ParseRanksFile(tempFile);
            act.Should().Throw<Exception>();
        }

        #endregion

        #region B. Station Parsing (6 tests)

        [Fact]
        public void ParseRanksFile_RankWithSingleStation_ParsesStation()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            var station = rank.Stations.Should().ContainSingle().Subject;
            station.StationName.Should().Be("Mission Row");
        }

        [Fact]
        public void ParseRanksFile_RankWithMultipleStations_ParsesAllStations()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
      </Station>
      <Station>
        <StationName>Vespucci</StationName>
        <StyleID>2</StyleID>
      </Station>
      <Station>
        <StationName>Davis</StationName>
        <StyleID>3</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Stations.Should().HaveCount(3);
            rank.Stations[0].StationName.Should().Be("Mission Row");
            rank.Stations[1].StationName.Should().Be("Vespucci");
            rank.Stations[2].StationName.Should().Be("Davis");
        }

        [Fact]
        public void ParseRanksFile_Station_ParsesStyleID()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>5</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.StyleID.Should().Be(5);
        }

        [Fact]
        public void ParseRanksFile_StationWithZones_ParsesAllZones()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <Zones>
          <Zone>Downtown</Zone>
          <Zone>Little Seoul</Zone>
          <Zone>Textile City</Zone>
        </Zones>
        <StyleID>1</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.Zones.Should().HaveCount(3);
            station.Zones.Should().Contain("Downtown");
            station.Zones.Should().Contain("Little Seoul");
            station.Zones.Should().Contain("Textile City");
        }

        [Fact]
        public void ParseRanksFile_StationWithNoZones_CreatesEmptyZonesList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.Zones.Should().NotBeNull();
            station.Zones.Should().BeEmpty();
        }

        [Fact]
        public void ParseRanksFile_RankWithNoStations_CreatesEmptyStationsList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Stations.Should().NotBeNull();
            rank.Stations.Should().BeEmpty();
        }

        #endregion

        #region C. Global Vehicles Parsing (4 tests)

        [Fact]
        public void ParseRanksFile_RankWithGlobalVehicles_ParsesVehiclesAtRankLevel()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Vehicles>
      <Vehicle model=""police"">Police Cruiser</Vehicle>
      <Vehicle model=""police2"">Police Interceptor</Vehicle>
    </Vehicles>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Vehicles.Should().HaveCount(2);
        }

        [Fact]
        public void ParseRanksFile_GlobalVehicle_ParsesModelAttribute()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Vehicles>
      <Vehicle model=""police"">Police Cruiser</Vehicle>
    </Vehicles>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var vehicle = ranks[0].Vehicles.Should().ContainSingle().Subject;
            vehicle.Model.Should().Be("police");
        }

        [Fact]
        public void ParseRanksFile_GlobalVehicle_ParsesDisplayName()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Vehicles>
      <Vehicle model=""police"">Police Cruiser</Vehicle>
    </Vehicles>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var vehicle = ranks[0].Vehicles.Should().ContainSingle().Subject;
            vehicle.DisplayName.Should().Be("Police Cruiser");
        }

        [Fact]
        public void ParseRanksFile_RankWithNoVehicles_CreatesEmptyVehiclesList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Vehicles.Should().NotBeNull();
            rank.Vehicles.Should().BeEmpty();
        }

        #endregion

        #region D. Global Outfits Parsing (3 tests)

        [Fact]
        public void ParseRanksFile_RankWithGlobalOutfits_ParsesOutfitsAtRankLevel()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Outfits>
      <Outfit>LSPD_Standard_Uniform</Outfit>
      <Outfit>LSPD_Tactical_Uniform</Outfit>
    </Outfits>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Outfits.Should().HaveCount(2);
        }

        [Fact]
        public void ParseRanksFile_GlobalOutfit_ParsesOutfitName()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Outfits>
      <Outfit>LSPD_Standard_Uniform</Outfit>
    </Outfits>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var outfit = ranks[0].Outfits.Should().ContainSingle().Subject;
            outfit.Should().Be("LSPD_Standard_Uniform");
        }

        [Fact]
        public void ParseRanksFile_RankWithNoOutfits_CreatesEmptyOutfitsList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Outfits.Should().NotBeNull();
            rank.Outfits.Should().BeEmpty();
        }

        #endregion

        #region E. CRITICAL: Station-Specific Vehicles Parsing (4 tests)

        [Fact]
        public void ParseRanksFile_StationWithVehicles_ParsesVehiclesIntoStation()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Vehicles>
          <Vehicle model=""police3"">Station Police Car</Vehicle>
          <Vehicle model=""police4"">Station SUV</Vehicle>
        </Vehicles>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.Vehicles.Should().HaveCount(2, "Station-specific vehicles should be parsed into StationAssignment.Vehicles");

            // Verify NOT added to rank vehicles
            ranks[0].Vehicles.Should().BeEmpty("Station-specific vehicles should NOT be in Rank.Vehicles");
        }

        [Fact]
        public void ParseRanksFile_StationVehicle_ParsesModelAndDisplayName()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Vehicles>
          <Vehicle model=""police3"">Station Special</Vehicle>
        </Vehicles>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var vehicle = ranks[0].Stations[0].Vehicles.Should().ContainSingle().Subject;
            vehicle.Model.Should().Be("police3");
            vehicle.DisplayName.Should().Be("Station Special");
        }

        [Fact]
        public void ParseRanksFile_RankWithBothGlobalAndStationVehicles_ParsesBothCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Vehicles>
      <Vehicle model=""police"">Global Car</Vehicle>
    </Vehicles>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Vehicles>
          <Vehicle model=""police2"">Station Car</Vehicle>
        </Vehicles>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Vehicles.Should().ContainSingle()
                .Which.Model.Should().Be("police", "Global vehicle should be in Rank.Vehicles");

            var station = rank.Stations.Should().ContainSingle().Subject;
            station.Vehicles.Should().ContainSingle()
                .Which.Model.Should().Be("police2", "Station vehicle should be in StationAssignment.Vehicles");
        }

        [Fact]
        public void ParseRanksFile_MultipleStationsWithDifferentVehicles_ParsesCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Vehicles>
          <Vehicle model=""police"">Mission Row Car</Vehicle>
        </Vehicles>
      </Station>
      <Station>
        <StationName>Vespucci</StationName>
        <StyleID>2</StyleID>
        <Vehicles>
          <Vehicle model=""police2"">Vespucci Car</Vehicle>
        </Vehicles>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Stations.Should().HaveCount(2);
            rank.Stations[0].Vehicles.Should().ContainSingle().Which.Model.Should().Be("police");
            rank.Stations[1].Vehicles.Should().ContainSingle().Which.Model.Should().Be("police2");
        }

        #endregion

        #region F. CRITICAL: Station-Specific Outfits Parsing (4 tests)

        [Fact]
        public void ParseRanksFile_StationWithOutfits_ParsesOutfitsIntoStation()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Outfits>
          <Outfit>MissionRow_Uniform</Outfit>
          <Outfit>MissionRow_Tactical</Outfit>
        </Outfits>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.Outfits.Should().HaveCount(2, "Station-specific outfits should be parsed into StationAssignment.Outfits");

            // Verify NOT added to rank outfits
            ranks[0].Outfits.Should().BeEmpty("Station-specific outfits should NOT be in Rank.Outfits");
        }

        [Fact]
        public void ParseRanksFile_RankWithBothGlobalAndStationOutfits_ParsesBothCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Outfits>
      <Outfit>LSPD_Global_Uniform</Outfit>
    </Outfits>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Outfits>
          <Outfit>MissionRow_Uniform</Outfit>
        </Outfits>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Outfits.Should().ContainSingle()
                .Which.Should().Be("LSPD_Global_Uniform", "Global outfit should be in Rank.Outfits");

            var station = rank.Stations.Should().ContainSingle().Subject;
            station.Outfits.Should().ContainSingle()
                .Which.Should().Be("MissionRow_Uniform", "Station outfit should be in StationAssignment.Outfits");
        }

        [Fact]
        public void ParseRanksFile_StationWithNoOutfits_CreatesEmptyOutfitsList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var station = ranks[0].Stations.Should().ContainSingle().Subject;
            station.Outfits.Should().NotBeNull();
            station.Outfits.Should().BeEmpty();
        }

        [Fact]
        public void ParseRanksFile_MultipleStationsWithDifferentOutfits_ParsesCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>0</RequiredPoints>
    <Salary>1000</Salary>
    <Stations>
      <Station>
        <StationName>Mission Row</StationName>
        <StyleID>1</StyleID>
        <Outfits>
          <Outfit>MissionRow_Uniform</Outfit>
        </Outfits>
      </Station>
      <Station>
        <StationName>Vespucci</StationName>
        <StyleID>2</StyleID>
        <Outfits>
          <Outfit>Vespucci_Uniform</Outfit>
        </Outfits>
      </Station>
    </Stations>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Stations.Should().HaveCount(2);
            rank.Stations[0].Outfits.Should().ContainSingle().Which.Should().Be("MissionRow_Uniform");
            rank.Stations[1].Outfits.Should().ContainSingle().Which.Should().Be("Vespucci_Uniform");
        }

        #endregion

        #region G. Missing/Optional Elements (3 tests)

        [Fact]
        public void ParseRanksFile_MissingRequiredPoints_DefaultsToZero()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.RequiredPoints.Should().Be(0, "Missing RequiredPoints should default to 0");
        }

        [Fact]
        public void ParseRanksFile_MissingSalary_DefaultsToZero()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>100</RequiredPoints>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Salary.Should().Be(0, "Missing Salary should default to 0");
        }

        [Fact]
        public void ParseRanksFile_MissingName_DefaultsToUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <RequiredPoints>100</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(xml);

            // Act
            var ranks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            var rank = ranks.Should().ContainSingle().Subject;
            rank.Name.Should().Be("Unknown", "Missing Name should default to 'Unknown'");
        }

        #endregion

        #region H. Malformed XML (1 test)

        [Fact]
        public void ParseRanksFile_MalformedXml_ThrowsExceptionWithDetails()
        {
            // Arrange
            var malformedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
  <Rank>
    <Name>Officer</Name>
    <RequiredPoints>NotANumber</RequiredPoints>
    <Salary>1000</Salary>
  </Rank>
</Ranks>";
            var tempFile = CreateTempXmlFileAndTrack(malformedXml);

            // Act & Assert
            Action act = () => RanksParser.ParseRanksFile(tempFile);
            act.Should().Throw<Exception>("Invalid data should throw exception");
        }

        #endregion
    }
}
