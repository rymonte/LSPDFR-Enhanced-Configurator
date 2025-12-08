using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Parsers;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Loads and aggregates all LSPDFR configuration data
    /// </summary>
    public class DataLoadingService
    {
        private readonly FileDiscoveryService _fileDiscovery;

        public virtual List<Agency> Agencies { get; private set; } = new List<Agency>();
        public virtual List<Vehicle> AllVehicles { get; private set; } = new List<Vehicle>();
        public virtual List<Station> Stations { get; private set; } = new List<Station>();
        public virtual List<OutfitVariation> OutfitVariations { get; private set; } = new List<OutfitVariation>();
        public virtual List<Rank> Ranks { get; private set; } = new List<Rank>();

        public DataLoadingService(FileDiscoveryService fileDiscovery)
        {
            _fileDiscovery = fileDiscovery;
        }

        /// <summary>
        /// Load all configuration data from the LSPDFR installation
        /// </summary>
        public void LoadAll()
        {
            Logger.Section("DataLoadingService.LoadAll");
            Logger.Debug("Starting data loading process");

            LoadDutySelections();
            LoadAgencies();
            LoadStations();
            LoadOutfits();
            LoadRanks();

            Logger.Info("Data loading completed successfully");
            Logger.Debug($"  - Agencies: {Agencies.Count}");
            Logger.Debug($"  - Vehicles: {AllVehicles.Count}");
            Logger.Debug($"  - Stations: {Stations.Count}");
            Logger.Debug($"  - Outfits: {OutfitVariations.Count}");
            Logger.Debug($"  - Ranks: {Ranks.Count}");
        }

        private void LoadDutySelections()
        {
            Logger.Debug("Loading duty selections...");
            var dutyFiles = _fileDiscovery.FindDutySelectionFiles();
            Logger.Debug($"Found {dutyFiles.Count} duty selection file(s)");

            var allDescriptions = new List<Dictionary<string, VehicleDescription>>();

            foreach (var file in dutyFiles)
            {
                try
                {
                    Logger.Debug($"  Parsing: {Path.GetFileName(file)}");
                    var descriptions = DutySelectionParser.ParseDutySelectionFile(file);
                    allDescriptions.Add(descriptions);
                    Logger.Debug($"    Loaded {descriptions.Count} vehicle descriptions");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error parsing duty selection file {file}", ex);
                }
            }

            // Store merged descriptions for later use with vehicles
            if (allDescriptions.Count > 0)
            {
                _vehicleDescriptions = DutySelectionParser.MergeDescriptions(allDescriptions.ToArray());
                Logger.Debug($"Merged {_vehicleDescriptions.Count} total vehicle descriptions");
            }
        }

        private Dictionary<string, VehicleDescription> _vehicleDescriptions = new Dictionary<string, VehicleDescription>();

        private void LoadAgencies()
        {
            Logger.Debug("Loading agencies...");
            var agencyFiles = _fileDiscovery.FindAgencyFiles();
            Logger.Debug($"Found {agencyFiles.Count} agency file(s)");

            var allAgencies = new List<Agency>();

            foreach (var file in agencyFiles)
            {
                try
                {
                    Logger.Debug($"  Parsing: {Path.GetFileName(file)}");
                    var agencies = AgencyParser.ParseAgencyFile(file);
                    allAgencies.AddRange(agencies);
                    Logger.Debug($"    Loaded {agencies.Count} agency/agencies");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error parsing agency file {file}", ex);
                }
            }

            // Merge agencies from all files
            Agencies = AgencyParser.MergeAgencies(allAgencies);
            Logger.Debug($"Merged into {Agencies.Count} unique agencies");

            // Log discovered agencies
            Logger.Debug("Discovered agencies:");
            foreach (var agency in Agencies.OrderBy(a => a.ShortName))
            {
                Logger.Debug($"  - {agency.ShortName}: {agency.Name} ({agency.Vehicles.Count} vehicles)");
            }

            // Apply vehicle descriptions
            ApplyVehicleDescriptions();

            // Build complete vehicle list, consolidating vehicles from multiple agencies
            var vehicleDict = new Dictionary<string, Vehicle>(StringComparer.OrdinalIgnoreCase);
            foreach (var agency in Agencies)
            {
                foreach (var vehicle in agency.Vehicles)
                {
                    if (vehicleDict.ContainsKey(vehicle.Model))
                    {
                        // Merge agencies - add this agency if not already in the list
                        var existing = vehicleDict[vehicle.Model];
                        if (!existing.Agencies.Contains(agency.ShortName, StringComparer.OrdinalIgnoreCase))
                        {
                            existing.Agencies.Add(agency.ShortName);
                        }
                    }
                    else
                    {
                        vehicleDict[vehicle.Model] = vehicle;
                    }
                }
            }
            AllVehicles = vehicleDict.Values.ToList();
        }

        private void ApplyVehicleDescriptions()
        {
            foreach (var agency in Agencies)
            {
                foreach (var vehicle in agency.Vehicles)
                {
                    if (_vehicleDescriptions.ContainsKey(vehicle.Model))
                    {
                        var desc = _vehicleDescriptions[vehicle.Model];
                        vehicle.DisplayName = desc.FullName;

                        // Update agency associations
                        if (!vehicle.Agencies.Contains(desc.AgencyRef))
                        {
                            vehicle.Agencies.Add(desc.AgencyRef);
                        }
                    }
                }
            }
        }

        private void LoadStations()
        {
            Logger.Debug("Loading stations...");
            var stationFiles = _fileDiscovery.FindStationFiles();
            Logger.Debug($"Found {stationFiles.Count} station file(s)");

            var allStations = new List<Station>();

            foreach (var file in stationFiles)
            {
                try
                {
                    Logger.Debug($"  Parsing: {Path.GetFileName(file)}");
                    var stations = StationParser.ParseStationsFile(file);
                    allStations.AddRange(stations);
                    Logger.Debug($"    Loaded {stations.Count} station(s)");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error parsing station file {file}", ex);
                }
            }

            // Merge stations from all files
            Stations = StationParser.MergeStations(allStations);
            Logger.Debug($"Merged into {Stations.Count} unique stations");

            // Log discovered stations grouped by agency
            Logger.Debug("Discovered stations by agency:");
            var stationsByAgency = Stations.GroupBy(s => s.Agency).OrderBy(g => g.Key);
            foreach (var group in stationsByAgency)
            {
                Logger.Debug($"  {group.Key.ToUpper()}: {group.Count()} stations");
                foreach (var station in group.OrderBy(s => s.Name))
                {
                    Logger.Debug($"    - {station.Name}");
                }
            }
        }

        private void LoadOutfits()
        {
            Logger.Debug("Loading outfits...");
            var outfitFiles = _fileDiscovery.FindOutfitFiles();
            Logger.Debug($"Found {outfitFiles.Count} outfit file(s)");

            var allVariations = new List<OutfitVariation>();

            foreach (var file in outfitFiles)
            {
                try
                {
                    Logger.Debug($"  Parsing: {Path.GetFileName(file)}");
                    var variations = OutfitParser.ParseOutfitsFile(file);
                    allVariations.AddRange(variations);

                    // Get outfit name from first variation (all variations share the same parent outfit)
                    var outfitName = variations.Count > 0 ? variations[0].ParentOutfit.Name : "Unknown";
                    Logger.Info($"    Loaded {variations.Count} outfit variation(s) for outfit {outfitName}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error parsing outfit file {file}", ex);
                }
            }

            // Merge variations from all files
            OutfitVariations = OutfitParser.MergeOutfits(allVariations);
            Logger.Debug($"Merged into {OutfitVariations.Count} unique outfit variations");
        }

        private void LoadRanks()
        {
            Logger.Debug("Loading ranks...");
            var ranksFile = _fileDiscovery.FindRanksFile();

            if (ranksFile != null)
            {
                Logger.Debug($"Found Ranks.xml: {ranksFile}");
                try
                {
                    Ranks = RanksParser.ParseRanksFile(ranksFile);
                    Logger.Debug($"Loaded {Ranks.Count} rank(s) from Ranks.xml");

                    // Link station references
                    LinkStationReferences();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error parsing ranks file {ranksFile}", ex);
                    Ranks = new List<Rank>();
                }
            }
            else
            {
                Logger.Warn("No Ranks.xml file found. Starting with empty ranks list.");
                Ranks = new List<Rank>();
            }
        }

        private void LinkStationReferences()
        {
            foreach (var rank in Ranks)
            {
                foreach (var stationAssignment in rank.Stations)
                {
                    var station = Stations.FirstOrDefault(s =>
                        s.Name.Equals(stationAssignment.StationName, StringComparison.OrdinalIgnoreCase));

                    if (station != null)
                    {
                        stationAssignment.StationReference = station;
                    }
                    else
                    {
                        Logger.Warn($"Station reference not found for '{stationAssignment.StationName}' in rank '{rank.Name}'. Station will display as [UNKNOWN].");
                    }
                }
            }
        }

        /// <summary>
        /// Link station and vehicle references for external RankHierarchy objects (e.g., loaded from XML)
        /// </summary>
        public void LinkStationReferencesForHierarchies(List<RankHierarchy> hierarchies)
        {
            foreach (var hierarchy in hierarchies)
            {
                // Link for the hierarchy itself
                LinkStationReferencesForRank(hierarchy);
                LinkVehicleReferencesForRank(hierarchy);

                // Link for all pay bands
                foreach (var payBand in hierarchy.PayBands)
                {
                    LinkStationReferencesForRank(payBand);
                    LinkVehicleReferencesForRank(payBand);
                }
            }
        }

        private void LinkStationReferencesForRank(RankHierarchy rank)
        {
            foreach (var stationAssignment in rank.Stations)
            {
                var station = Stations.FirstOrDefault(s =>
                    s.Name.Equals(stationAssignment.StationName, StringComparison.OrdinalIgnoreCase));

                if (station != null)
                {
                    stationAssignment.StationReference = station;
                    Logger.Info($"Linked station reference: '{stationAssignment.StationName}' -> Agency: {station.Agency}");
                }
                else
                {
                    Logger.Warn($"Station reference not found for '{stationAssignment.StationName}' in rank '{rank.Name}'. Station will display as [UNKNOWN].");
                }
            }
        }

        private void LinkVehicleReferencesForRank(RankHierarchy rank)
        {
            for (int i = 0; i < rank.Vehicles.Count; i++)
            {
                var vehicle = rank.Vehicles[i];
                var masterVehicle = AllVehicles.FirstOrDefault(v =>
                    v.Model.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase));

                if (masterVehicle != null)
                {
                    // Replace with master vehicle reference (which has proper Agencies list)
                    rank.Vehicles[i] = masterVehicle;
                    Logger.Info($"Linked vehicle reference: '{vehicle.Model}' -> Agencies: {string.Join(", ", masterVehicle.Agencies)}");
                }
                else
                {
                    Logger.Warn($"Vehicle reference not found for '{vehicle.Model}' in rank '{rank.Name}'. Vehicle will display as [UNKNOWN].");
                }
            }
        }

        /// <summary>
        /// Get all vehicles filtered by agency
        /// </summary>
        public List<Vehicle> GetVehiclesByAgency(string agencyScriptName)
        {
            return AllVehicles.Where(v => v.BelongsToAgency(agencyScriptName)).ToList();
        }

        /// <summary>
        /// Get all stations filtered by agency
        /// </summary>
        public List<Station> GetStationsByAgency(string agencyScriptName)
        {
            return Stations.Where(s => s.Agency.Equals(agencyScriptName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Get all outfit variations filtered by inferred agency
        /// </summary>
        public List<OutfitVariation> GetOutfitsByAgency(string agencyScriptName)
        {
            return OutfitVariations.Where(o =>
                o.ParentOutfit.InferredAgency.Equals(agencyScriptName, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
