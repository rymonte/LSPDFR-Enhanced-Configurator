using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Tests.Builders
{
    /// <summary>
    /// Fluent builder for creating Vehicle test data
    /// </summary>
    public class VehicleBuilder
    {
        private string _model = "police";
        private string _displayName = "Police Cruiser";
        private List<string> _agencies = new();
        private string _category = "Sedan";

        /// <summary>
        /// Set the vehicle model
        /// </summary>
        public VehicleBuilder WithModel(string model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Set the display name
        /// </summary>
        public VehicleBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        /// <summary>
        /// Add an agency to this vehicle
        /// </summary>
        public VehicleBuilder WithAgency(string agency)
        {
            _agencies.Add(agency);
            return this;
        }

        /// <summary>
        /// Add multiple agencies to this vehicle
        /// </summary>
        public VehicleBuilder WithAgencies(params string[] agencies)
        {
            _agencies.AddRange(agencies);
            return this;
        }

        /// <summary>
        /// Set the vehicle category
        /// </summary>
        public VehicleBuilder WithCategory(string category)
        {
            _category = category;
            return this;
        }

        /// <summary>
        /// Build the Vehicle instance
        /// </summary>
        public Vehicle Build()
        {
            return new Vehicle(_model, _displayName, new List<string>(_agencies), _category);
        }

        /// <summary>
        /// Create a default LSPD vehicle for quick testing
        /// </summary>
        public static Vehicle CreateDefault()
        {
            return new VehicleBuilder()
                .WithModel("police")
                .WithDisplayName("2011 Ford Crown Victoria")
                .WithAgency("lspd")
                .WithCategory("Sedan")
                .Build();
        }

        /// <summary>
        /// Create an LSPD patrol vehicle
        /// </summary>
        public static Vehicle CreateLSPDPatrol()
        {
            return new VehicleBuilder()
                .WithModel("police")
                .WithDisplayName("2011 Ford Crown Victoria")
                .WithAgency("lspd")
                .WithCategory("Sedan")
                .Build();
        }

        /// <summary>
        /// Create an LSSD patrol vehicle
        /// </summary>
        public static Vehicle CreateLSSDPatrol()
        {
            return new VehicleBuilder()
                .WithModel("sheriff")
                .WithDisplayName("2013 Dodge Charger")
                .WithAgency("lssd")
                .WithCategory("Sedan")
                .Build();
        }

        /// <summary>
        /// Create a shared vehicle (multiple agencies)
        /// </summary>
        public static Vehicle CreateSharedVehicle()
        {
            return new VehicleBuilder()
                .WithModel("police2")
                .WithDisplayName("2012 Ford Interceptor")
                .WithAgencies("lspd", "lssd")
                .WithCategory("Sedan")
                .Build();
        }

        /// <summary>
        /// Create an SUV vehicle
        /// </summary>
        public static Vehicle CreateSUV(string agency = "lspd")
        {
            return new VehicleBuilder()
                .WithModel("police3")
                .WithDisplayName("Ford Explorer")
                .WithAgency(agency)
                .WithCategory("SUV")
                .Build();
        }

        /// <summary>
        /// Create a motorcycle
        /// </summary>
        public static Vehicle CreateMotorcycle(string agency = "lspd")
        {
            return new VehicleBuilder()
                .WithModel("policeb")
                .WithDisplayName("Police Motorcycle")
                .WithAgency(agency)
                .WithCategory("Motorcycle")
                .Build();
        }
    }
}
