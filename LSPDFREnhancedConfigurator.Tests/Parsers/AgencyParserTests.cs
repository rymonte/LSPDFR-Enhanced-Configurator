using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Parsers;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Parsers
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Parsers")]
    public class AgencyParserTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        public void Dispose()
        {
            // Cleanup temp files
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private string CreateTempXmlFile(string content)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"agency_{Guid.NewGuid()}.xml");
            File.WriteAllText(tempFile, content);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        #region ParseAgencyFile Tests

        [Fact]
        public void ParseAgencyFile_ValidXml_ReturnsAgencies()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>Los Santos Police Department</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
            <Vehicle>police2</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Los Santos Police Department");
            result[0].ShortName.Should().Be("LSPD");
            result[0].ScriptName.Should().Be("lspd");
            result[0].Vehicles.Should().HaveCount(2);
        }

        [Fact]
        public void ParseAgencyFile_MultipleAgencies_ReturnsAll()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
        </Loadout>
    </Agency>
    <Agency>
        <Name>BCSO</Name>
        <ShortName>BCSO</ShortName>
        <ScriptName>bcso</ScriptName>
        <Loadout>
            <Vehicle>sheriff</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(2);
            result[0].ScriptName.Should().Be("lspd");
            result[1].ScriptName.Should().Be("bcso");
        }

        [Fact]
        public void ParseAgencyFile_MultipleLoadouts_CombinesVehicles()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
            <Vehicle>police2</Vehicle>
        </Loadout>
        <Loadout>
            <Vehicle>police3</Vehicle>
            <Vehicle>police4</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(4);
            result[0].Vehicles.Select(v => v.Model).Should().BeEquivalentTo(new[] { "police", "police2", "police3", "police4" });
        }

        [Fact]
        public void ParseAgencyFile_DuplicateVehicles_AddOnlyOnce()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
            <Vehicle>POLICE</Vehicle>
            <Vehicle>Police</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
            result[0].Vehicles[0].Model.Should().Be("police");
        }

        [Fact]
        public void ParseAgencyFile_MissingName_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Unknown");
        }

        [Fact]
        public void ParseAgencyFile_MissingShortName_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].ShortName.Should().Be("Unknown");
        }

        [Fact]
        public void ParseAgencyFile_MissingScriptName_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <Loadout>
            <Vehicle>police</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].ScriptName.Should().Be("unknown");
        }

        [Fact]
        public void ParseAgencyFile_EmptyVehicle_IsSkipped()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle></Vehicle>
            <Vehicle>police</Vehicle>
            <Vehicle>   </Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
            result[0].Vehicles[0].Model.Should().Be("police");
        }

        [Fact]
        public void ParseAgencyFile_WhitespaceVehicle_IsTrimmed()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>  police  </Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
            result[0].Vehicles[0].Model.Should().Be("police");
        }

        [Fact]
        public void ParseAgencyFile_NoLoadouts_ReturnsEmptyVehicles()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().BeEmpty();
        }

        [Fact]
        public void ParseAgencyFile_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParseAgencyFile_FileNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xml");

            // Act & Assert
            var act = () => AgencyParser.ParseAgencyFile(nonExistentFile);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse agency file {nonExistentFile}*");
        }

        [Fact]
        public void ParseAgencyFile_InvalidXml_ThrowsException()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>Unclosed tag
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act & Assert
            var act = () => AgencyParser.ParseAgencyFile(filePath);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse agency file {filePath}*");
        }

        [Fact]
        public void ParseAgencyFile_VehicleAgencies_ContainsAgencyScriptName()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>LSPD</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Loadout>
            <Vehicle>police</Vehicle>
        </Loadout>
    </Agency>
</Agencies>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = AgencyParser.ParseAgencyFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
            result[0].Vehicles[0].Agencies.Should().Contain("lspd");
        }

        #endregion

        #region MergeAgencies Tests

        [Fact]
        public void MergeAgencies_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var agencies = new List<Agency>();

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void MergeAgencies_SingleAgency_ReturnsSingle()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");
            agency.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));
            var agencies = new List<Agency> { agency };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().Be(agency);
        }

        [Fact]
        public void MergeAgencies_DuplicateAgencies_MergesVehicles()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            agency1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var agency2 = new Agency("LSPD", "LSPD", "lspd");
            agency2.Vehicles.Add(new Vehicle("police2", "Police SUV", "lspd"));

            var agencies = new List<Agency> { agency1, agency2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(2);
            result[0].Vehicles.Select(v => v.Model).Should().BeEquivalentTo(new[] { "police", "police2" });
        }

        [Fact]
        public void MergeAgencies_DuplicateVehiclesInMerge_AddOnlyOnce()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            agency1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var agency2 = new Agency("LSPD", "LSPD", "lspd");
            agency2.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd")); // Duplicate

            var agencies = new List<Agency> { agency1, agency2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
        }

        [Fact]
        public void MergeAgencies_CaseInsensitiveAgencyScriptName_Merges()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "LSPD");
            agency1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var agency2 = new Agency("LSPD", "LSPD", "lspd"); // Different case
            agency2.Vehicles.Add(new Vehicle("police2", "Police SUV", "lspd"));

            var agencies = new List<Agency> { agency1, agency2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(2);
        }

        [Fact]
        public void MergeAgencies_CaseInsensitiveVehicleModel_AddOnlyOnce()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            agency1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var agency2 = new Agency("LSPD", "LSPD", "lspd");
            agency2.Vehicles.Add(new Vehicle("POLICE", "Police Cruiser", "lspd")); // Different case

            var agencies = new List<Agency> { agency1, agency2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Vehicles.Should().HaveCount(1);
        }

        [Fact]
        public void MergeAgencies_DifferentAgencies_ReturnsBoth()
        {
            // Arrange
            var lspd = new Agency("LSPD", "LSPD", "lspd");
            lspd.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var bcso = new Agency("BCSO", "BCSO", "bcso");
            bcso.Vehicles.Add(new Vehicle("sheriff", "Sheriff Cruiser", "bcso"));

            var agencies = new List<Agency> { lspd, bcso };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.ScriptName == "lspd");
            result.Should().Contain(a => a.ScriptName == "bcso");
        }

        [Fact]
        public void MergeAgencies_MultipleAgenciesWithDuplicates_MergesCorrectly()
        {
            // Arrange
            var lspd1 = new Agency("LSPD", "LSPD", "lspd");
            lspd1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var bcso = new Agency("BCSO", "BCSO", "bcso");
            bcso.Vehicles.Add(new Vehicle("sheriff", "Sheriff Cruiser", "bcso"));

            var lspd2 = new Agency("LSPD", "LSPD", "lspd");
            lspd2.Vehicles.Add(new Vehicle("police2", "Police SUV", "lspd"));

            var agencies = new List<Agency> { lspd1, bcso, lspd2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(2);
            var lspdResult = result.First(a => a.ScriptName == "lspd");
            lspdResult.Vehicles.Should().HaveCount(2);

            var bcsoResult = result.First(a => a.ScriptName == "bcso");
            bcsoResult.Vehicles.Should().HaveCount(1);
        }

        [Fact]
        public void MergeAgencies_PreservesFirstAgencyName()
        {
            // Arrange
            var agency1 = new Agency("First Name", "LSPD", "lspd");
            agency1.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            var agency2 = new Agency("Second Name", "LSPD", "lspd");
            agency2.Vehicles.Add(new Vehicle("police2", "Police SUV", "lspd"));

            var agencies = new List<Agency> { agency1, agency2 };

            // Act
            var result = AgencyParser.MergeAgencies(agencies);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("First Name"); // Should keep first agency's name
        }

        #endregion
    }
}
