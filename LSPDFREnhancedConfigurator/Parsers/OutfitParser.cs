using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Parsers
{
    /// <summary>
    /// Parses outfits.xml files to extract outfit definitions and variations
    /// </summary>
    public class OutfitParser
    {
        /// <summary>
        /// Parse an outfits XML file
        /// </summary>
        public static List<OutfitVariation> ParseOutfitsFile(string filePath)
        {
            var allVariations = new List<OutfitVariation>();

            try
            {
                var doc = XDocument.Load(filePath);
                var outfitElements = doc.Descendants("Outfit");

                foreach (var outfitElement in outfitElements)
                {
                    var outfitName = outfitElement.Element("Name")?.Value ?? "Unknown";
                    var scriptName = outfitElement.Element("ScriptName")?.Value ?? "unknown";

                    var outfit = new Outfit(outfitName, scriptName);

                    // Parse all variations
                    var variationElements = outfitElement.Descendants("Variation");
                    foreach (var varElement in variationElements)
                    {
                        var variation = ParseVariation(varElement, outfit);
                        if (variation != null)
                        {
                            outfit.Variations.Add(variation);
                            allVariations.Add(variation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse outfits file {filePath}: {ex.Message}", ex);
            }

            return allVariations;
        }

        private static OutfitVariation? ParseVariation(XElement varElement, Outfit parentOutfit)
        {
            var name = varElement.Element("Name")?.Value;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var scriptName = varElement.Element("ScriptName")?.Value ?? string.Empty;
            var gender = varElement.Element("Gender")?.Value ?? "Unisex";

            var variation = new OutfitVariation(name, scriptName, parentOutfit);
            return variation;
        }

        /// <summary>
        /// Merge outfit variations from multiple files
        /// </summary>
        public static List<OutfitVariation> MergeOutfits(List<OutfitVariation> variations)
        {
            var mergedVariations = new Dictionary<string, OutfitVariation>(StringComparer.OrdinalIgnoreCase);

            foreach (var variation in variations)
            {
                var key = variation.CombinedName;
                if (!mergedVariations.ContainsKey(key))
                {
                    mergedVariations[key] = variation;
                }
            }

            return mergedVariations.Values.ToList();
        }
    }
}
