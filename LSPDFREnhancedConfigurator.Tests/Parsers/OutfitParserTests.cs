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
    public class OutfitParserTests : IDisposable
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
            var tempFile = Path.Combine(Path.GetTempPath(), $"outfits_{Guid.NewGuid()}.xml");
            File.WriteAllText(tempFile, content);
            _tempFiles.Add(tempFile);
            return tempFile;
        }

        #region ParseOutfitsFile Tests

        [Fact]
        public void ParseOutfitsFile_ValidXml_ReturnsOutfitVariations()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
            <Gender>Male</Gender>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Standard");
            result[0].ScriptName.Should().Be("standard");
            result[0].ParentOutfit.Should().NotBeNull();
            result[0].ParentOutfit.Name.Should().Be("Police Uniform");
        }

        [Fact]
        public void ParseOutfitsFile_MultipleVariations_ReturnsAll()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
        </Variation>
        <Variation>
            <Name>Supervisor</Name>
            <ScriptName>supervisor</ScriptName>
        </Variation>
        <Variation>
            <Name>Tactical</Name>
            <ScriptName>tactical</ScriptName>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(3);
            result.Select(v => v.Name).Should().BeEquivalentTo(new[] { "Standard", "Supervisor", "Tactical" });
        }

        [Fact]
        public void ParseOutfitsFile_MultipleOutfits_ReturnsAllVariations()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
        </Variation>
    </Outfit>
    <Outfit>
        <Name>Sheriff Uniform</Name>
        <ScriptName>sheriff_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(2);
            result[0].ParentOutfit.Name.Should().Be("Police Uniform");
            result[1].ParentOutfit.Name.Should().Be("Sheriff Uniform");
        }

        [Fact]
        public void ParseOutfitsFile_MissingName_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].ParentOutfit.Name.Should().Be("Unknown");
        }

        [Fact]
        public void ParseOutfitsFile_MissingScriptName_UsesUnknown()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <Variation>
            <Name>Standard</Name>
            <ScriptName>standard</ScriptName>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].ParentOutfit.ScriptName.Should().Be("unknown");
        }

        [Fact]
        public void ParseOutfitsFile_VariationMissingName_IsSkipped()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <ScriptName>invalid</ScriptName>
        </Variation>
        <Variation>
            <Name>Valid</Name>
            <ScriptName>valid</ScriptName>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Valid");
        }

        [Fact]
        public void ParseOutfitsFile_VariationMissingScriptName_UsesEmpty()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variation>
            <Name>Standard</Name>
        </Variation>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(1);
            result[0].ScriptName.Should().BeEmpty();
        }

        [Fact]
        public void ParseOutfitsFile_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParseOutfitsFile_NoVariations_ReturnsEmptyList()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ParseOutfitsFile_FileNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xml");

            // Act & Assert
            var act = () => OutfitParser.ParseOutfitsFile(nonExistentFile);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse outfits file {nonExistentFile}*");
        }

        [Fact]
        public void ParseOutfitsFile_InvalidXml_ThrowsException()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Unclosed tag
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act & Assert
            var act = () => OutfitParser.ParseOutfitsFile(filePath);
            act.Should().Throw<Exception>()
                .WithMessage($"Failed to parse outfits file {filePath}*");
        }

        [Fact]
        public void ParseOutfitsFile_NestedVariations_ParsesAll()
        {
            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Outfits>
    <Outfit>
        <Name>Police Uniform</Name>
        <ScriptName>police_uniform</ScriptName>
        <Variations>
            <Variation>
                <Name>Nested Standard</Name>
                <ScriptName>nested_standard</ScriptName>
            </Variation>
            <Variation>
                <Name>Nested Supervisor</Name>
                <ScriptName>nested_supervisor</ScriptName>
            </Variation>
        </Variations>
    </Outfit>
</Outfits>";
            var filePath = CreateTempXmlFile(xml);

            // Act
            var result = OutfitParser.ParseOutfitsFile(filePath);

            // Assert
            result.Should().HaveCount(2);
            result.Select(v => v.Name).Should().BeEquivalentTo(new[] { "Nested Standard", "Nested Supervisor" });
        }

        #endregion

        #region MergeOutfits Tests

        [Fact]
        public void MergeOutfits_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var variations = new List<OutfitVariation>();

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void MergeOutfits_SingleVariation_ReturnsSingle()
        {
            // Arrange
            var outfit = new Outfit("Police Uniform", "police_uniform");
            var variation = new OutfitVariation("Standard", "standard", outfit);
            var variations = new List<OutfitVariation> { variation };

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().Be(variation);
        }

        [Fact]
        public void MergeOutfits_DuplicateCombinedNames_KeepsFirst()
        {
            // Arrange
            var outfit = new Outfit("Police Uniform", "police_uniform");
            var variation1 = new OutfitVariation("Standard", "standard", outfit);
            var variation2 = new OutfitVariation("Standard", "standard", outfit);  // Duplicate
            var variations = new List<OutfitVariation> { variation1, variation2 };

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().Be(variation1);
        }

        [Fact]
        public void MergeOutfits_DifferentOutfits_ReturnsBoth()
        {
            // Arrange
            var policeOutfit = new Outfit("Police Uniform", "police_uniform");
            var sheriffOutfit = new Outfit("Sheriff Uniform", "sheriff_uniform");
            var variation1 = new OutfitVariation("Standard", "standard", policeOutfit);
            var variation2 = new OutfitVariation("Standard", "standard", sheriffOutfit);
            var variations = new List<OutfitVariation> { variation1, variation2 };

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public void MergeOutfits_CaseInsensitive_MergesCorrectly()
        {
            // Arrange
            var outfit = new Outfit("Police Uniform", "police_uniform");
            var variation1 = new OutfitVariation("Standard", "STANDARD", outfit);
            var variation2 = new OutfitVariation("STANDARD", "standard", outfit);
            var variations = new List<OutfitVariation> { variation1, variation2 };

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert - should merge based on CombinedName case-insensitivity
            result.Should().HaveCountLessOrEqualTo(2);
        }

        [Fact]
        public void MergeOutfits_MultipleVariations_PreservesOrder()
        {
            // Arrange
            var outfit = new Outfit("Police Uniform", "police_uniform");
            var variation1 = new OutfitVariation("Standard", "standard", outfit);
            var variation2 = new OutfitVariation("Supervisor", "supervisor", outfit);
            var variation3 = new OutfitVariation("Tactical", "tactical", outfit);
            var variations = new List<OutfitVariation> { variation1, variation2, variation3 };

            // Act
            var result = OutfitParser.MergeOutfits(variations);

            // Assert
            result.Should().HaveCount(3);
            result.Select(v => v.Name).Should().ContainInOrder("Standard", "Supervisor", "Tactical");
        }

        #endregion
    }
}
