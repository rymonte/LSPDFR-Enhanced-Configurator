using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.Services.Validation.Rules;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services.Validation
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Validation")]
    public class AdvisoryRuleTests
    {
        private readonly AdvisoryRule _rule;
        private readonly Mock<DataLoadingService> _mockDataService;

        public AdvisoryRuleTests()
        {
            _rule = new AdvisoryRule();
            _mockDataService = new MockServiceBuilder().BuildMock();
        }

        #region Empty Ranks Validation

        [Fact]
        public void Validate_EmptyRanksList_DoesNotThrow()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void Validate_SingleRank_OnlyChecksForMissingResources()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Should only check for missing vehicles and outfits, no comparison issues
            result.Issues.Should().AllSatisfy(i =>
            {
                i.Severity.Should().Be(ValidationSeverity.Advisory);
                i.Message.Should().Match(m =>
                    m.Contains("no vehicles assigned") ||
                    m.Contains("no outfits assigned"));
            });
        }

        #endregion

        #region Station Count Validation

        [Fact]
        public void Validate_DecreasingStationCount_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank1.Stations.Add(new StationAssignment { StationName = "Station2" });
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Station" &&
                i.Message.Contains("station(s)") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_IncreasingStationCount_PassesValidation()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank2.Stations.Add(new StationAssignment { StationName = "Station2" });
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i => i.Category == "Station" && i.Message.Contains("reduction"));
        }

        #endregion

        #region Vehicle Validation

        [Fact]
        public void Validate_RemovedVehicles_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            rank1.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // Missing SUV

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("missing") &&
                i.Message.Contains("vehicle(s)") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_DecreasingVehicleCountWithoutRemoval_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // Same model twice

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // One instance only

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("has") &&
                i.Message.Contains("vehicle(s)") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_RankWithNoVehicles_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            // No vehicles at all

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("no vehicles assigned") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_FirstRankWithNoVehicles_ReturnsAdvisory()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            // No vehicles

            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("no vehicles assigned") &&
                i.RankName == "Officer");
        }

        [Fact]
        public void Validate_RankWithStationSpecificVehicles_PassesValidation()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            var station = new StationAssignment { StationName = "Station1" };
            station.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            rank.Stations.Add(station);

            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i =>
                i.Category == "Vehicle" &&
                i.Message.Contains("no vehicles assigned"));
        }

        #endregion

        #region Outfit Validation

        [Fact]
        public void Validate_RemovedOutfits_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Outfits.Add("Uniform1");
            rank1.Outfits.Add("Uniform2");
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Outfits.Add("Uniform1"); // Missing Uniform2
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("missing") &&
                i.Message.Contains("outfit(s)") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_RankWithNoOutfits_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Outfits.Add("Uniform1");
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            // No outfits at all

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("no outfits assigned") &&
                i.RankName == "Detective");
        }

        [Fact]
        public void Validate_FirstRankWithNoOutfits_ReturnsAdvisory()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            rank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            // No outfits

            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("no outfits assigned") &&
                i.RankName == "Officer");
        }

        [Fact]
        public void Validate_RankWithStationSpecificOutfits_PassesValidation()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            rank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            var station = new StationAssignment { StationName = "Station1" };
            station.Outfits.Add("Uniform1");
            rank.Stations.Add(station);

            var ranks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().NotContain(i =>
                i.Category == "Outfit" &&
                i.Message.Contains("no outfits assigned"));
        }

        #endregion

        #region ValidateSingleRank Tests

        [Fact]
        public void ValidateSingleRank_ParentRankWithPayBands_SkipsValidation()
        {
            // Arrange
            var parent = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            parent.IsParent = true;

            var payBand1 = new RankHierarchyBuilder()
                .WithName("Detective I")
                .Build();
            payBand1.RequiredPoints = 0;
            payBand1.Salary = 3000;
            payBand1.Parent = parent;

            var payBand2 = new RankHierarchyBuilder()
                .WithName("Detective II")
                .Build();
            payBand2.RequiredPoints = 100;
            payBand2.Salary = 4000;
            payBand2.Parent = parent;

            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);

            var allRanks = new List<RankHierarchy> { parent };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(parent, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert - Should skip advisory checks for parent ranks
            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void ValidateSingleRank_RankNotInList_SkipsValidation()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;

            var allRanks = new List<RankHierarchy> { rank1 }; // rank2 not in list
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void ValidateSingleRank_NoVehicles_ReturnsAdvisory()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;

            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("no vehicles assigned"));
        }

        [Fact]
        public void ValidateSingleRank_NoOutfits_ReturnsAdvisory()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 0;
            rank.Salary = 1000;
            rank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var allRanks = new List<RankHierarchy> { rank };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("no outfits assigned"));
        }

        [Fact]
        public void ValidateSingleRank_DecreasingStationCount_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank1.Stations.Add(new StationAssignment { StationName = "Station2" });
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Stations.Add(new StationAssignment { StationName = "Station1" });
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Station" &&
                i.Message.Contains("station(s)"));
        }

        [Fact]
        public void ValidateSingleRank_RemovedVehicles_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            rank1.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // Missing SUV

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("missing") &&
                i.Message.Contains("vehicle(s)"));
        }

        [Fact]
        public void ValidateSingleRank_RemovedOutfits_ReturnsAdvisory()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Outfits.Add("Uniform1");
            rank1.Outfits.Add("Uniform2");
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Outfits.Add("Uniform1"); // Missing Uniform2
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var allRanks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.ValidateSingleRank(rank2, allRanks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("missing") &&
                i.Message.Contains("outfit(s)"));
        }

        #endregion

        #region Large Removed Lists

        [Fact]
        public void Validate_MoreThanThreeRemovedVehicles_TruncatesDisplay()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Vehicles.Add(new Vehicle { Model = "vehicle1" });
            rank1.Vehicles.Add(new Vehicle { Model = "vehicle2" });
            rank1.Vehicles.Add(new Vehicle { Model = "vehicle3" });
            rank1.Vehicles.Add(new Vehicle { Model = "vehicle4" });
            rank1.Vehicles.Add(new Vehicle { Model = "vehicle5" });

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            // No vehicles - all removed

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Vehicle" &&
                i.Message.Contains("missing 5 vehicle(s)") &&
                i.Message.Contains("and 2 more"));
        }

        [Fact]
        public void Validate_MoreThanThreeRemovedOutfits_TruncatesDisplay()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank1.RequiredPoints = 0;
            rank1.Salary = 1000;
            rank1.Outfits.Add("outfit1");
            rank1.Outfits.Add("outfit2");
            rank1.Outfits.Add("outfit3");
            rank1.Outfits.Add("outfit4");
            rank1.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var rank2 = new RankHierarchyBuilder()
                .WithName("Detective")
                .Build();
            rank2.RequiredPoints = 100;
            rank2.Salary = 2000;
            rank2.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            // No outfits - all removed

            var ranks = new List<RankHierarchy> { rank1, rank2 };
            var result = new ValidationResult();

            // Act
            _rule.Validate(ranks, result, ValidationContext.Full, _mockDataService.Object);

            // Assert
            result.Issues.Should().Contain(i =>
                i.Severity == ValidationSeverity.Advisory &&
                i.Category == "Outfit" &&
                i.Message.Contains("missing 4 outfit(s)") &&
                i.Message.Contains("and 1 more"));
        }

        #endregion
    }
}
