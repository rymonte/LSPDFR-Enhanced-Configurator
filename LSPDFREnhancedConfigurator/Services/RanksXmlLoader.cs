using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    public class RanksXmlLoader
    {
        /// <summary>
        /// Loads ranks from a Ranks.xml file
        /// </summary>
        public static List<RankHierarchy> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Ranks.xml file not found at: {filePath}");
            }

            try
            {
                var doc = XDocument.Load(filePath);
                var ranksElement = doc.Root;

                if (ranksElement == null || ranksElement.Name != "Ranks")
                {
                    throw new InvalidDataException("Invalid Ranks.xml file: Root element must be <Ranks>");
                }

                var ranks = new List<Rank>();

                foreach (var rankElement in ranksElement.Elements("Rank"))
                {
                    var rank = ParseRankElement(rankElement);
                    ranks.Add(rank);
                }

                // Convert to RankHierarchy
                return ConvertToHierarchy(ranks);
            }
            catch (System.Xml.XmlException ex)
            {
                throw new InvalidDataException($"Failed to parse Ranks.xml: {ex.Message}", ex);
            }
        }

        private static Rank ParseRankElement(XElement rankElement)
        {
            var name = rankElement.Element("Name")?.Value ?? "Unknown";
            var requiredPoints = int.Parse(rankElement.Element("RequiredPoints")?.Value ?? "0");
            var salary = int.Parse(rankElement.Element("Salary")?.Value ?? "0");

            var rank = new Rank(name, requiredPoints, salary);

            // Parse Stations
            var stationsElement = rankElement.Element("Stations");
            if (stationsElement != null)
            {
                foreach (var stationElement in stationsElement.Elements("Station"))
                {
                    var stationName = stationElement.Element("StationName")?.Value ?? "";
                    var styleId = int.Parse(stationElement.Element("StyleID")?.Value ?? "1");

                    var zones = new List<string>();
                    var zonesElement = stationElement.Element("Zones");
                    if (zonesElement != null)
                    {
                        foreach (var zoneElement in zonesElement.Elements("Zone"))
                        {
                            var zoneName = zoneElement.Value;
                            if (!string.IsNullOrEmpty(zoneName))
                            {
                                zones.Add(zoneName);
                            }
                        }
                    }

                    rank.Stations.Add(new StationAssignment(stationName, zones, styleId));
                }
            }

            // Parse Vehicles
            var vehiclesElement = rankElement.Element("Vehicles");
            if (vehiclesElement != null)
            {
                foreach (var vehicleElement in vehiclesElement.Elements("Vehicle"))
                {
                    var model = vehicleElement.Attribute("model")?.Value ?? "";
                    var displayName = vehicleElement.Value;

                    if (!string.IsNullOrEmpty(model))
                    {
                        // We don't know the agency yet, will be populated later
                        rank.Vehicles.Add(new Vehicle(model, displayName, ""));
                    }
                }
            }

            // Parse Outfits
            var outfitsElement = rankElement.Element("Outfits");
            if (outfitsElement != null)
            {
                foreach (var outfitElement in outfitsElement.Elements("Outfit"))
                {
                    var outfitName = outfitElement.Value;
                    if (!string.IsNullOrEmpty(outfitName))
                    {
                        rank.Outfits.Add(outfitName);
                    }
                }
            }

            return rank;
        }

        private static List<RankHierarchy> ConvertToHierarchy(List<Rank> ranks)
        {
            var hierarchies = new List<RankHierarchy>();

            if (ranks.Count == 0)
            {
                return hierarchies;
            }

            // Group ranks by base name (e.g., "Officer I", "Officer II" -> "Officer")
            var grouped = new Dictionary<string, List<Rank>>();

            foreach (var rank in ranks)
            {
                var baseName = GetBaseName(rank.Name);

                if (!grouped.ContainsKey(baseName))
                {
                    grouped[baseName] = new List<Rank>();
                }

                grouped[baseName].Add(rank);
            }

            // Create hierarchies
            int currentXP = 0;
            foreach (var group in grouped)
            {
                var groupRanks = group.Value.OrderBy(r => r.RequiredPoints).ToList();

                if (groupRanks.Count == 1)
                {
                    // Single rank, no pay bands
                    var rank = groupRanks[0];

                    var hierarchy = new RankHierarchy(rank.Name, rank.RequiredPoints, rank.Salary)
                    {
                        Stations = rank.Stations,
                        Vehicles = rank.Vehicles,
                        Outfits = rank.Outfits
                    };

                    hierarchies.Add(hierarchy);
                }
                else
                {
                    // Multiple ranks with same base name - create parent with pay bands
                    var firstRank = groupRanks[0];

                    var parent = new RankHierarchy(group.Key, firstRank.RequiredPoints, firstRank.Salary);

                    foreach (var rank in groupRanks)
                    {
                        var payBand = new RankHierarchy(rank.Name, rank.RequiredPoints, rank.Salary)
                        {
                            Parent = parent,
                            Stations = rank.Stations,
                            Vehicles = rank.Vehicles,
                            Outfits = rank.Outfits
                        };

                        parent.PayBands.Add(payBand);
                    }

                    parent.IsParent = true;
                    hierarchies.Add(parent);
                }
            }

            return hierarchies;
        }

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
            var ranksPath = Path.Combine(gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", profileName, "Ranks.xml");

            if (File.Exists(ranksPath))
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
            var profilesPath = Path.Combine(gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles");

            if (!Directory.Exists(profilesPath))
            {
                return new List<string>();
            }

            return Directory.GetDirectories(profilesPath)
                .Select(d => Path.GetFileName(d))
                .ToList();
        }
    }
}
