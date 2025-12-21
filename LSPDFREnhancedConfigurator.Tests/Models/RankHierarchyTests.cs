using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Models
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Models")]
    public class RankHierarchyTests
    {
        #region Constructor Tests

        [Fact]
        public void RankHierarchy_DefaultConstructor_InitializesCollections()
        {
            // Act
            var rank = new RankHierarchy();

            // Assert
            rank.PayBands.Should().NotBeNull();
            rank.Stations.Should().NotBeNull();
            rank.Vehicles.Should().NotBeNull();
            rank.Outfits.Should().NotBeNull();
            rank.Id.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RankHierarchy_ParameterizedConstructor_SetsProperties()
        {
            // Act
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Assert
            rank.Name.Should().Be("Officer");
            rank.RequiredPoints.Should().Be(1000);
            rank.Salary.Should().Be(5000);
            rank.PayBands.Should().NotBeNull();
            rank.Stations.Should().NotBeNull();
            rank.Vehicles.Should().NotBeNull();
            rank.Outfits.Should().NotBeNull();
        }

        #endregion

        #region GetRomanNumeral Tests

        [Theory]
        [InlineData(1, "I")]
        [InlineData(2, "II")]
        [InlineData(3, "III")]
        [InlineData(4, "IV")]
        [InlineData(5, "V")]
        [InlineData(6, "VI")]
        [InlineData(7, "VII")]
        [InlineData(8, "VIII")]
        [InlineData(9, "IX")]
        [InlineData(10, "X")]
        public void GetRomanNumeral_ValidNumber_ReturnsRomanNumeral(int number, string expected)
        {
            // Act
            var result = RankHierarchy.GetRomanNumeral(number);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetRomanNumeral_LessThanOne_ReturnsEmpty()
        {
            // Act & Assert
            RankHierarchy.GetRomanNumeral(0).Should().BeEmpty();
            RankHierarchy.GetRomanNumeral(-1).Should().BeEmpty();
            RankHierarchy.GetRomanNumeral(-100).Should().BeEmpty();
        }

        [Fact]
        public void GetRomanNumeral_GreaterThanTen_ReturnsNumber()
        {
            // Act & Assert
            RankHierarchy.GetRomanNumeral(11).Should().Be("11");
            RankHierarchy.GetRomanNumeral(20).Should().Be("20");
            RankHierarchy.GetRomanNumeral(100).Should().Be("100");
        }

        #endregion

        #region AddPayBand Tests

        [Fact]
        public void AddPayBand_FirstPayBand_CreatesPayBandI()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var payBand = rank.AddPayBand();

            // Assert
            payBand.Should().NotBeNull();
            payBand.Name.Should().Be("Officer I");
            payBand.RequiredPoints.Should().Be(1000);
            payBand.Salary.Should().Be(5000);
            payBand.Parent.Should().Be(rank);
            payBand.IsParent.Should().BeFalse();
            rank.PayBands.Should().Contain(payBand);
            rank.IsParent.Should().BeTrue();
        }

        [Fact]
        public void AddPayBand_SecondPayBand_CreatesPayBandII()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            rank.AddPayBand();

            // Act
            var payBand = rank.AddPayBand();

            // Assert
            payBand.Name.Should().Be("Officer II");
            rank.PayBands.Should().HaveCount(2);
        }

        [Fact]
        public void AddPayBand_MultiplePayBands_CreatesSequentialNumerals()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var pb1 = rank.AddPayBand();
            var pb2 = rank.AddPayBand();
            var pb3 = rank.AddPayBand();

            // Assert
            pb1.Name.Should().Be("Officer I");
            pb2.Name.Should().Be("Officer II");
            pb3.Name.Should().Be("Officer III");
            rank.PayBands.Should().HaveCount(3);
        }

        [Fact]
        public void AddPayBand_InitializesCollections()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var payBand = rank.AddPayBand();

            // Assert
            payBand.Stations.Should().NotBeNull();
            payBand.Vehicles.Should().NotBeNull();
            payBand.Outfits.Should().NotBeNull();
        }

        #endregion

        #region Clone Tests

        [Fact]
        public void Clone_BasicRank_ClonesCoreProperties()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000)
            {
                IsParent = true
            };

            // Act
            var clone = rank.Clone();

            // Assert
            clone.Should().NotBeSameAs(rank);
            clone.Name.Should().Be("Officer (Copy)");
            clone.RequiredPoints.Should().Be(1000);
            clone.Salary.Should().Be(5000);
            clone.IsParent.Should().BeTrue();
        }

        [Fact]
        public void Clone_WithVehicles_ClonesVehiclesList()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            rank.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));
            rank.Vehicles.Add(new Vehicle("police2", "Police SUV", "lspd"));

            // Act
            var clone = rank.Clone();

            // Assert
            clone.Vehicles.Should().HaveCount(2);
            clone.Vehicles.Should().NotBeSameAs(rank.Vehicles);
            clone.Vehicles[0].Model.Should().Be("police");
            clone.Vehicles[1].Model.Should().Be("police2");
        }

        [Fact]
        public void Clone_WithOutfits_ClonesOutfitsList()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            rank.Outfits.Add("outfit1");
            rank.Outfits.Add("outfit2");

            // Act
            var clone = rank.Clone();

            // Assert
            clone.Outfits.Should().HaveCount(2);
            clone.Outfits.Should().NotBeSameAs(rank.Outfits);
            clone.Outfits.Should().BeEquivalentTo(new[] { "outfit1", "outfit2" });
        }

        [Fact]
        public void Clone_WithStations_ClonesStationsDeep()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            var station = new StationAssignment("Mission Row", new List<string> { "zone1" }, 0);
            station.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));
            station.Outfits.Add("outfit1");
            rank.Stations.Add(station);

            // Act
            var clone = rank.Clone();

            // Assert
            clone.Stations.Should().HaveCount(1);
            clone.Stations.Should().NotBeSameAs(rank.Stations);
            clone.Stations[0].Should().NotBeSameAs(station);
            clone.Stations[0].StationName.Should().Be("Mission Row");
            clone.Stations[0].Vehicles.Should().HaveCount(1);
            clone.Stations[0].Outfits.Should().HaveCount(1);
        }

        [Fact]
        public void Clone_WithPayBands_ClonesPayBandsRecursively()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            var payBand = rank.AddPayBand();
            payBand.Vehicles.Add(new Vehicle("police", "Police Cruiser", "lspd"));

            // Act
            var clone = rank.Clone();

            // Assert
            clone.PayBands.Should().HaveCount(1);
            clone.PayBands.Should().NotBeSameAs(rank.PayBands);
            clone.PayBands[0].Should().NotBeSameAs(payBand);
            clone.PayBands[0].Name.Should().Be("Officer I (Copy)");
            clone.PayBands[0].Parent.Should().Be(clone);
            clone.PayBands[0].Vehicles.Should().HaveCount(1);
        }

        [Fact]
        public void Clone_EmptyRank_WorksCorrectly()
        {
            // Arrange
            var rank = new RankHierarchy();

            // Act
            var clone = rank.Clone();

            // Assert
            clone.Should().NotBeSameAs(rank);
            clone.PayBands.Should().BeEmpty();
            clone.Stations.Should().BeEmpty();
            clone.Vehicles.Should().BeEmpty();
            clone.Outfits.Should().BeEmpty();
        }

        #endregion

        #region PromoteToParent Tests

        [Fact]
        public void PromoteToParent_WithParent_ClearsParent()
        {
            // Arrange
            var parent = new RankHierarchy("Officer", 1000, 5000);
            var payBand = parent.AddPayBand();

            // Act
            payBand.PromoteToParent();

            // Assert
            payBand.Parent.Should().BeNull();
        }

        [Fact]
        public void PromoteToParent_ClearsIsParentFlag()
        {
            // Arrange
            var parent = new RankHierarchy("Officer", 1000, 5000);
            var payBand = parent.AddPayBand();
            payBand.IsParent = true;

            // Act
            payBand.PromoteToParent();

            // Assert
            payBand.IsParent.Should().BeFalse();
        }

        [Fact]
        public void PromoteToParent_ClearsPayBands()
        {
            // Arrange
            var parent = new RankHierarchy("Officer", 1000, 5000);
            var payBand = parent.AddPayBand();
            payBand.PayBands.Add(new RankHierarchy("Nested", 100, 200));

            // Act
            payBand.PromoteToParent();

            // Assert
            payBand.PayBands.Should().BeEmpty();
        }

        [Fact]
        public void PromoteToParent_WithoutParent_DoesNotThrow()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var act = () => rank.PromoteToParent();

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region GetSummary Tests

        [Fact]
        public void GetSummary_SimpleRank_NoNextRank_ShowsXPPlus()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var summary = rank.GetSummary();

            // Assert
            summary.Should().Contain("Officer");
            summary.Should().Contain("1000+");
            summary.Should().Contain("$5,000");
        }

        [Fact]
        public void GetSummary_SimpleRank_WithNextRank_ShowsXPRange()
        {
            // Arrange
            var officer = new RankHierarchy("Officer", 1000, 5000);
            var sergeant = new RankHierarchy("Sergeant", 2000, 7000);

            // Act
            var summary = officer.GetSummary(sergeant);

            // Assert
            summary.Should().Contain("Officer");
            summary.Should().Contain("1000-1999");
            summary.Should().Contain("$5,000");
        }

        [Fact]
        public void GetSummary_ParentRank_ShowsPayBandRange()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);
            var pb1 = rank.AddPayBand();
            pb1.RequiredPoints = 1000;
            pb1.Salary = 5000;
            var pb2 = rank.AddPayBand();
            pb2.RequiredPoints = 1500;
            pb2.Salary = 6000;

            // Act
            var summary = rank.GetSummary();

            // Assert
            summary.Should().Contain("Officer");
            summary.Should().Contain("1000-1500");
            summary.Should().Contain("$5,000-$6,000");
        }

        [Fact]
        public void GetSummary_ParentRank_WithNextRank_ShowsRangePlus()
        {
            // Arrange
            var officer = new RankHierarchy("Officer", 1000, 5000);
            var pb1 = officer.AddPayBand();
            pb1.RequiredPoints = 1000;
            pb1.Salary = 5000;
            var pb2 = officer.AddPayBand();
            pb2.RequiredPoints = 1500;
            pb2.Salary = 6000;

            var sergeant = new RankHierarchy("Sergeant", 2000, 7000);

            // Act
            var summary = officer.GetSummary(sergeant);

            // Assert
            summary.Should().Contain("1000-1500+");
        }

        [Fact]
        public void GetSummary_ParentRankWithNoPayBands_ShowsSimpleFormat()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000)
            {
                IsParent = true
                // No pay bands added
            };

            // Act
            var summary = rank.GetSummary();

            // Assert
            summary.Should().Contain("Officer");
            summary.Should().Contain("1000+");
            summary.Should().Contain("$5,000");
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ReturnsName()
        {
            // Arrange
            var rank = new RankHierarchy("Officer", 1000, 5000);

            // Act
            var result = rank.ToString();

            // Assert
            result.Should().Be("Officer");
        }

        [Fact]
        public void ToString_EmptyName_ReturnsEmptyString()
        {
            // Arrange
            var rank = new RankHierarchy();

            // Act
            var result = rank.ToString();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Id_AutoGenerated_IsUnique()
        {
            // Arrange & Act
            var rank1 = new RankHierarchy();
            var rank2 = new RankHierarchy();

            // Assert
            rank1.Id.Should().NotBe(rank2.Id);
        }

        [Fact]
        public void Properties_CanBeSetAndGet()
        {
            // Arrange
            var rank = new RankHierarchy();

            // Act
            rank.Name = "Test Rank";
            rank.RequiredPoints = 999;
            rank.Salary = 12345;
            rank.IsParent = true;

            // Assert
            rank.Name.Should().Be("Test Rank");
            rank.RequiredPoints.Should().Be(999);
            rank.Salary.Should().Be(12345);
            rank.IsParent.Should().BeTrue();
        }

        #endregion
    }
}
