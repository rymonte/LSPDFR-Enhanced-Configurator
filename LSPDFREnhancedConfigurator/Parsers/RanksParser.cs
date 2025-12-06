using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Parsers
{
    /// <summary>
    /// Parses Ranks.xml files to extract rank progression definitions
    /// </summary>
    public class RanksParser
    {
        /// <summary>
        /// Parse a Ranks.xml file
        /// </summary>
        public static List<Rank> ParseRanksFile(string filePath)
        {
            var ranks = new List<Rank>();

            try
            {
                var doc = XDocument.Load(filePath);
                var rankElements = doc.Descendants("Rank");

                foreach (var rankElement in rankElements)
                {
                    var rank = ParseRank(rankElement);
                    ranks.Add(rank);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse ranks file {filePath}: {ex.Message}", ex);
            }

            return ranks;
        }

        private static Rank ParseRank(XElement rankElement)
        {
            var name = rankElement.Element("Name")?.Value ?? "Unknown";
            var requiredPoints = int.Parse(rankElement.Element("RequiredPoints")?.Value ?? "0");
            var salary = int.Parse(rankElement.Element("Salary")?.Value ?? "0");

            var rank = new Rank(name, requiredPoints, salary);

            // Parse stations
            var stationsElement = rankElement.Element("Stations");
            if (stationsElement != null)
            {
                foreach (var stationElement in stationsElement.Elements("Station"))
                {
                    var stationAssignment = ParseStationAssignment(stationElement);
                    rank.Stations.Add(stationAssignment);
                }
            }

            // Parse vehicles
            var vehiclesElement = rankElement.Element("Vehicles");
            if (vehiclesElement != null)
            {
                foreach (var vehicleElement in vehiclesElement.Elements("Vehicle"))
                {
                    var model = vehicleElement.Attribute("model")?.Value ?? string.Empty;
                    var displayName = vehicleElement.Value?.Trim() ?? model;

                    if (!string.IsNullOrEmpty(model))
                    {
                        var vehicle = new Vehicle(model, displayName, new List<string>());
                        rank.Vehicles.Add(vehicle);
                    }
                }
            }

            // Parse rank-level outfits
            var outfitsElement = rankElement.Element("Outfits");
            if (outfitsElement != null)
            {
                foreach (var outfitElement in outfitsElement.Elements("Outfit"))
                {
                    var outfitName = outfitElement.Value?.Trim();
                    if (!string.IsNullOrEmpty(outfitName))
                    {
                        rank.Outfits.Add(outfitName);
                    }
                }
            }

            return rank;
        }

        private static StationAssignment ParseStationAssignment(XElement stationElement)
        {
            var stationName = stationElement.Element("StationName")?.Value ?? "Unknown";
            var styleId = int.Parse(stationElement.Element("StyleID")?.Value ?? "1");

            var zones = new List<string>();
            var zonesElement = stationElement.Element("Zones");
            if (zonesElement != null)
            {
                foreach (var zoneElement in zonesElement.Elements("Zone"))
                {
                    var zoneName = zoneElement.Value?.Trim();
                    if (!string.IsNullOrEmpty(zoneName))
                    {
                        zones.Add(zoneName);
                    }
                }
            }

            var assignment = new StationAssignment(stationName, zones, styleId);

            // Parse station-level vehicle overrides
            var vehiclesElement = stationElement.Element("Vehicles");
            if (vehiclesElement != null)
            {
                foreach (var vehicleElement in vehiclesElement.Elements("Vehicle"))
                {
                    var model = vehicleElement.Attribute("model")?.Value ?? string.Empty;
                    var displayName = vehicleElement.Value?.Trim() ?? model;

                    if (!string.IsNullOrEmpty(model))
                    {
                        var vehicle = new Vehicle(model, displayName, new List<string>());
                        assignment.VehicleOverrides.Add(vehicle);
                    }
                }
            }

            // Parse station-level outfit overrides
            var outfitsElement = stationElement.Element("Outfits");
            if (outfitsElement != null)
            {
                foreach (var outfitElement in outfitsElement.Elements("Outfit"))
                {
                    var outfitName = outfitElement.Value?.Trim();
                    if (!string.IsNullOrEmpty(outfitName))
                    {
                        assignment.OutfitOverrides.Add(outfitName);
                    }
                }
            }

            return assignment;
        }
    }
}
