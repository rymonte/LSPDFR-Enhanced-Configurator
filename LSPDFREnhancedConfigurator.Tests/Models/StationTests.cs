using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Models
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Models")]
    public class StationTests
    {
        #region Station Constructor Tests

        [Fact]
        public void Station_DefaultConstructor_InitializesProperties()
        {
            // Act
            var station = new Station();

            // Assert
            station.Name.Should().NotBeNull();
            station.Agency.Should().NotBeNull();
            station.ScriptName.Should().NotBeNull();
            station.Position.Should().NotBeNull();
            station.Heading.Should().NotBeNull();
        }

        [Fact]
        public void Station_ParameterizedConstructor_SetsProperties()
        {
            // Act
            var station = new Station("Mission Row", "lspd", "lspd");

            // Assert
            station.Name.Should().Be("Mission Row");
            station.Agency.Should().Be("lspd");
            station.ScriptName.Should().Be("lspd");
        }

        #endregion

        #region ParsePosition Tests

        [Fact]
        public void ParsePosition_ValidPosition_ParsesCoordinates()
        {
            // Arrange
            var station = new Station { Position = "460.3052f, -990.7862f, 30.68962f" };

            // Act
            station.ParsePosition();

            // Assert
            station.X.Should().BeApproximately(460.3052f, 0.001f);
            station.Y.Should().BeApproximately(-990.7862f, 0.001f);
            station.HasValidCoordinates.Should().BeTrue();
        }

        [Fact]
        public void ParsePosition_EmptyPosition_DoesNotSetCoordinates()
        {
            // Arrange
            var station = new Station { Position = "" };

            // Act
            station.ParsePosition();

            // Assert
            station.X.Should().BeNull();
            station.Y.Should().BeNull();
            station.HasValidCoordinates.Should().BeFalse();
        }

        [Fact]
        public void ParsePosition_InvalidPosition_DoesNotSetCoordinates()
        {
            // Arrange
            var station = new Station { Position = "invalid" };

            // Act
            station.ParsePosition();

            // Assert
            station.HasValidCoordinates.Should().BeFalse();
        }

        #endregion

        #region DisplayName and ToString Tests

        [Fact]
        public void DisplayName_ReturnsFormattedName()
        {
            // Arrange
            var station = new Station("Mission Row", "lspd", "lspd");

            // Act
            var displayName = station.DisplayName;

            // Assert
            displayName.Should().Be("[LSPD] Mission Row");
        }

        [Fact]
        public void ToString_ReturnsDisplayName()
        {
            // Arrange
            var station = new Station("Mission Row", "lspd", "lspd");

            // Act
            var result = station.ToString();

            // Assert
            result.Should().Be("[LSPD] Mission Row");
        }

        #endregion

        #region Equals and GetHashCode Tests

        [Fact]
        public void Equals_SameName_ReturnsTrue()
        {
            // Arrange
            var station1 = new Station("Mission Row", "lspd", "lspd");
            var station2 = new Station("Mission Row", "sheriff", "sheriff");

            // Act & Assert
            station1.Equals(station2).Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentName_ReturnsFalse()
        {
            // Arrange
            var station1 = new Station("Mission Row", "lspd", "lspd");
            var station2 = new Station("Sandy Shores", "sheriff", "sheriff");

            // Act & Assert
            station1.Equals(station2).Should().BeFalse();
        }

        [Fact]
        public void Equals_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var station1 = new Station("Mission Row", "lspd", "lspd");
            var station2 = new Station("MISSION ROW", "lspd", "lspd");

            // Act & Assert
            station1.Equals(station2).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_SameName_ReturnsSameHash()
        {
            // Arrange
            var station1 = new Station("Mission Row", "lspd", "lspd");
            var station2 = new Station("Mission Row", "sheriff", "sheriff");

            // Act & Assert
            station1.GetHashCode().Should().Be(station2.GetHashCode());
        }

        #endregion

        #region StationAssignment Tests

        [Fact]
        public void StationAssignment_DefaultConstructor_InitializesCollections()
        {
            // Act
            var assignment = new StationAssignment();

            // Assert
            assignment.StationName.Should().NotBeNull();
            assignment.Zones.Should().NotBeNull();
            assignment.Vehicles.Should().NotBeNull();
            assignment.Outfits.Should().NotBeNull();
        }

        [Fact]
        public void StationAssignment_ParameterizedConstructor_SetsProperties()
        {
            // Arrange
            var zones = new List<string> { "zone1", "zone2" };

            // Act
            var assignment = new StationAssignment("Mission Row", zones, 1);

            // Assert
            assignment.StationName.Should().Be("Mission Row");
            assignment.Zones.Should().HaveCount(2);
            assignment.StyleID.Should().Be(1);
        }

        [Fact]
        public void StationAssignment_DisplayName_WithReference_ShowsAgency()
        {
            // Arrange
            var assignment = new StationAssignment { StationName = "Mission Row" };
            assignment.StationReference = new Station("Mission Row", "lspd", "lspd");

            // Act
            var displayName = assignment.DisplayName;

            // Assert
            displayName.Should().Be("[LSPD] Mission Row");
        }

        [Fact]
        public void StationAssignment_DisplayName_WithoutReference_ShowsUnknown()
        {
            // Arrange
            var assignment = new StationAssignment { StationName = "Mission Row" };

            // Act
            var displayName = assignment.DisplayName;

            // Assert
            displayName.Should().Be("[UNKNOWN] Mission Row");
        }

        [Fact]
        public void StationAssignment_IsValid_WithReference_ReturnsTrue()
        {
            // Arrange
            var assignment = new StationAssignment();
            assignment.StationReference = new Station();

            // Act & Assert
            assignment.IsValid.Should().BeTrue();
        }

        [Fact]
        public void StationAssignment_IsValid_WithoutReference_ReturnsFalse()
        {
            // Arrange
            var assignment = new StationAssignment();

            // Act & Assert
            assignment.IsValid.Should().BeFalse();
        }

        [Fact]
        public void StationAssignment_ToString_ReturnsFormattedString()
        {
            // Arrange
            var zones = new List<string> { "zone1", "zone2" };
            var assignment = new StationAssignment("Mission Row", zones, 1);

            // Act
            var result = assignment.ToString();

            // Assert
            result.Should().Be("Mission Row (Style: 1, Zones: 2)");
        }

        [Fact]
        public void StationAssignment_PropertyChanged_TriggersOnStationReferenceChange()
        {
            // Arrange
            var assignment = new StationAssignment();
            var propertyChangedFired = false;
            assignment.PropertyChanged += (s, e) => propertyChangedFired = true;

            // Act
            assignment.StationReference = new Station();

            // Assert
            propertyChangedFired.Should().BeTrue();
        }

        [Fact]
        public void StationAssignment_NullZones_HandledGracefully()
        {
            // Arrange & Act
            var assignment = new StationAssignment("Mission Row", null, 1);

            // Assert
            assignment.Zones.Should().NotBeNull();
            assignment.Zones.Should().BeEmpty();
        }

        [Fact]
        public void Station_ParsePosition_WithUppercaseF_ParsesCorrectly()
        {
            // Arrange
            var station = new Station { Position = "100.5F, 200.25F, 50.0F" };

            // Act
            station.ParsePosition();

            // Assert
            station.X.Should().BeApproximately(100.5f, 0.001f);
            station.Y.Should().BeApproximately(200.25f, 0.001f);
        }

        [Fact]
        public void Station_ParsePosition_OnlyOneCoordinate_SetsNone()
        {
            // Arrange
            var station = new Station { Position = "100.5f" };

            // Act
            station.ParsePosition();

            // Assert
            station.HasValidCoordinates.Should().BeFalse();
        }

        [Fact]
        public void StationAssignment_DisplayName_EmptyAgency_ShowsUnknown()
        {
            // Arrange
            var assignment = new StationAssignment { StationName = "Test Station" };
            assignment.StationReference = new Station("Test Station", "", "");

            // Act
            var displayName = assignment.DisplayName;

            // Assert
            displayName.Should().Be("[UNKNOWN] Test Station");
        }

        [Fact]
        public void Station_Equals_WithNonStationObject_ReturnsFalse()
        {
            // Arrange
            var station = new Station("Mission Row", "lspd", "lspd");

            // Act
            var result = station.Equals("Mission Row");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Station_Equals_WithNullObject_ReturnsFalse()
        {
            // Arrange
            var station = new Station("Mission Row", "lspd", "lspd");

            // Act
            var result = station.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Station_Equals_WithIntegerObject_ReturnsFalse()
        {
            // Arrange
            var station = new Station("Mission Row", "lspd", "lspd");

            // Act
            var result = station.Equals(123);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
