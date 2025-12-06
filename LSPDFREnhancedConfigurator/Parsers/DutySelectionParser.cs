using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Parsers
{
    /// <summary>
    /// Parses duty_selection.xml to extract vehicle display names
    /// </summary>
    public class DutySelectionParser
    {
        /// <summary>
        /// Parse duty_selection.xml and return a dictionary of vehicle model -> display name
        /// </summary>
        public static Dictionary<string, VehicleDescription> ParseDutySelectionFile(string filePath)
        {
            var vehicleDescriptions = new Dictionary<string, VehicleDescription>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var doc = XDocument.Load(filePath);
                var descriptionElements = doc.Descendants("Description");

                foreach (var descElement in descriptionElements)
                {
                    var model = descElement.Value?.Trim();
                    var fullName = descElement.Attribute("fullName")?.Value ?? model;
                    var agencyRef = descElement.Attribute("agencyRef")?.Value ?? "unknown";

                    if (!string.IsNullOrEmpty(model))
                    {
                        vehicleDescriptions[model] = new VehicleDescription
                        {
                            Model = model,
                            FullName = fullName,
                            AgencyRef = agencyRef
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse duty selection file {filePath}: {ex.Message}", ex);
            }

            return vehicleDescriptions;
        }

        /// <summary>
        /// Merge vehicle descriptions from multiple files
        /// </summary>
        public static Dictionary<string, VehicleDescription> MergeDescriptions(
            params Dictionary<string, VehicleDescription>[] descriptionSets)
        {
            var merged = new Dictionary<string, VehicleDescription>(StringComparer.OrdinalIgnoreCase);

            foreach (var descSet in descriptionSets)
            {
                foreach (var kvp in descSet)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }

            return merged;
        }
    }

    /// <summary>
    /// Represents a vehicle description from duty_selection.xml
    /// </summary>
    public class VehicleDescription
    {
        public string Model { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AgencyRef { get; set; } = string.Empty;
    }
}
