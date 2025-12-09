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
        /// Parse a Ranks.xml file and return rank hierarchies (with pay band support)
        /// </summary>
        public static List<RankHierarchy> ParseRanksFile(string filePath)
        {
            var ranks = new List<RankHierarchy>();

            try
            {
                var doc = XDocument.Load(filePath);
                var rankElements = doc.Descendants("Rank");

                foreach (var rankElement in rankElements)
                {
                    var rank = ParseRankHierarchy(rankElement);
                    ranks.Add(rank);
                }

                // Convert to hierarchy with pay bands
                return ConvertToHierarchy(ranks);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse ranks file {filePath}: {ex.Message}", ex);
            }
        }

        private static RankHierarchy ParseRankHierarchy(XElement rankElement)
        {
            var name = rankElement.Element("Name")?.Value ?? "Unknown";
            var requiredPoints = int.Parse(rankElement.Element("RequiredPoints")?.Value ?? "0");
            var salary = int.Parse(rankElement.Element("Salary")?.Value ?? "0");

            var rank = new RankHierarchy(name, requiredPoints, salary);

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
                        assignment.Vehicles.Add(vehicle);
                    }
                }
            }

            // Parse station-level outfits
            var outfitsElement = stationElement.Element("Outfits");
            if (outfitsElement != null)
            {
                foreach (var outfitElement in outfitsElement.Elements("Outfit"))
                {
                    var outfitName = outfitElement.Value?.Trim();
                    if (!string.IsNullOrEmpty(outfitName))
                    {
                        assignment.Outfits.Add(outfitName);
                    }
                }
            }

            return assignment;
        }

        /// <summary>
        /// Convert flat list of ranks to hierarchy with pay bands
        /// </summary>
        private static List<RankHierarchy> ConvertToHierarchy(List<RankHierarchy> ranks)
        {
            var hierarchies = new List<RankHierarchy>();

            if (ranks.Count == 0)
            {
                return hierarchies;
            }

            // Group ranks by base name (e.g., "Officer I", "Officer II" -> "Officer")
            var grouped = new Dictionary<string, List<RankHierarchy>>();

            foreach (var rank in ranks)
            {
                var baseName = GetBaseName(rank.Name);

                if (!grouped.ContainsKey(baseName))
                {
                    grouped[baseName] = new List<RankHierarchy>();
                }

                grouped[baseName].Add(rank);
            }

            // Create hierarchies
            foreach (var group in grouped)
            {
                var groupRanks = group.Value.OrderBy(r => r.RequiredPoints).ToList();

                if (groupRanks.Count == 1)
                {
                    // Single rank, no pay bands
                    var rank = groupRanks[0];
                    hierarchies.Add(rank);
                }
                else
                {
                    // Multiple ranks with same base name - create parent with pay bands
                    var firstRank = groupRanks[0];

                    var parent = new RankHierarchy(group.Key, firstRank.RequiredPoints, firstRank.Salary);

                    foreach (var rank in groupRanks)
                    {
                        // Convert rank to pay band
                        rank.Parent = parent;
                        parent.PayBands.Add(rank);
                    }

                    parent.IsParent = true;
                    hierarchies.Add(parent);
                }
            }

            return hierarchies;
        }

        /// <summary>
        /// Extract base name from rank name (remove Roman numerals)
        /// </summary>
        private static string GetBaseName(string rankName)
        {
            // Remove Roman numerals and trailing numbers
            // "Officer I" -> "Officer"
            // "Officer III+I" -> "Officer"
            // "Sergeant II" -> "Sergeant"

            var parts = rankName.Split(' ');
            if (parts.Length > 1)
            {
                var lastPart = parts[parts.Length - 1];

                // Check if last part is a Roman numeral or contains Roman numerals
                if (IsRomanNumeralOrVariation(lastPart))
                {
                    return string.Join(" ", parts.Take(parts.Length - 1));
                }
            }

            return rankName;
        }

        /// <summary>
        /// Check if text is a Roman numeral or variation
        /// </summary>
        private static bool IsRomanNumeralOrVariation(string text)
        {
            // Check for Roman numerals (I, II, III, IV, V, etc.) or variations like "III+I"
            if (string.IsNullOrEmpty(text))
                return false;

            // Remove + signs and check if remaining is Roman numeral
            text = text.Replace("+", "");

            return System.Text.RegularExpressions.Regex.IsMatch(text, "^[IVXLCDM]+$");
        }

        /// <summary>
        /// Finds the Ranks.xml file in the LSPDFR Enhanced profiles directory
        /// </summary>
        public static string FindRanksXml(string gtaRootPath, string profileName = "Default")
        {
            var ranksPath = System.IO.Path.Combine(gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", profileName, "Ranks.xml");

            if (System.IO.File.Exists(ranksPath))
            {
                return ranksPath;
            }

            return null;
        }

        /// <summary>
        /// Gets all available profiles
        /// </summary>
        public static List<string> GetAvailableProfiles(string gtaRootPath)
        {
            var profilesPath = System.IO.Path.Combine(gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles");

            if (!System.IO.Directory.Exists(profilesPath))
            {
                return new List<string>();
            }

            return System.IO.Directory.GetDirectories(profilesPath)
                .Select(d => System.IO.Path.GetFileName(d))
                .ToList();
        }
    }
}
