using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Tests.Builders
{
    /// <summary>
    /// Fluent builder for creating RankHierarchy test data
    /// </summary>
    public class RankHierarchyBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Officer";
        private int _requiredPoints = 0;
        private int _salary = 1000;
        private bool _isParent = false;
        private RankHierarchy _parent = null;
        private List<StationAssignment> _stations = new();
        private List<Vehicle> _vehicles = new();
        private List<string> _outfits = new();
        private List<RankHierarchy> _payBands = new();

        /// <summary>
        /// Set the rank ID
        /// </summary>
        public RankHierarchyBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Set the rank name
        /// </summary>
        public RankHierarchyBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Set the required XP points
        /// </summary>
        public RankHierarchyBuilder WithXP(int xp)
        {
            _requiredPoints = xp;
            return this;
        }

        /// <summary>
        /// Set the salary
        /// </summary>
        public RankHierarchyBuilder WithSalary(int salary)
        {
            _salary = salary;
            return this;
        }

        /// <summary>
        /// Mark this rank as a parent rank (has pay bands)
        /// </summary>
        public RankHierarchyBuilder AsParent()
        {
            _isParent = true;
            return this;
        }

        /// <summary>
        /// Set the parent rank (for pay bands)
        /// </summary>
        public RankHierarchyBuilder WithParent(RankHierarchy parent)
        {
            _parent = parent;
            return this;
        }

        /// <summary>
        /// Add a station assignment to this rank
        /// </summary>
        public RankHierarchyBuilder WithStation(StationAssignment station)
        {
            _stations.Add(station);
            return this;
        }

        /// <summary>
        /// Add multiple station assignments to this rank
        /// </summary>
        public RankHierarchyBuilder WithStations(params StationAssignment[] stations)
        {
            _stations.AddRange(stations);
            return this;
        }

        /// <summary>
        /// Add a vehicle to this rank
        /// </summary>
        public RankHierarchyBuilder WithVehicle(Vehicle vehicle)
        {
            _vehicles.Add(vehicle);
            return this;
        }

        /// <summary>
        /// Add multiple vehicles to this rank
        /// </summary>
        public RankHierarchyBuilder WithVehicles(params Vehicle[] vehicles)
        {
            _vehicles.AddRange(vehicles);
            return this;
        }

        /// <summary>
        /// Add an outfit (combined name) to this rank
        /// </summary>
        public RankHierarchyBuilder WithOutfit(string outfit)
        {
            _outfits.Add(outfit);
            return this;
        }

        /// <summary>
        /// Add multiple outfits to this rank
        /// </summary>
        public RankHierarchyBuilder WithOutfits(params string[] outfits)
        {
            _outfits.AddRange(outfits);
            return this;
        }

        /// <summary>
        /// Create pay bands for this rank with auto-generated names and incremental XP/salary
        /// </summary>
        /// <param name="count">Number of pay bands to create</param>
        /// <param name="xpIncrement">XP increment per pay band (default: 100)</param>
        /// <param name="salaryIncrement">Salary increment per pay band (default: 500)</param>
        public RankHierarchyBuilder WithPayBands(int count, int xpIncrement = 100, int salaryIncrement = 500)
        {
            _isParent = true;
            _payBands.Clear();

            for (int i = 0; i < count; i++)
            {
                var payBandNumber = i + 1;
                var payBandName = $"{_name} {RankHierarchy.GetRomanNumeral(payBandNumber)}";
                var payBandXP = _requiredPoints + (payBandNumber * xpIncrement);
                var payBandSalary = _salary + (payBandNumber * salaryIncrement);

                var payBand = new RankHierarchy(payBandName, payBandXP, payBandSalary)
                {
                    Id = $"{_id}_pb{i}",
                    IsParent = false
                };

                _payBands.Add(payBand);
            }

            return this;
        }

        /// <summary>
        /// Add a custom pay band
        /// </summary>
        public RankHierarchyBuilder WithPayBand(RankHierarchy payBand)
        {
            _isParent = true;
            _payBands.Add(payBand);
            return this;
        }

        /// <summary>
        /// Build the RankHierarchy instance
        /// </summary>
        public RankHierarchy Build()
        {
            var rank = new RankHierarchy(_name, _requiredPoints, _salary)
            {
                Id = _id,
                IsParent = _isParent,
                Parent = _parent
            };

            // Add stations
            foreach (var station in _stations)
            {
                rank.Stations.Add(station);
            }

            // Add vehicles
            foreach (var vehicle in _vehicles)
            {
                rank.Vehicles.Add(vehicle);
            }

            // Add outfits
            foreach (var outfit in _outfits)
            {
                rank.Outfits.Add(outfit);
            }

            // Add pay bands and set parent references
            foreach (var payBand in _payBands)
            {
                payBand.Parent = rank;
                rank.PayBands.Add(payBand);
            }

            return rank;
        }

        /// <summary>
        /// Create a default rank with preset values for quick testing
        /// </summary>
        public static RankHierarchy CreateDefault()
        {
            return new RankHierarchyBuilder()
                .WithName("Officer")
                .WithXP(0)
                .WithSalary(1000)
                .Build();
        }

        /// <summary>
        /// Create a rank with pay bands for testing hierarchies
        /// </summary>
        public static RankHierarchy CreateWithPayBands(string name, int payBandCount = 3)
        {
            return new RankHierarchyBuilder()
                .WithName(name)
                .WithXP(500)
                .WithSalary(3000)
                .WithPayBands(payBandCount)
                .Build();
        }
    }
}
