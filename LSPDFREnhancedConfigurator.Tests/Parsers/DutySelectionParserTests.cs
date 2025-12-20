using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Parsers;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Parsers
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Parsers")]
    public class DutySelectionParserTests : IDisposable
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
            var tempFile = Path.Combine(Path.GetTempPath(), $"duty_selection_{Guid.NewGuid()}.xml");
            File.WriteAllText(tempFile, content);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        #region ParseDutySelectionFile Tests

        [Fact]
        public void ParseDutySelectionFile_ValidXml_ReturnsVehicleDescriptions()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Police Cruiser (Interceptor)"" agencyRef=""LSPD"">police</Description>
    <Description fullName=""Sheriff Cruiser"" agencyRef=""BCSO"">sheriff</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainKey("police");
            result["police"].Model.Should().Be("police");
            result["police"].FullName.Should().Be("Police Cruiser (Interceptor)");
            result["police"].AgencyRef.Should().Be("LSPD");
        }

        [Fact]
        public void ParseDutySelectionFile_MissingFullName_UsesModelAsFullName()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description agencyRef=""LSPD"">police</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result["police"].Model.Should().Be("police");
            result["police"].FullName.Should().Be("police");
        }

        [Fact]
        public void ParseDutySelectionFile_MissingAgencyRef_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Police Cruiser"">police</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result["police"].AgencyRef.Should().Be("unknown");
        }

        [Fact]
        public void ParseDutySelectionFile_EmptyModel_IsSkipped()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Empty Model"" agencyRef=""LSPD""></Description>
    <Description fullName=""Valid"" agencyRef=""LSPD"">valid</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainKey("valid");
            result.Should().NotContainKey("");
        }

        [Fact]
        public void ParseDutySelectionFile_WhitespaceModel_IsTrimmed()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Police Cruiser"" agencyRef=""LSPD"">  police  </Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainKey("police");
            result["police"].Model.Should().Be("police");
        }

        [Fact]
        public void ParseDutySelectionFile_CaseInsensitiveLookup_WorksCorrectly()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Police Cruiser"" agencyRef=""LSPD"">POLICE</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().ContainKey("police");
            result.Should().ContainKey("POLICE");
            result.Should().ContainKey("Police");
        }

        [Fact]
        public void ParseDutySelectionFile_DuplicateModels_LastOneWins()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""First Description"" agencyRef=""LSPD"">police</Description>
    <Description fullName=""Second Description"" agencyRef=""BCSO"">police</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result["police"].FullName.Should().Be("Second Description");
            result["police"].AgencyRef.Should().Be("BCSO");
        }

        [Fact]
        public void ParseDutySelectionFile_EmptyFile_ReturnsEmptyDictionary()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParseDutySelectionFile_NoDescriptionElements_ReturnsEmptyDictionary()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <SomeOtherElement>value</SomeOtherElement>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParseDutySelectionFile_FileNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xml");

            // Act & Assert
            var act = () => DutySelectionParser.ParseDutySelectionFile(nonExistentFile);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse duty selection file {nonExistentFile}*");
        }

        [Fact]
        public void ParseDutySelectionFile_InvalidXml_ThrowsException()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description>unclosed tag
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act & Assert
            var act = () => DutySelectionParser.ParseDutySelectionFile(filePath);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse duty selection file {filePath}*");
        }

        [Fact]
        public void ParseDutySelectionFile_MultipleVehicles_ParsesAll()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DutySelection>
    <Description fullName=""Police Cruiser"" agencyRef=""LSPD"">police</Description>
    <Description fullName=""Police SUV"" agencyRef=""LSPD"">police2</Description>
    <Description fullName=""Sheriff Cruiser"" agencyRef=""BCSO"">sheriff</Description>
    <Description fullName=""State Patrol"" agencyRef=""SAHP"">police3</Description>
</DutySelection>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = DutySelectionParser.ParseDutySelectionFile(filePath);

            // Assert
            result.Should().HaveCount(4);
            result.Keys.Should().BeEquivalentTo(new[] { "police", "police2", "sheriff", "police3" });
        }

        #endregion

        #region MergeDescriptions Tests

        [Fact]
        public void MergeDescriptions_SingleDictionary_ReturnsCopy()
        {
            // Arrange
            var dict1 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription
                {
                    Model = "police",
                    FullName = "Police Cruiser",
                    AgencyRef = "LSPD"
                }
            };

            // Act
            var result = DutySelectionParser.MergeDescriptions(dict1);

            // Assert
            result.Should().HaveCount(1);
            result["police"].FullName.Should().Be("Police Cruiser");
        }

        [Fact]
        public void MergeDescriptions_MultipleDictionaries_MergesAll()
        {
            // Arrange
            var dict1 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription { Model = "police", FullName = "Police Cruiser", AgencyRef = "LSPD" }
            };

            var dict2 = new Dictionary<string, VehicleDescription>
            {
                ["sheriff"] = new VehicleDescription { Model = "sheriff", FullName = "Sheriff Cruiser", AgencyRef = "BCSO" }
            };

            var dict3 = new Dictionary<string, VehicleDescription>
            {
                ["police3"] = new VehicleDescription { Model = "police3", FullName = "State Patrol", AgencyRef = "SAHP" }
            };

            // Act
            var result = DutySelectionParser.MergeDescriptions(dict1, dict2, dict3);

            // Assert
            result.Should().HaveCount(3);
            result.Should().ContainKey("police");
            result.Should().ContainKey("sheriff");
            result.Should().ContainKey("police3");
        }

        [Fact]
        public void MergeDescriptions_DuplicateKeys_LastOneWins()
        {
            // Arrange
            var dict1 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription { Model = "police", FullName = "First", AgencyRef = "LSPD" }
            };

            var dict2 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription { Model = "police", FullName = "Second", AgencyRef = "BCSO" }
            };

            var dict3 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription { Model = "police", FullName = "Third", AgencyRef = "SAHP" }
            };

            // Act
            var result = DutySelectionParser.MergeDescriptions(dict1, dict2, dict3);

            // Assert
            result.Should().HaveCount(1);
            result["police"].FullName.Should().Be("Third");
            result["police"].AgencyRef.Should().Be("SAHP");
        }

        [Fact]
        public void MergeDescriptions_EmptyDictionaries_ReturnsEmpty()
        {
            // Arrange
            var dict1 = new Dictionary<string, VehicleDescription>();
            var dict2 = new Dictionary<string, VehicleDescription>();

            // Act
            var result = DutySelectionParser.MergeDescriptions(dict1, dict2);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void MergeDescriptions_NoDictionaries_ReturnsEmpty()
        {
            // Act
            var result = DutySelectionParser.MergeDescriptions();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void MergeDescriptions_CaseInsensitiveMerge_WorksCorrectly()
        {
            // Arrange
            var dict1 = new Dictionary<string, VehicleDescription>
            {
                ["POLICE"] = new VehicleDescription { Model = "POLICE", FullName = "First", AgencyRef = "LSPD" }
            };

            var dict2 = new Dictionary<string, VehicleDescription>
            {
                ["police"] = new VehicleDescription { Model = "police", FullName = "Second", AgencyRef = "BCSO" }
            };

            // Act
            var result = DutySelectionParser.MergeDescriptions(dict1, dict2);

            // Assert
            result.Should().HaveCount(1);
            result["POLICE"].FullName.Should().Be("Second");
            result["police"].FullName.Should().Be("Second");
        }

        #endregion
    }
}
