using System.Collections.Generic;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a law enforcement agency
    /// </summary>
    public class Agency
    {
        /// <summary>
        /// Full agency name (e.g., "Los Santos Police Department")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short name (e.g., "LSPD")
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Script name identifier (e.g., "lspd")
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// List of vehicles associated with this agency
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        public Agency()
        {
            Name = string.Empty;
            ShortName = string.Empty;
            ScriptName = string.Empty;
            Vehicles = new List<Vehicle>();
        }

        public Agency(string name, string shortName, string scriptName)
        {
            Name = name;
            ShortName = shortName;
            ScriptName = scriptName;
            Vehicles = new List<Vehicle>();
        }

        public override string ToString()
        {
            return $"{ShortName} - {Name}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Agency other)
            {
                return ScriptName.Equals(other.ScriptName, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ScriptName.ToLowerInvariant().GetHashCode();
        }
    }
}
