using System;
using System.Collections.Generic;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a rank that can have pay band children
    /// </summary>
    public class RankHierarchy
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int RequiredPoints { get; set; }
        public int Salary { get; set; }
        public bool IsParent { get; set; } // Has pay bands
        public RankHierarchy? Parent { get; set; }
        public List<RankHierarchy> PayBands { get; set; } = new List<RankHierarchy>();

        /// <summary>
        /// List of stations available at this rank
        /// </summary>
        public List<StationAssignment> Stations { get; set; } = new List<StationAssignment>();

        /// <summary>
        /// List of vehicles available at this rank
        /// </summary>
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        /// <summary>
        /// List of outfit combined names available at this rank
        /// </summary>
        public List<string> Outfits { get; set; } = new List<string>();

        public RankHierarchy()
        {
        }

        public RankHierarchy(string name, int requiredPoints, int salary)
        {
            Name = name;
            RequiredPoints = requiredPoints;
            Salary = salary;
        }

        /// <summary>
        /// Generate Roman numeral for pay band index
        /// </summary>
        public static string GetRomanNumeral(int number)
        {
            if (number < 1) return "";

            string[] romanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
            return number <= 10 ? romanNumerals[number - 1] : number.ToString();
        }

        /// <summary>
        /// Add a pay band with auto-generated name
        /// </summary>
        public RankHierarchy AddPayBand()
        {
            var payBandNumber = PayBands.Count + 1;
            var payBandName = $"{this.Name} {GetRomanNumeral(payBandNumber)}";

            // Use constructor to initialize collections
            var payBand = new RankHierarchy(payBandName, this.RequiredPoints, this.Salary)
            {
                Parent = this,
                IsParent = false
            };

            PayBands.Add(payBand);
            this.IsParent = true;

            return payBand;
        }

        /// <summary>
        /// Clone this rank hierarchy
        /// </summary>
        public RankHierarchy Clone()
        {
            var clone = new RankHierarchy
            {
                Name = this.Name + " (Copy)",
                RequiredPoints = this.RequiredPoints,
                Salary = this.Salary,
                IsParent = this.IsParent
            };

            // Clone stations with deep copy
            foreach (var station in Stations)
            {
                var stationClone = new StationAssignment(
                    station.StationName,
                    new List<string>(station.Zones),
                    station.StyleID
                );
                stationClone.Vehicles = new List<Vehicle>(station.Vehicles);
                stationClone.Outfits = new List<string>(station.Outfits);
                stationClone.StationReference = station.StationReference;
                clone.Stations.Add(stationClone);
            }

            // Clone vehicles and outfits
            clone.Vehicles = new List<Vehicle>(this.Vehicles);
            clone.Outfits = new List<string>(this.Outfits);

            // Clone pay bands
            foreach (var payBand in PayBands)
            {
                var clonedPayBand = payBand.Clone();
                clonedPayBand.Parent = clone;
                clone.PayBands.Add(clonedPayBand);
            }

            return clone;
        }

        /// <summary>
        /// Promote pay band to parent level
        /// </summary>
        public void PromoteToParent()
        {
            if (Parent == null) return;

            Parent = null;
            IsParent = false;
            PayBands.Clear();
        }

        /// <summary>
        /// Summary for display
        /// </summary>
        public string GetSummary(RankHierarchy? nextRank = null)
        {
            if (IsParent && PayBands.Count > 0)
            {
                // For parent ranks, show the range across all pay bands
                var minXP = PayBands.Min(p => p.RequiredPoints);
                var maxXP = PayBands.Max(p => p.RequiredPoints);
                var minSalary = PayBands.Min(p => p.Salary);
                var maxSalary = PayBands.Max(p => p.Salary);

                // Show the XP range of the pay bands, with + if there's a next rank
                var xpRange = nextRank != null ? $"{minXP}-{maxXP}+" : $"{minXP}-{maxXP}";

                return $"{Name} (XP: {xpRange} | ${minSalary:N0}-${maxSalary:N0})";
            }
            else
            {
                var maxXP = nextRank?.RequiredPoints ?? int.MaxValue;
                return $"{Name} (XP: {RequiredPoints}{(maxXP != int.MaxValue ? $"-{maxXP - 1}" : "+")} | ${Salary:N0})";
            }
        }

        /// <summary>
        /// Override ToString for ComboBox display
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
