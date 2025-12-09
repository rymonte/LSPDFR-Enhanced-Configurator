using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Tests.Builders
{
    /// <summary>
    /// Fluent builder for creating StationAssignment test data
    /// </summary>
    public class StationAssignmentBuilder
    {
        private string _stationName = "Mission Row Police Station";
        private List<string> _zones = new();
        private int _styleId = 0;
        private List<Vehicle> _vehicles = new();
        private List<string> _outfits = new();
        private Station _stationReference = null;

        /// <summary>
        /// Set the station name
        /// </summary>
        public StationAssignmentBuilder WithName(string stationName)
        {
            _stationName = stationName;
            return this;
        }

        /// <summary>
        /// Add a zone to this station
        /// </summary>
        public StationAssignmentBuilder WithZone(string zone)
        {
            _zones.Add(zone);
            return this;
        }

        /// <summary>
        /// Add multiple zones to this station
        /// </summary>
        public StationAssignmentBuilder WithZones(params string[] zones)
        {
            _zones.AddRange(zones);
            return this;
        }

        /// <summary>
        /// Set the style ID
        /// </summary>
        public StationAssignmentBuilder WithStyleId(int styleId)
        {
            _styleId = styleId;
            return this;
        }

        /// <summary>
        /// Add a vehicle to this station
        /// </summary>
        public StationAssignmentBuilder WithVehicle(Vehicle vehicle)
        {
            _vehicles.Add(vehicle);
            return this;
        }

        /// <summary>
        /// Add multiple vehicles to this station
        /// </summary>
        public StationAssignmentBuilder WithVehicles(params Vehicle[] vehicles)
        {
            _vehicles.AddRange(vehicles);
            return this;
        }

        /// <summary>
        /// Add an outfit to this station
        /// </summary>
        public StationAssignmentBuilder WithOutfit(string outfit)
        {
            _outfits.Add(outfit);
            return this;
        }

        /// <summary>
        /// Add multiple outfits to this station
        /// </summary>
        public StationAssignmentBuilder WithOutfits(params string[] outfits)
        {
            _outfits.AddRange(outfits);
            return this;
        }

        /// <summary>
        /// Set the station reference (links to actual station definition)
        /// </summary>
        public StationAssignmentBuilder WithStationReference(Station station)
        {
            _stationReference = station;
            return this;
        }

        /// <summary>
        /// Build the StationAssignment instance
        /// </summary>
        public StationAssignment Build()
        {
            var station = new StationAssignment(_stationName, new List<string>(_zones), _styleId)
            {
                StationReference = _stationReference
            };

            // Add vehicles
            foreach (var vehicle in _vehicles)
            {
                station.Vehicles.Add(vehicle);
            }

            // Add outfits
            foreach (var outfit in _outfits)
            {
                station.Outfits.Add(outfit);
            }

            return station;
        }

        /// <summary>
        /// Create a default station assignment for quick testing
        /// </summary>
        public static StationAssignment CreateDefault()
        {
            return new StationAssignmentBuilder()
                .WithName("Mission Row Police Station")
                .WithZones("Downtown", "Little Seoul")
                .WithStyleId(0)
                .Build();
        }

        /// <summary>
        /// Create a station with a station reference for integration testing
        /// </summary>
        public static StationAssignment CreateWithReference(string name, string agency)
        {
            var stationRef = new Station(name, agency, name.Replace(" ", ""));
            return new StationAssignmentBuilder()
                .WithName(name)
                .WithStationReference(stationRef)
                .WithZones("Zone 1")
                .Build();
        }
    }
}
