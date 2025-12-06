using System.Collections.Generic;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a vehicle available in LSPDFR
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Vehicle model identifier (e.g., "police", "police2")
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Human-readable display name (e.g., "2011 Ford Crown Vic")
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// List of agencies this vehicle belongs to (e.g., ["lspd", "lssd"])
        /// </summary>
        public List<string> Agencies { get; set; }

        /// <summary>
        /// Category of vehicle (sedan, SUV, bike, etc.) - inferred from loadout name
        /// </summary>
        public string Category { get; set; }

        public Vehicle()
        {
            Model = string.Empty;
            DisplayName = string.Empty;
            Agencies = new List<string>();
            Category = string.Empty;
        }

        public Vehicle(string model, string displayName, List<string> agencies, string category = "")
        {
            Model = model;
            DisplayName = displayName;
            Agencies = agencies ?? new List<string>();
            Category = category;
        }

        /// <summary>
        /// Helper constructor for single-agency vehicles
        /// </summary>
        public Vehicle(string model, string displayName, string agency, string category = "")
        {
            Model = model;
            DisplayName = displayName;
            Agencies = new List<string> { agency };
            Category = category;
        }

        /// <summary>
        /// Check if this vehicle belongs to a specific agency
        /// </summary>
        public bool BelongsToAgency(string agency)
        {
            return Agencies.Any(a => a.Equals(agency, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get primary agency (first in list)
        /// </summary>
        public string PrimaryAgency
        {
            get { return Agencies.Count > 0 ? Agencies[0] : "Unknown"; }
        }

        /// <summary>
        /// Check if vehicle is shared across multiple agencies
        /// </summary>
        public bool IsShared
        {
            get { return Agencies.Count > 1; }
        }

        public override string ToString()
        {
            string agencyInfo = Agencies.Count > 0 ? string.Join(", ", Agencies.Select(a => a.ToUpper())) : "Unknown";
            if (string.IsNullOrEmpty(DisplayName))
            {
                return $"{Model} [{agencyInfo}]";
            }
            return $"{DisplayName} ({Model}) [{agencyInfo}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vehicle other)
            {
                return Model.Equals(other.Model, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Model.ToLowerInvariant().GetHashCode();
        }
    }
}
