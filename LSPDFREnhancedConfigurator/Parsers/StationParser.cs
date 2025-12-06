using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Parsers
{
    /// <summary>
    /// Parses stations.xml files to extract police station definitions
    /// </summary>
    public class StationParser
    {
        /// <summary>
        /// Parse a stations XML file
        /// </summary>
        public static List<Station> ParseStationsFile(string filePath)
        {
            var stations = new List<Station>();

            try
            {
                var doc = XDocument.Load(filePath);
                var stationElements = doc.Descendants("Station");

                foreach (var stationElement in stationElements)
                {
                    var station = ParseStation(stationElement);
                    stations.Add(station);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse stations file {filePath}: {ex.Message}", ex);
            }

            return stations;
        }

        private static Station ParseStation(XElement stationElement)
        {
            var name = stationElement.Element("Name")?.Value ?? "Unknown";
            var agency = stationElement.Element("Agency")?.Value ?? "unknown";
            var scriptName = stationElement.Element("ScriptName")?.Value ?? "unknown";
            var position = stationElement.Element("Position")?.Value ?? string.Empty;
            var heading = stationElement.Element("Heading")?.Value ?? string.Empty;

            var station = new Station(name, agency, scriptName)
            {
                Position = position,
                Heading = heading
            };

            // Parse coordinates for map visualization
            station.ParsePosition();

            return station;
        }

        /// <summary>
        /// Merge stations from multiple files, avoiding duplicates by name
        /// </summary>
        public static List<Station> MergeStations(List<Station> stations)
        {
            var mergedStations = new Dictionary<string, Station>(StringComparer.OrdinalIgnoreCase);

            foreach (var station in stations)
            {
                if (!mergedStations.ContainsKey(station.Name))
                {
                    mergedStations[station.Name] = station;
                }
            }

            return mergedStations.Values.ToList();
        }
    }
}
