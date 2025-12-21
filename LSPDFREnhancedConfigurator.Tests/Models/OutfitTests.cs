using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Models
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Models")]
    public class OutfitTests
    {
        #region Outfit Constructor Tests

        [Fact]
        public void Outfit_DefaultConstructor_InitializesProperties()
        {
            // Act
            var outfit = new Outfit();

            // Assert
            outfit.Name.Should().NotBeNull();
            outfit.ScriptName.Should().NotBeNull();
            outfit.Variations.Should().NotBeNull();
            outfit.Variations.Should().BeEmpty();
            outfit.InferredAgency.Should().NotBeNull();
        }

        [Fact]
        public void Outfit_ParameterizedConstructor_SetsProperties()
        {
            // Act
            var outfit = new Outfit("LSPD Class A", "lspd_class_a");

            // Assert
            outfit.Name.Should().Be("LSPD Class A");
            outfit.ScriptName.Should().Be("lspd_class_a");
            outfit.Variations.Should().NotBeNull();
            outfit.Variations.Should().BeEmpty();
        }

        #endregion

        #region Outfit InferAgency Tests

        [Theory]
        [InlineData("LSPD Class A", "LSPD")]
        [InlineData("lspd patrol", "LSPD")]
        [InlineData("Officer LSPD", "LSPD")]
        public void Outfit_Constructor_InfersLSPDAgency(string name, string expectedAgency)
        {
            // Act
            var outfit = new Outfit(name, "script");

            // Assert
            outfit.InferredAgency.Should().Be(expectedAgency);
        }

        [Theory]
        [InlineData("LSSD Class A", "LSSD")]
        [InlineData("Sheriff Patrol", "LSSD")]
        [InlineData("Blaine County Sheriff", "LSSD")]
        public void Outfit_Constructor_InfersLSSDAgency(string name, string expectedAgency)
        {
            // Act
            var outfit = new Outfit(name, "script");

            // Assert
            outfit.InferredAgency.Should().Be(expectedAgency);
        }

        [Fact]
        public void Outfit_Constructor_InfersBCSOAgency()
        {
            // Act
            var outfit = new Outfit("BCSO Deputy", "script");

            // Assert
            outfit.InferredAgency.Should().Be("BCSO");
        }

        [Theory]
        [InlineData("SAHP Trooper", "SAHP")]
        [InlineData("Highway Patrol", "SAHP")]
        public void Outfit_Constructor_InfersSAHPAgency(string name, string expectedAgency)
        {
            // Act
            var outfit = new Outfit(name, "script");

            // Assert
            outfit.InferredAgency.Should().Be(expectedAgency);
        }

        [Theory]
        [InlineData("SASP Ranger", "SASP")]
        [InlineData("State Ranger", "SASP")]
        public void Outfit_Constructor_InfersSASPAgency(string name, string expectedAgency)
        {
            // Act
            var outfit = new Outfit(name, "script");

            // Assert
            outfit.InferredAgency.Should().Be(expectedAgency);
        }

        [Fact]
        public void Outfit_Constructor_InfersUnknownAgency()
        {
            // Act
            var outfit = new Outfit("Custom Outfit", "script");

            // Assert
            outfit.InferredAgency.Should().Be("Unknown");
        }

        [Fact]
        public void Outfit_Constructor_InferAgencyCaseInsensitive()
        {
            // Arrange & Act
            var outfit1 = new Outfit("lspd class a", "script");
            var outfit2 = new Outfit("LSPD CLASS A", "script");
            var outfit3 = new Outfit("LsPd Class A", "script");

            // Assert
            outfit1.InferredAgency.Should().Be("LSPD");
            outfit2.InferredAgency.Should().Be("LSPD");
            outfit3.InferredAgency.Should().Be("LSPD");
        }

        #endregion

        #region Outfit ToString Tests

        [Fact]
        public void Outfit_ToString_ReturnsName()
        {
            // Arrange
            var outfit = new Outfit("LSPD Class A", "script");

            // Act
            var result = outfit.ToString();

            // Assert
            result.Should().Be("LSPD Class A");
        }

        #endregion

        #region OutfitVariation Constructor Tests

        [Fact]
        public void OutfitVariation_DefaultConstructor_InitializesProperties()
        {
            // Act
            var variation = new OutfitVariation();

            // Assert
            variation.Name.Should().NotBeNull();
            variation.ScriptName.Should().NotBeNull();
            variation.ParentOutfit.Should().BeNull();
        }

        [Fact]
        public void OutfitVariation_ParameterizedConstructor_SetsProperties()
        {
            // Arrange
            var parent = new Outfit("LSPD Class A", "lspd_class_a");

            // Act
            var variation = new OutfitVariation("Officer", "officer_1", parent);

            // Assert
            variation.Name.Should().Be("Officer");
            variation.ScriptName.Should().Be("officer_1");
            variation.ParentOutfit.Should().BeSameAs(parent);
        }

        #endregion

        #region OutfitVariation CombinedName Tests

        [Fact]
        public void OutfitVariation_CombinedName_WithParent_ReturnsFormattedName()
        {
            // Arrange
            var parent = new Outfit("LSPD Class A", "lspd_class_a");
            var variation = new OutfitVariation("Officer", "officer_1", parent);

            // Act
            var combinedName = variation.CombinedName;

            // Assert
            combinedName.Should().Be("LSPD Class A.Officer");
        }

        [Fact]
        public void OutfitVariation_CombinedName_WithoutParent_ReturnsNameOnly()
        {
            // Arrange
            var variation = new OutfitVariation { Name = "Officer" };

            // Act
            var combinedName = variation.CombinedName;

            // Assert
            combinedName.Should().Be("Officer");
        }

        #endregion

        #region OutfitVariation InferredGender Tests

        [Theory]
        [InlineData("officer_f_1", "Female")]
        [InlineData("cop_female_uniform", "Female")]
        [InlineData("female_officer", "Female")]
        public void OutfitVariation_InferredGender_Female_DetectsCorrectly(string scriptName, string expected)
        {
            // Arrange
            var variation = new OutfitVariation { ScriptName = scriptName };

            // Act
            var gender = variation.InferredGender;

            // Assert
            gender.Should().Be(expected);
        }

        [Theory]
        [InlineData("officer_m_1", "Male")]
        [InlineData("cop_male_uniform", "Male")]
        [InlineData("male_officer", "Male")]
        public void OutfitVariation_InferredGender_Male_DetectsCorrectly(string scriptName, string expected)
        {
            // Arrange
            var variation = new OutfitVariation { ScriptName = scriptName };

            // Act
            var gender = variation.InferredGender;

            // Assert
            gender.Should().Be(expected);
        }

        [Fact]
        public void OutfitVariation_InferredGender_Unisex_WhenNoGenderIndicator()
        {
            // Arrange
            var variation = new OutfitVariation { ScriptName = "officer_1" };

            // Act
            var gender = variation.InferredGender;

            // Assert
            gender.Should().Be("Unisex");
        }

        [Fact]
        public void OutfitVariation_InferredGender_NullScriptName_ReturnsUnisex()
        {
            // Arrange
            var variation = new OutfitVariation { ScriptName = null };

            // Act
            var gender = variation.InferredGender;

            // Assert
            gender.Should().Be("Unisex");
        }

        [Fact]
        public void OutfitVariation_InferredGender_CaseInsensitive()
        {
            // Arrange
            var variation1 = new OutfitVariation { ScriptName = "officer_F_1" };
            var variation2 = new OutfitVariation { ScriptName = "OFFICER_M_1" };

            // Act & Assert
            variation1.InferredGender.Should().Be("Female");
            variation2.InferredGender.Should().Be("Male");
        }

        #endregion

        #region OutfitVariation ToString Tests

        [Fact]
        public void OutfitVariation_ToString_ReturnsCombinedName()
        {
            // Arrange
            var parent = new Outfit("LSPD Class A", "lspd_class_a");
            var variation = new OutfitVariation("Officer", "officer_1", parent);

            // Act
            var result = variation.ToString();

            // Assert
            result.Should().Be("LSPD Class A.Officer");
        }

        #endregion

        #region OutfitVariation Equals Tests

        [Fact]
        public void OutfitVariation_Equals_SameCombinedName_ReturnsTrue()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("LSPD Class A", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("Officer", "officer_2", parent2);

            // Act
            var result = variation1.Equals(variation2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OutfitVariation_Equals_DifferentCombinedName_ReturnsFalse()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("LSPD Class A", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("Detective", "detective_1", parent2);

            // Act
            var result = variation1.Equals(variation2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void OutfitVariation_Equals_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("lspd class a", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("officer", "officer_2", parent2);

            // Act
            var result = variation1.Equals(variation2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OutfitVariation_Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var variation = new OutfitVariation("Officer", "officer_1", null);

            // Act
            var result = variation.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void OutfitVariation_Equals_NonOutfitVariationObject_ReturnsFalse()
        {
            // Arrange
            var variation = new OutfitVariation("Officer", "officer_1", null);

            // Act
            var result = variation.Equals("Officer");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region OutfitVariation GetHashCode Tests

        [Fact]
        public void OutfitVariation_GetHashCode_SameCombinedName_ReturnsSameHashCode()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("LSPD Class A", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("Officer", "officer_2", parent2);

            // Act
            var hash1 = variation1.GetHashCode();
            var hash2 = variation2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void OutfitVariation_GetHashCode_CaseInsensitive_ReturnsSameHashCode()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("lspd class a", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("officer", "officer_2", parent2);

            // Act
            var hash1 = variation1.GetHashCode();
            var hash2 = variation2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void OutfitVariation_GetHashCode_DifferentCombinedName_ReturnsDifferentHashCode()
        {
            // Arrange
            var parent1 = new Outfit("LSPD Class A", "lspd_class_a");
            var parent2 = new Outfit("LSPD Class A", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", parent1);
            var variation2 = new OutfitVariation("Detective", "detective_1", parent2);

            // Act
            var hash1 = variation1.GetHashCode();
            var hash2 = variation2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Outfit_CanAddVariations()
        {
            // Arrange
            var outfit = new Outfit("LSPD Class A", "lspd_class_a");
            var variation1 = new OutfitVariation("Officer", "officer_1", outfit);
            var variation2 = new OutfitVariation("Detective", "detective_1", outfit);

            // Act
            outfit.Variations.Add(variation1);
            outfit.Variations.Add(variation2);

            // Assert
            outfit.Variations.Should().HaveCount(2);
            outfit.Variations[0].Should().BeSameAs(variation1);
            outfit.Variations[1].Should().BeSameAs(variation2);
        }

        [Fact]
        public void OutfitVariation_WithoutParent_CombinedNameEqualsName()
        {
            // Arrange
            var variation = new OutfitVariation("Officer", "officer_1", null);

            // Act & Assert
            variation.CombinedName.Should().Be("Officer");
            variation.ToString().Should().Be("Officer");
        }

        #endregion
    }
}
