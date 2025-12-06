using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.Writers
{
    /// <summary>
    /// Writes Ranks.xml files with validation and backup
    /// </summary>
    public class RanksWriter
    {
        /// <summary>
        /// Validate a list of ranks before writing
        /// </summary>
        public static List<string> ValidateRanks(List<Rank> ranks, DataLoadingService dataService)
        {
            var errors = new List<string>();

            if (ranks.Count == 0)
            {
                errors.Add("No ranks defined. At least one rank is required.");
                return errors;
            }

            // Check XP thresholds increase correctly
            for (int i = 1; i < ranks.Count; i++)
            {
                if (ranks[i].RequiredPoints <= ranks[i - 1].RequiredPoints)
                {
                    errors.Add($"Rank '{ranks[i].Name}' has RequiredPoints ({ranks[i].RequiredPoints}) less than or equal to previous rank '{ranks[i - 1].Name}' ({ranks[i - 1].RequiredPoints})");
                }
            }

            // Validate each rank
            for (int i = 0; i < ranks.Count; i++)
            {
                var rank = ranks[i];
                var rankPrefix = $"Rank {i + 1} ('{rank.Name}')";

                // Check stations exist
                foreach (var stationAssignment in rank.Stations)
                {
                    var station = dataService.Stations.FirstOrDefault(s =>
                        s.Name.Equals(stationAssignment.StationName, StringComparison.OrdinalIgnoreCase));

                    if (station == null)
                    {
                        errors.Add($"{rankPrefix}: Station '{stationAssignment.StationName}' not found in loaded stations");
                    }
                }

                // Check vehicles exist
                foreach (var vehicle in rank.Vehicles)
                {
                    var exists = dataService.AllVehicles.Any(v =>
                        v.Model.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        errors.Add($"{rankPrefix}: Vehicle '{vehicle.Model}' not found in loaded vehicles");
                    }
                }

                // Check outfits exist
                foreach (var outfit in rank.Outfits)
                {
                    var exists = dataService.OutfitVariations.Any(o =>
                        o.CombinedName.Equals(outfit, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        errors.Add($"{rankPrefix}: Outfit '{outfit}' not found in loaded outfits");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Create a backup of the existing Ranks.xml file
        /// </summary>
        public static void CreateBackup(string ranksFilePath)
        {
            if (!File.Exists(ranksFilePath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(ranksFilePath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new Exception("Invalid ranks file path");
            }

            var backupDir = Path.Combine(directory, "backups");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"Ranks_{timestamp}.xml";
            var backupPath = Path.Combine(backupDir, backupFileName);

            File.Copy(ranksFilePath, backupPath, overwrite: true);
        }

        /// <summary>
        /// Write ranks to a Ranks.xml file
        /// </summary>
        public static void WriteRanksFile(string outputPath, List<Rank> ranks)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Ranks",
                    ranks.Select(rank => CreateRankElement(rank))
                )
            );

            // Save with proper formatting
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(outputPath, settings))
            {
                doc.Save(writer);
            }
        }

        private static XElement CreateRankElement(Rank rank)
        {
            var rankElement = new XElement("Rank",
                new XElement("Name", rank.Name),
                new XElement("RequiredPoints", rank.RequiredPoints),
                new XElement("Salary", rank.Salary)
            );

            // Add stations
            if (rank.Stations.Count > 0)
            {
                var stationsElement = new XElement("Stations",
                    rank.Stations.Select(CreateStationElement)
                );
                rankElement.Add(stationsElement);
            }

            // Add vehicles
            if (rank.Vehicles.Count > 0)
            {
                var vehiclesElement = new XElement("Vehicles",
                    rank.Vehicles.Select(v => new XElement("Vehicle",
                        new XAttribute("model", v.Model),
                        v.DisplayName
                    ))
                );
                rankElement.Add(vehiclesElement);
            }

            // Add outfits
            if (rank.Outfits.Count > 0)
            {
                var outfitsElement = new XElement("Outfits",
                    rank.Outfits.Select(o => new XElement("Outfit", o))
                );
                rankElement.Add(outfitsElement);
            }

            return rankElement;
        }

        private static XElement CreateStationElement(StationAssignment station)
        {
            var stationElement = new XElement("Station",
                new XElement("StationName", station.StationName)
            );

            // Add zones
            if (station.Zones.Count > 0)
            {
                var zonesElement = new XElement("Zones",
                    station.Zones.Select(z => new XElement("Zone", z))
                );
                stationElement.Add(zonesElement);
            }

            // Add StyleID
            stationElement.Add(new XElement("StyleID", station.StyleID));

            // Add vehicle overrides if any
            if (station.VehicleOverrides.Count > 0)
            {
                var vehiclesElement = new XElement("Vehicles",
                    station.VehicleOverrides.Select(v => new XElement("Vehicle",
                        new XAttribute("model", v.Model),
                        v.DisplayName
                    ))
                );
                stationElement.Add(vehiclesElement);
            }

            // Add outfit overrides if any
            if (station.OutfitOverrides.Count > 0)
            {
                var outfitsElement = new XElement("Outfits",
                    station.OutfitOverrides.Select(o => new XElement("Outfit", o))
                );
                stationElement.Add(outfitsElement);
            }

            return stationElement;
        }

        /// <summary>
        /// Generate a human-readable summary of all ranks
        /// </summary>
        public static string GenerateSummary(List<Rank> ranks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RANKS SUMMARY ===\n");

            foreach (var rank in ranks)
            {
                sb.AppendLine($"Rank: {rank.Name}");
                sb.AppendLine($"  Required Points: {rank.RequiredPoints}");
                sb.AppendLine($"  Salary: ${rank.Salary}");
                sb.AppendLine($"  Stations: {rank.Stations.Count}");

                foreach (var station in rank.Stations)
                {
                    sb.AppendLine($"    - {station.StationName} (Style: {station.StyleID}, Zones: {station.Zones.Count})");

                    if (station.VehicleOverrides.Count > 0)
                    {
                        sb.AppendLine($"      Vehicle Overrides: {station.VehicleOverrides.Count}");
                    }

                    if (station.OutfitOverrides.Count > 0)
                    {
                        sb.AppendLine($"      Outfit Overrides: {station.OutfitOverrides.Count}");
                    }
                }

                sb.AppendLine($"  Vehicles: {rank.Vehicles.Count}");
                sb.AppendLine($"  Outfits: {rank.Outfits.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
