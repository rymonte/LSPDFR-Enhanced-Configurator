using System.Collections.Generic;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a rank in the LSPDFR progression system
    /// </summary>
    public class Rank
    {
        /// <summary>
        /// Rank name (e.g., "Rookie", "Officer I")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Required XP points to achieve this rank
        /// </summary>
        public int RequiredPoints { get; set; }

        /// <summary>
        /// Salary for this rank
        /// </summary>
        public int Salary { get; set; }

        /// <summary>
        /// List of stations available at this rank
        /// </summary>
        public List<StationAssignment> Stations { get; set; }

        /// <summary>
        /// List of vehicles available at this rank
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// List of outfit combined names available at this rank
        /// </summary>
        public List<string> Outfits { get; set; }

        public Rank()
        {
            Name = string.Empty;
            RequiredPoints = 0;
            Salary = 0;
            Stations = new List<StationAssignment>();
            Vehicles = new List<Vehicle>();
            Outfits = new List<string>();
        }

        public Rank(string name, int requiredPoints, int salary)
        {
            Name = name;
            RequiredPoints = requiredPoints;
            Salary = salary;
            Stations = new List<StationAssignment>();
            Vehicles = new List<Vehicle>();
            Outfits = new List<string>();
        }

        /// <summary>
        /// Creates a deep copy of this rank
        /// </summary>
        public Rank Clone()
        {
            var clone = new Rank(Name + " (Copy)", RequiredPoints, Salary);

            foreach (var station in Stations)
            {
                var stationClone = new StationAssignment(
                    station.StationName,
                    new List<string>(station.Zones),
                    station.StyleID
                );
                stationClone.VehicleOverrides = new List<Vehicle>(station.VehicleOverrides);
                stationClone.OutfitOverrides = new List<string>(station.OutfitOverrides);
                stationClone.StationReference = station.StationReference;
                clone.Stations.Add(stationClone);
            }

            clone.Vehicles = new List<Vehicle>(Vehicles);
            clone.Outfits = new List<string>(Outfits);

            return clone;
        }

        public override string ToString()
        {
            return $"{Name} (XP: {RequiredPoints}, Salary: ${Salary})";
        }
    }
}
