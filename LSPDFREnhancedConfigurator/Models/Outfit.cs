using System.Collections.Generic;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents an outfit with its variations
    /// </summary>
    public class Outfit
    {
        /// <summary>
        /// Outfit name (e.g., "LSPD Class A")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Script name for the outfit
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// List of variations for this outfit
        /// </summary>
        public List<OutfitVariation> Variations { get; set; }

        /// <summary>
        /// Inferred agency based on outfit name
        /// </summary>
        public string InferredAgency { get; set; }

        public Outfit()
        {
            Name = string.Empty;
            ScriptName = string.Empty;
            Variations = new List<OutfitVariation>();
            InferredAgency = string.Empty;
        }

        public Outfit(string name, string scriptName)
        {
            Name = name;
            ScriptName = scriptName;
            Variations = new List<OutfitVariation>();
            InferredAgency = InferAgency(name);
        }

        /// <summary>
        /// Infers the agency from the outfit name
        /// </summary>
        private string InferAgency(string outfitName)
        {
            string upper = outfitName.ToUpperInvariant();
            if (upper.Contains("LSPD")) return "LSPD";
            if (upper.Contains("LSSD") || upper.Contains("SHERIFF")) return "LSSD";
            if (upper.Contains("BCSO")) return "BCSO";
            if (upper.Contains("SAHP") || upper.Contains("HIGHWAY")) return "SAHP";
            if (upper.Contains("SASP") || upper.Contains("RANGER")) return "SASP";
            return "Unknown";
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents a variation of an outfit
    /// </summary>
    public class OutfitVariation
    {
        /// <summary>
        /// Variation name (e.g., "Officer", "Officer III")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Script name for the variation
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// Parent outfit
        /// </summary>
        public Outfit ParentOutfit { get; set; }

        /// <summary>
        /// Combined name in format OutfitName.VariationName
        /// </summary>
        public string CombinedName
        {
            get
            {
                if (ParentOutfit != null)
                {
                    return $"{ParentOutfit.Name}.{Name}";
                }
                return Name;
            }
        }

        /// <summary>
        /// Inferred gender (Male/Female) based on naming conventions
        /// </summary>
        public string InferredGender
        {
            get
            {
                string scriptName = ScriptName?.ToLowerInvariant() ?? "";
                if (scriptName.Contains("_f_") || scriptName.Contains("female")) return "Female";
                if (scriptName.Contains("_m_") || scriptName.Contains("male")) return "Male";
                return "Unisex";
            }
        }

        public OutfitVariation()
        {
            Name = string.Empty;
            ScriptName = string.Empty;
        }

        public OutfitVariation(string name, string scriptName, Outfit parentOutfit)
        {
            Name = name;
            ScriptName = scriptName;
            ParentOutfit = parentOutfit;
        }

        public override string ToString()
        {
            return CombinedName;
        }

        public override bool Equals(object obj)
        {
            if (obj is OutfitVariation other)
            {
                return CombinedName.Equals(other.CombinedName, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CombinedName.ToLowerInvariant().GetHashCode();
        }
    }
}
