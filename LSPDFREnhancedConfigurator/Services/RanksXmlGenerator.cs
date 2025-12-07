using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    public class RanksXmlGenerator
    {
        /// <summary>
        /// Custom StringWriter that uses UTF-8 encoding instead of UTF-16
        /// </summary>
        private class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding => new UTF8Encoding(false);
        }
        public static string GenerateXml(List<RankHierarchy> rankHierarchies)
        {
            var ranksElement = new XElement("Ranks");

            foreach (var hierarchy in rankHierarchies)
            {
                // If it's a parent with pay bands, generate ranks for each pay band
                if (hierarchy.IsParent && hierarchy.PayBands.Count > 0)
                {
                    foreach (var payBand in hierarchy.PayBands)
                    {
                        ranksElement.Add(CreateRankElement(payBand));
                    }
                }
                else
                {
                    // Standalone rank
                    ranksElement.Add(CreateRankElement(hierarchy));
                }
            }

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                ranksElement
            );

            var settings = new StringBuilder();
            using (var writer = new Utf8StringWriter(settings))
            {
                doc.Save(writer);
                return settings.ToString();
            }
        }

        private static XElement CreateRankElement(RankHierarchy rank)
        {
            var rankElement = new XElement("Rank");

            // Name
            rankElement.Add(new XElement("Name", rank.Name));

            // RequiredPoints
            rankElement.Add(new XElement("RequiredPoints", rank.RequiredPoints));

            // Salary
            rankElement.Add(new XElement("Salary", rank.Salary));

            // Stations
            if (rank.Stations.Count > 0)
            {
                var stationsElement = new XElement("Stations");

                foreach (var station in rank.Stations)
                {
                    var stationElement = new XElement("Station");

                    stationElement.Add(new XElement("StationName", station.StationName));

                    // Zones
                    if (station.Zones.Count > 0)
                    {
                        var zonesElement = new XElement("Zones");
                        foreach (var zone in station.Zones)
                        {
                            zonesElement.Add(new XElement("Zone", zone));
                        }
                        stationElement.Add(zonesElement);
                    }

                    // StyleID
                    stationElement.Add(new XElement("StyleID", station.StyleID));

                    stationsElement.Add(stationElement);
                }

                rankElement.Add(stationsElement);
            }

            // Vehicles
            if (rank.Vehicles.Count > 0)
            {
                var vehiclesElement = new XElement("Vehicles");

                foreach (var vehicle in rank.Vehicles)
                {
                    vehiclesElement.Add(new XElement("Vehicle",
                        new XAttribute("model", vehicle.Model),
                        vehicle.DisplayName));
                }

                rankElement.Add(vehiclesElement);
            }

            // Outfits
            if (rank.Outfits.Count > 0)
            {
                var outfitsElement = new XElement("Outfits");

                foreach (var outfit in rank.Outfits)
                {
                    outfitsElement.Add(new XElement("Outfit", outfit));
                }

                rankElement.Add(outfitsElement);
            }

            return rankElement;
        }

        public static List<string> ValidateRanks(List<RankHierarchy> rankHierarchies)
        {
            var errors = new List<string>();

            if (rankHierarchies.Count == 0)
            {
                errors.Add("No ranks defined. At least one rank is required.");
                return errors;
            }

            // Flatten all ranks (including pay bands)
            var allRanks = new List<RankHierarchy>();
            foreach (var hierarchy in rankHierarchies)
            {
                if (hierarchy.IsParent && hierarchy.PayBands.Count > 0)
                {
                    allRanks.AddRange(hierarchy.PayBands);
                }
                else
                {
                    allRanks.Add(hierarchy);
                }
            }

            // Check first rank starts at 0
            if (allRanks[0].RequiredPoints != 0)
            {
                errors.Add($"First rank '{allRanks[0].Name}' must start at XP 0, not {allRanks[0].RequiredPoints}.");
            }

            // Check for XP progression - each rank must have RequiredPoints greater than previous
            for (int i = 0; i < allRanks.Count - 1; i++)
            {
                var current = allRanks[i];
                var next = allRanks[i + 1];

                if (next.RequiredPoints <= current.RequiredPoints)
                {
                    errors.Add($"Rank '{next.Name}' (XP: {next.RequiredPoints}) must have Required Points greater than '{current.Name}' (XP: {current.RequiredPoints}).");
                }
            }

            // Check each rank has at least one station assigned
            foreach (var rank in allRanks)
            {
                if (rank.Stations.Count == 0)
                {
                    errors.Add($"Rank '{rank.Name}' has no stations assigned. Each rank must have at least one station.");
                }
            }

            return errors;
        }
    }
}
