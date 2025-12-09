using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using Moq;

namespace LSPDFREnhancedConfigurator.Tests.Builders
{
    /// <summary>
    /// Fluent builder for creating pre-configured Mock DataLoadingService instances
    /// </summary>
    public class MockServiceBuilder
    {
        private List<Agency> _agencies = new();
        private List<Vehicle> _vehicles = new();
        private List<Station> _stations = new();
        private List<OutfitVariation> _outfitVariations = new();
        private List<RankHierarchy> _ranks = new();

        /// <summary>
        /// Add a custom agency
        /// </summary>
        public MockServiceBuilder WithAgency(Agency agency)
        {
            _agencies.Add(agency);
            return this;
        }

        /// <summary>
        /// Add default agencies (LSPD, LSSD, SAHP, BCSO)
        /// </summary>
        public MockServiceBuilder WithDefaultAgencies()
        {
            _agencies.AddRange(new[]
            {
                new Agency("Los Santos Police Department", "LSPD", "lspd"),
                new Agency("Los Santos Sheriff's Department", "LSSD", "lssd"),
                new Agency("San Andreas Highway Patrol", "SAHP", "sahp"),
                new Agency("Blaine County Sheriff's Office", "BCSO", "bcso")
            });
            return this;
        }

        /// <summary>
        /// Add a custom vehicle
        /// </summary>
        public MockServiceBuilder WithVehicle(Vehicle vehicle)
        {
            _vehicles.Add(vehicle);
            return this;
        }

        /// <summary>
        /// Add multiple vehicles
        /// </summary>
        public MockServiceBuilder WithVehicles(params Vehicle[] vehicles)
        {
            _vehicles.AddRange(vehicles);
            return this;
        }

        /// <summary>
        /// Add default LSPD vehicles
        /// </summary>
        public MockServiceBuilder WithDefaultLSPDVehicles()
        {
            _vehicles.AddRange(new[]
            {
                VehicleBuilder.CreateLSPDPatrol(),
                VehicleBuilder.CreateSUV("lspd"),
                VehicleBuilder.CreateMotorcycle("lspd")
            });
            return this;
        }

        /// <summary>
        /// Add default vehicles for all agencies
        /// </summary>
        public MockServiceBuilder WithDefaultVehicles()
        {
            _vehicles.AddRange(new[]
            {
                VehicleBuilder.CreateLSPDPatrol(),
                VehicleBuilder.CreateSUV("lspd"),
                VehicleBuilder.CreateMotorcycle("lspd"),
                VehicleBuilder.CreateLSSDPatrol(),
                VehicleBuilder.CreateSUV("lssd")
            });
            return this;
        }

        /// <summary>
        /// Add a custom station
        /// </summary>
        public MockServiceBuilder WithStation(Station station)
        {
            _stations.Add(station);
            return this;
        }

        /// <summary>
        /// Add multiple stations
        /// </summary>
        public MockServiceBuilder WithStations(params Station[] stations)
        {
            _stations.AddRange(stations);
            return this;
        }

        /// <summary>
        /// Add default stations (Mission Row, Vespucci, Sandy Shores, etc.)
        /// </summary>
        public MockServiceBuilder WithDefaultStations()
        {
            _stations.AddRange(new[]
            {
                new Station("Mission Row Police Station", "lspd", "MissionRow"),
                new Station("Vespucci Police Station", "lspd", "Vespucci"),
                new Station("Davis Police Station", "lspd", "Davis"),
                new Station("Sandy Shores Sheriff's Office", "lssd", "SandyShores"),
                new Station("Paleto Bay Sheriff's Office", "lssd", "PaletoBay")
            });
            return this;
        }

        /// <summary>
        /// Add a custom outfit variation
        /// </summary>
        public MockServiceBuilder WithOutfitVariation(OutfitVariation variation)
        {
            _outfitVariations.Add(variation);
            return this;
        }

        /// <summary>
        /// Add default LSPD outfit variations
        /// </summary>
        public MockServiceBuilder WithDefaultLSPDOutfits()
        {
            var outfit = OutfitVariationBuilder.CreateLSPDClassAOutfit();
            _outfitVariations.AddRange(outfit.Variations);
            return this;
        }

        /// <summary>
        /// Add default outfit variations for multiple agencies
        /// </summary>
        public MockServiceBuilder WithDefaultOutfits()
        {
            var lspdOutfit = OutfitVariationBuilder.CreateLSPDClassAOutfit();
            var lssdOutfit = OutfitVariationBuilder.CreateLSSDOutfit();

            _outfitVariations.AddRange(lspdOutfit.Variations);
            _outfitVariations.AddRange(lssdOutfit.Variations);
            return this;
        }

        /// <summary>
        /// Add a custom rank
        /// </summary>
        public MockServiceBuilder WithRank(RankHierarchy rank)
        {
            _ranks.Add(rank);
            return this;
        }

        /// <summary>
        /// Add multiple ranks
        /// </summary>
        public MockServiceBuilder WithRanks(params RankHierarchy[] ranks)
        {
            _ranks.AddRange(ranks);
            return this;
        }

        /// <summary>
        /// Add default rank hierarchy (Officer, Sergeant, Detective, Lieutenant)
        /// </summary>
        public MockServiceBuilder WithDefaultRanks()
        {
            _ranks.AddRange(new[]
            {
                new RankHierarchyBuilder()
                    .WithName("Officer")
                    .WithXP(0)
                    .WithSalary(1000)
                    .Build(),
                new RankHierarchyBuilder()
                    .WithName("Sergeant")
                    .WithXP(500)
                    .WithSalary(3000)
                    .Build(),
                new RankHierarchyBuilder()
                    .WithName("Detective")
                    .WithXP(1000)
                    .WithSalary(5000)
                    .WithPayBands(3)
                    .Build(),
                new RankHierarchyBuilder()
                    .WithName("Lieutenant")
                    .WithXP(2000)
                    .WithSalary(7000)
                    .Build()
            });
            return this;
        }

        /// <summary>
        /// Build a Mock of DataLoadingService with all configured data
        /// </summary>
        public Mock<DataLoadingService> BuildMock()
        {
            var mock = new Mock<DataLoadingService>(null);

            mock.Setup(x => x.Agencies).Returns(_agencies);
            mock.Setup(x => x.AllVehicles).Returns(_vehicles);
            mock.Setup(x => x.Stations).Returns(_stations);
            mock.Setup(x => x.OutfitVariations).Returns(_outfitVariations);
            mock.Setup(x => x.Ranks).Returns(_ranks);

            return mock;
        }

        /// <summary>
        /// Build the actual DataLoadingService instance (not a mock) with configured data
        /// Useful for integration tests that need a real service instance
        /// </summary>
        public DataLoadingService BuildReal()
        {
            var service = new DataLoadingService(null);

            // Use reflection to set the protected properties
            var agenciesProperty = typeof(DataLoadingService).GetProperty("Agencies");
            var vehiclesProperty = typeof(DataLoadingService).GetProperty("AllVehicles");
            var stationsProperty = typeof(DataLoadingService).GetProperty("Stations");
            var outfitsProperty = typeof(DataLoadingService).GetProperty("OutfitVariations");
            var ranksProperty = typeof(DataLoadingService).GetProperty("Ranks");

            agenciesProperty?.SetValue(service, _agencies);
            vehiclesProperty?.SetValue(service, _vehicles);
            stationsProperty?.SetValue(service, _stations);
            outfitsProperty?.SetValue(service, _outfitVariations);
            ranksProperty?.SetValue(service, _ranks);

            return service;
        }

        /// <summary>
        /// Create a fully populated mock service with all default data
        /// </summary>
        public static Mock<DataLoadingService> CreateFullyPopulated()
        {
            return new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultVehicles()
                .WithDefaultStations()
                .WithDefaultOutfits()
                .WithDefaultRanks()
                .BuildMock();
        }

        /// <summary>
        /// Create a minimal mock service with just agencies
        /// </summary>
        public static Mock<DataLoadingService> CreateMinimal()
        {
            return new MockServiceBuilder()
                .WithDefaultAgencies()
                .BuildMock();
        }
    }
}
