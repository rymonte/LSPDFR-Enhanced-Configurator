using System;
using System.Linq;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.Testing
{
    /// <summary>
    /// Test harness for parser functionality
    /// </summary>
    public static class ParserTests
    {
        public static void RunAllTests(string sampleDataPath)
        {
            Console.WriteLine("=== LSPDFR Enhanced Configurator PARSER TESTS ===\n");

            var fileDiscovery = new FileDiscoveryService(sampleDataPath);
            var dataLoader = new DataLoadingService(fileDiscovery);

            // Test 1: Validate GTA V root
            Console.WriteLine("TEST 1: Validating sample data path...");
            if (fileDiscovery.IsValidGTAVRoot())
            {
                Console.WriteLine("✓ Valid LSPDFR installation detected\n");
            }
            else
            {
                Console.WriteLine("✗ WARNING: Not a standard GTA V/LSPDFR folder structure\n");
            }

            // Test 2: File Discovery
            Console.WriteLine("TEST 2: Discovering XML files...");
            var agencyFiles = fileDiscovery.FindAgencyFiles();
            var stationFiles = fileDiscovery.FindStationFiles();
            var outfitFiles = fileDiscovery.FindOutfitFiles();
            var dutyFiles = fileDiscovery.FindDutySelectionFiles();
            var ranksFile = fileDiscovery.FindRanksFile();

            Console.WriteLine($"  Agency files found: {agencyFiles.Count}");
            Console.WriteLine($"  Station files found: {stationFiles.Count}");
            Console.WriteLine($"  Outfit files found: {outfitFiles.Count}");
            Console.WriteLine($"  Duty selection files found: {dutyFiles.Count}");
            Console.WriteLine($"  Ranks file: {(ranksFile != null ? "Found" : "Not found")}");
            Console.WriteLine();

            // Test 3: Load All Data
            Console.WriteLine("TEST 3: Loading and parsing all XML files...");
            try
            {
                dataLoader.LoadAll();
                Console.WriteLine("✓ All files loaded successfully\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ ERROR loading files: {ex.Message}\n");
                return;
            }

            // Test 4: Data Statistics
            Console.WriteLine("TEST 4: Data Statistics");
            Console.WriteLine($"  Total Agencies: {dataLoader.Agencies.Count}");
            Console.WriteLine($"  Total Vehicles: {dataLoader.AllVehicles.Count}");
            Console.WriteLine($"  Total Stations: {dataLoader.Stations.Count}");
            Console.WriteLine($"  Total Outfit Variations: {dataLoader.OutfitVariations.Count}");
            Console.WriteLine($"  Total Ranks: {dataLoader.Ranks.Count}");
            Console.WriteLine();

            // Test 5: Agency Details
            Console.WriteLine("TEST 5: Agency Details");
            foreach (var agency in dataLoader.Agencies.Take(5))
            {
                Console.WriteLine($"  {agency.ShortName} ({agency.ScriptName}): {agency.Vehicles.Count} vehicles");
            }
            if (dataLoader.Agencies.Count > 5)
            {
                Console.WriteLine($"  ... and {dataLoader.Agencies.Count - 5} more agencies");
            }
            Console.WriteLine();

            // Test 6: Vehicle Display Names
            Console.WriteLine("TEST 6: Sample Vehicles with Display Names");
            var sampleVehicles = dataLoader.AllVehicles.Take(10).ToList();
            foreach (var vehicle in sampleVehicles)
            {
                Console.WriteLine($"  {vehicle.Model} -> {vehicle.DisplayName} [{vehicle.PrimaryAgency}]");
            }
            if (dataLoader.AllVehicles.Count > 10)
            {
                Console.WriteLine($"  ... and {dataLoader.AllVehicles.Count - 10} more vehicles");
            }
            Console.WriteLine();

            // Test 7: Station Details with Coordinates
            Console.WriteLine("TEST 7: Sample Stations with Coordinates");
            foreach (var station in dataLoader.Stations.Take(5))
            {
                if (station.HasValidCoordinates)
                {
                    Console.WriteLine($"  {station.Name} ({station.Agency}) - X: {station.X:F2}, Y: {station.Y:F2}");
                }
                else
                {
                    Console.WriteLine($"  {station.Name} ({station.Agency}) - No coordinates");
                }
            }
            if (dataLoader.Stations.Count > 5)
            {
                Console.WriteLine($"  ... and {dataLoader.Stations.Count - 5} more stations");
            }
            var stationsWithCoords = dataLoader.Stations.Count(s => s.HasValidCoordinates);
            Console.WriteLine($"  Total stations with valid coordinates: {stationsWithCoords}/{dataLoader.Stations.Count}");
            Console.WriteLine();

            // Test 8: Outfit Variations
            Console.WriteLine("TEST 8: Sample Outfit Variations");
            foreach (var outfit in dataLoader.OutfitVariations.Take(10))
            {
                Console.WriteLine($"  {outfit.CombinedName} (Gender: {outfit.InferredGender})");
            }
            if (dataLoader.OutfitVariations.Count > 10)
            {
                Console.WriteLine($"  ... and {dataLoader.OutfitVariations.Count - 10} more outfits");
            }
            Console.WriteLine();

            // Test 9: Ranks Details
            Console.WriteLine("TEST 9: Ranks Overview");
            if (dataLoader.Ranks.Count > 0)
            {
                foreach (var rank in dataLoader.Ranks.Take(3))
                {
                    Console.WriteLine($"  {rank.Name}:");
                    Console.WriteLine($"    XP: {rank.RequiredPoints}, Salary: ${rank.Salary}");
                    Console.WriteLine($"    Stations: {rank.Stations.Count}, Vehicles: {rank.Vehicles.Count}, Outfits: {rank.Outfits.Count}");

                    // Check for station-specific items
                    var stationsWithItems = rank.Stations.Where(s =>
                        s.Vehicles.Count > 0 || s.Outfits.Count > 0).ToList();

                    if (stationsWithItems.Count > 0)
                    {
                        Console.WriteLine($"    Stations with specific items: {stationsWithItems.Count}");
                    }
                }
                if (dataLoader.Ranks.Count > 3)
                {
                    Console.WriteLine($"  ... and {dataLoader.Ranks.Count - 3} more ranks");
                }
            }
            else
            {
                Console.WriteLine("  No ranks found in data");
            }
            Console.WriteLine();

            // Test 10: Validation
            Console.WriteLine("TEST 10: Validating Ranks");
            if (dataLoader.Ranks.Count > 0)
            {
                var validationService = new StartupValidationService(dataLoader);
                var report = validationService.ValidateRanks(dataLoader.Ranks);
                var errors = report.Errors.Select(e => e.Message).ToList();
                if (errors.Count == 0)
                {
                    Console.WriteLine("✓ All ranks are valid!\n");
                }
                else
                {
                    Console.WriteLine($"✗ Found {errors.Count} validation errors:");
                    foreach (var error in errors.Take(10))
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    if (errors.Count > 10)
                    {
                        Console.WriteLine($"  ... and {errors.Count - 10} more errors");
                    }
                    Console.WriteLine();
                }
            }

            // Test 11: Disabled (GenerateSummary will be ported separately)
            Console.WriteLine("TEST 11: Generate Ranks Summary");
            Console.WriteLine("  (Summary generation feature to be restored later)\n");

            Console.WriteLine("=== ALL TESTS COMPLETED ===");
        }
    }
}
