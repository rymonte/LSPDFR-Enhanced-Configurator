using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    public class RanksXmlGenerator
    {
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

            var doc = new XDocument(ranksElement);

            // Generate XML without declaration, then prepend UTF-8 declaration manually
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true // Don't generate declaration
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, xmlSettings))
            {
                doc.Save(writer);
            }

            // Manually prepend UTF-8 declaration
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + sb.ToString();
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

    }
}
