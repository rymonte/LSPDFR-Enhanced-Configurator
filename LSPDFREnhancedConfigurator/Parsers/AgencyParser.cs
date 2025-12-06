using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Parsers
{
    /// <summary>
    /// Parses Agency XML files to extract vehicle and agency information
    /// </summary>
    public class AgencyParser
    {
        /// <summary>
        /// Parse an Agency XML file and extract all agencies and their vehicles
        /// </summary>
        public static List<Agency> ParseAgencyFile(string filePath)
        {
            var agencies = new List<Agency>();

            try
            {
                var doc = XDocument.Load(filePath);
                var agencyElements = doc.Descendants("Agency");

                foreach (var agencyElement in agencyElements)
                {
                    var agency = ParseAgency(agencyElement);
                    agencies.Add(agency);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse agency file {filePath}: {ex.Message}", ex);
            }

            return agencies;
        }

        private static Agency ParseAgency(XElement agencyElement)
        {
            var name = agencyElement.Element("Name")?.Value ?? "Unknown";
            var shortName = agencyElement.Element("ShortName")?.Value ?? "Unknown";
            var scriptName = agencyElement.Element("ScriptName")?.Value ?? "unknown";

            var agency = new Agency(name, shortName, scriptName);

            // Parse all loadouts to extract vehicles
            var loadouts = agencyElement.Descendants("Loadout");
            foreach (var loadout in loadouts)
            {
                var vehicleElements = loadout.Descendants("Vehicle");
                foreach (var vehicleElement in vehicleElements)
                {
                    var vehicleModel = vehicleElement.Value?.Trim();
                    if (!string.IsNullOrEmpty(vehicleModel))
                    {
                        // Check if this vehicle already exists in the agency
                        if (!agency.Vehicles.Any(v => v.Model.Equals(vehicleModel, StringComparison.OrdinalIgnoreCase)))
                        {
                            var vehicle = new Vehicle(vehicleModel, vehicleModel, scriptName);
                            agency.Vehicles.Add(vehicle);
                        }
                    }
                }
            }

            return agency;
        }

        /// <summary>
        /// Merge multiple agency lists, combining vehicles from the same agencies
        /// </summary>
        public static List<Agency> MergeAgencies(List<Agency> agencies)
        {
            var mergedAgencies = new Dictionary<string, Agency>(StringComparer.OrdinalIgnoreCase);

            foreach (var agency in agencies)
            {
                if (mergedAgencies.ContainsKey(agency.ScriptName))
                {
                    // Merge vehicles into existing agency
                    var existing = mergedAgencies[agency.ScriptName];
                    foreach (var vehicle in agency.Vehicles)
                    {
                        if (!existing.Vehicles.Any(v => v.Model.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase)))
                        {
                            existing.Vehicles.Add(vehicle);
                        }
                    }
                }
                else
                {
                    mergedAgencies[agency.ScriptName] = agency;
                }
            }

            return mergedAgencies.Values.ToList();
        }
    }
}
