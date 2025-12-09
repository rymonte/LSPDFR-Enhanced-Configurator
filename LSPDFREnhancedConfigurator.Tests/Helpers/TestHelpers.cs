using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;

namespace LSPDFREnhancedConfigurator.Tests.Helpers
{
    /// <summary>
    /// Shared test utilities for creating test data and validating XML.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a basic rank with default values and one station.
        /// </summary>
        public static RankHierarchy CreateBasicRank(string name = "Officer", int xp = 0, int salary = 1000)
        {
            var station = new StationAssignmentBuilder()
                .WithName("Mission Row")
                .WithZone("Downtown")
                .WithStyleId(1)
                .Build();

            return new RankHierarchyBuilder()
                .WithName(name)
                .WithXP(xp)
                .WithSalary(salary)
                .WithStation(station)
                .Build();
        }

        /// <summary>
        /// Creates a station with specified vehicles and outfits.
        /// </summary>
        public static StationAssignment CreateStationWithItems(
            string stationName,
            List<Vehicle>? vehicles = null,
            List<string>? outfits = null,
            List<string>? zones = null,
            int styleID = 1)
        {
            var builder = new StationAssignmentBuilder()
                .WithName(stationName)
                .WithStyleId(styleID);

            // Only add zones if explicitly provided (including empty lists)
            if (zones != null)
            {
                if (zones.Count > 0)
                {
                    builder.WithZones(zones.ToArray());
                }
                // If zones is an empty list, don't add any zones
            }
            else
            {
                // Default: add a single zone when zones parameter is null
                builder.WithZone("Zone1");
            }

            if (vehicles != null)
            {
                builder.WithVehicles(vehicles.ToArray());
            }

            if (outfits != null)
            {
                builder.WithOutfits(outfits.ToArray());
            }

            return builder.Build();
        }

        /// <summary>
        /// Creates a test vehicle with specified model and display name.
        /// </summary>
        public static Vehicle CreateVehicle(string model, string displayName)
        {
            return new VehicleBuilder()
                .WithModel(model)
                .WithDisplayName(displayName)
                .Build();
        }

        /// <summary>
        /// Parses XML string and validates it's well-formed.
        /// </summary>
        public static XDocument ParseAndValidateXml(string xml)
        {
            xml.Should().NotBeNullOrWhiteSpace("XML should not be empty");

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"XML is not well-formed: {ex.Message}", ex);
            }

            doc.Root.Should().NotBeNull("XML should have a root element");
            return doc;
        }

        /// <summary>
        /// Gets a Rank element by index from the XML document.
        /// </summary>
        public static XElement GetRankElement(XDocument doc, int index = 0)
        {
            var ranks = doc.Descendants("Rank").ToList();
            ranks.Should().HaveCountGreaterThan(index, $"Should have at least {index + 1} Rank element(s)");
            return ranks[index];
        }

        /// <summary>
        /// Gets a Station element by index from a Rank element.
        /// </summary>
        public static XElement GetStationElement(XElement rankElement, int index = 0)
        {
            var stations = rankElement.Descendants("Station").ToList();
            stations.Should().HaveCountGreaterThan(index, $"Should have at least {index + 1} Station element(s)");
            return stations[index];
        }

        /// <summary>
        /// Creates a temporary XML file with the given content and returns the path.
        /// File will be created in the temp directory.
        /// </summary>
        public static string CreateTempXmlFile(string xmlContent)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xml");
            File.WriteAllText(tempPath, xmlContent);
            return tempPath;
        }

        /// <summary>
        /// Deletes a temporary file if it exists.
        /// </summary>
        public static void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        /// <summary>
        /// Creates a rank with both global and station-specific vehicles for testing.
        /// </summary>
        public static RankHierarchy CreateRankWithBothGlobalAndStationVehicles()
        {
            var rank = CreateBasicRank();

            // Add global vehicle
            rank.Vehicles.Add(CreateVehicle("police", "Global Police Car"));

            // Add station-specific vehicle
            rank.Stations[0].Vehicles.Add(CreateVehicle("police2", "Station Police Car"));

            return rank;
        }

        /// <summary>
        /// Creates a rank with both global and station-specific outfits for testing.
        /// </summary>
        public static RankHierarchy CreateRankWithBothGlobalAndStationOutfits()
        {
            var rank = CreateBasicRank();

            // Add global outfit
            rank.Outfits.Add("LSPD_Standard_Uniform");

            // Add station-specific outfit
            rank.Stations[0].Outfits.Add("MissionRow_Uniform");

            return rank;
        }

        /// <summary>
        /// Creates a complex rank hierarchy for testing with multiple stations and items.
        /// </summary>
        public static RankHierarchy CreateComplexRank()
        {
            var rank = new RankHierarchy("Senior Officer", 500, 2000);

            // Global vehicles and outfits
            rank.Vehicles.Add(CreateVehicle("police", "Standard Cruiser"));
            rank.Vehicles.Add(CreateVehicle("police2", "Interceptor"));
            rank.Outfits.Add("LSPD_Standard");
            rank.Outfits.Add("LSPD_Tactical");

            // First station with specific items
            var station1 = CreateStationWithItems(
                "Mission Row",
                vehicles: new List<Vehicle>
                {
                    CreateVehicle("police3", "Mission Row Special")
                },
                outfits: new List<string> { "MissionRow_Custom" },
                zones: new List<string> { "Downtown", "Little Seoul" },
                styleID: 1
            );
            rank.Stations.Add(station1);

            // Second station with different items
            var station2 = CreateStationWithItems(
                "Vespucci",
                vehicles: new List<Vehicle>
                {
                    CreateVehicle("police4", "Beach Patrol")
                },
                outfits: new List<string> { "Vespucci_Shorts" },
                zones: new List<string> { "Vespucci Beach" },
                styleID: 2
            );
            rank.Stations.Add(station2);

            return rank;
        }

        /// <summary>
        /// Verifies that XML starts with proper UTF-8 declaration.
        /// </summary>
        public static void VerifyUtf8Declaration(string xml)
        {
            xml.Should().StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                "XML should start with UTF-8 declaration");
        }

        /// <summary>
        /// Verifies that an XElement has a specific child element.
        /// </summary>
        public static XElement GetRequiredElement(XElement parent, string elementName)
        {
            var element = parent.Element(elementName);
            element.Should().NotBeNull($"Element '{elementName}' should exist");
            return element!;
        }

        /// <summary>
        /// Verifies that an XElement has a specific attribute.
        /// </summary>
        public static string GetRequiredAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            attribute.Should().NotBeNull($"Attribute '{attributeName}' should exist");
            return attribute!.Value;
        }
    }
}
