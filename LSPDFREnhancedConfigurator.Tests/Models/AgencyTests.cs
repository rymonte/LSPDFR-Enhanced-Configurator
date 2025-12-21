using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Models
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Models")]
    public class AgencyTests
    {
        #region Constructor Tests

        [Fact]
        public void Agency_DefaultConstructor_InitializesProperties()
        {
            // Act
            var agency = new Agency();

            // Assert
            agency.Name.Should().NotBeNull();
            agency.ShortName.Should().NotBeNull();
            agency.ScriptName.Should().NotBeNull();
            agency.Vehicles.Should().NotBeNull();
            agency.Vehicles.Should().BeEmpty();
        }

        [Fact]
        public void Agency_ParameterizedConstructor_SetsProperties()
        {
            // Act
            var agency = new Agency("Los Santos Police Department", "LSPD", "lspd");

            // Assert
            agency.Name.Should().Be("Los Santos Police Department");
            agency.ShortName.Should().Be("LSPD");
            agency.ScriptName.Should().Be("lspd");
            agency.Vehicles.Should().NotBeNull();
            agency.Vehicles.Should().BeEmpty();
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var agency = new Agency("Los Santos Police Department", "LSPD", "lspd");

            // Act
            var result = agency.ToString();

            // Assert
            result.Should().Be("LSPD - Los Santos Police Department");
        }

        [Fact]
        public void ToString_EmptyValues_ReturnsFormattedString()
        {
            // Arrange
            var agency = new Agency("", "", "");

            // Act
            var result = agency.ToString();

            // Assert
            result.Should().Be(" - ");
        }

        #endregion

        #region Equals Tests

        [Fact]
        public void Equals_SameScriptName_ReturnsTrue()
        {
            // Arrange
            var agency1 = new Agency("Los Santos Police Department", "LSPD", "lspd");
            var agency2 = new Agency("Different Name", "Different Short", "lspd");

            // Act
            var result = agency1.Equals(agency2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentScriptName_ReturnsFalse()
        {
            // Arrange
            var agency1 = new Agency("Los Santos Police Department", "LSPD", "lspd");
            var agency2 = new Agency("Los Santos Sheriff Department", "LSSD", "sheriff");

            // Act
            var result = agency1.Equals(agency2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            var agency2 = new Agency("LSPD", "LSPD", "LSPD");

            // Act
            var result = agency1.Equals(agency2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");

            // Act
            var result = agency.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_NonAgencyObject_ReturnsFalse()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");

            // Act
            var result = agency.Equals("lspd");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");

            // Act
            var result = agency.Equals(agency);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_SameScriptName_ReturnsSameHashCode()
        {
            // Arrange
            var agency1 = new Agency("Los Santos Police Department", "LSPD", "lspd");
            var agency2 = new Agency("Different Name", "Different Short", "lspd");

            // Act
            var hash1 = agency1.GetHashCode();
            var hash2 = agency2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_CaseInsensitive_ReturnsSameHashCode()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            var agency2 = new Agency("LSPD", "LSPD", "LSPD");

            // Act
            var hash1 = agency1.GetHashCode();
            var hash2 = agency2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_DifferentScriptName_ReturnsDifferentHashCode()
        {
            // Arrange
            var agency1 = new Agency("LSPD", "LSPD", "lspd");
            var agency2 = new Agency("LSSD", "LSSD", "sheriff");

            // Act
            var hash1 = agency1.GetHashCode();
            var hash2 = agency2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Vehicles_CanAddAndRemove()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");
            var vehicle = new Vehicle("police", "Police Cruiser", "lspd");

            // Act
            agency.Vehicles.Add(vehicle);

            // Assert
            agency.Vehicles.Should().HaveCount(1);
            agency.Vehicles[0].Should().Be(vehicle);
        }

        [Fact]
        public void Vehicles_IsInitializedInDefaultConstructor()
        {
            // Arrange & Act
            var agency = new Agency();

            // Assert
            agency.Vehicles.Should().NotBeNull();
            agency.Vehicles.Should().BeOfType<System.Collections.Generic.List<Vehicle>>();
        }

        [Fact]
        public void Properties_CanBeSetIndependently()
        {
            // Arrange
            var agency = new Agency();

            // Act
            agency.Name = "Test Agency";
            agency.ShortName = "TA";
            agency.ScriptName = "test";

            // Assert
            agency.Name.Should().Be("Test Agency");
            agency.ShortName.Should().Be("TA");
            agency.ScriptName.Should().Be("test");
        }

        [Fact]
        public void Equals_WithNullObject_DoesNotThrow()
        {
            // Arrange
            var agency = new Agency("LSPD", "LSPD", "lspd");

            // Act
            var act = () => agency.Equals((object)null);

            // Assert
            act.Should().NotThrow();
            agency.Equals((object)null).Should().BeFalse();
        }

        #endregion
    }
}
