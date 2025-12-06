using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a police station definition from stations.xml
    /// </summary>
    public class Station
    {
        /// <summary>
        /// Station name (e.g., "Mission Row Police Station")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Associated agency (e.g., "lspd", "lssd")
        /// </summary>
        public string Agency { get; set; }

        /// <summary>
        /// Script name identifier
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// Position coordinates from XML (e.g., "460.3052f, -990.7862f, 30.68962f")
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Heading direction
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// Parsed X coordinate for 2D map visualization
        /// </summary>
        public float? X { get; private set; }

        /// <summary>
        /// Parsed Y coordinate for 2D map visualization
        /// </summary>
        public float? Y { get; private set; }

        public Station()
        {
            Name = string.Empty;
            Agency = string.Empty;
            ScriptName = string.Empty;
            Position = string.Empty;
            Heading = string.Empty;
        }

        public Station(string name, string agency, string scriptName)
        {
            Name = name;
            Agency = agency;
            ScriptName = scriptName;
            Position = string.Empty;
            Heading = string.Empty;
        }

        /// <summary>
        /// Parse position string into X and Y coordinates for 2D map overlay
        /// </summary>
        public void ParsePosition()
        {
            if (string.IsNullOrEmpty(Position)) return;

            try
            {
                // Handle format like "460.3052f, -990.7862f, 30.68962f"
                var cleanPosition = Position.Replace("f", "").Replace("F", "");
                var parts = cleanPosition.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    if (float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float x))
                    {
                        X = x;
                    }

                    if (float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float y))
                    {
                        Y = y;
                    }
                }
            }
            catch
            {
                // If parsing fails, X and Y remain null
            }
        }

        /// <summary>
        /// Check if coordinates have been successfully parsed
        /// </summary>
        public bool HasValidCoordinates => X.HasValue && Y.HasValue;

        /// <summary>
        /// Display name with agency prefix (e.g., "[LSPD] Mission Row Police Station")
        /// </summary>
        public string DisplayName => $"[{Agency.ToUpper()}] {Name}";

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            if (obj is Station other)
            {
                return Name.Equals(other.Name, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.ToLowerInvariant().GetHashCode();
        }
    }

    /// <summary>
    /// Represents a station entry within a rank, including zones and style
    /// </summary>
    public class StationAssignment : INotifyPropertyChanged
    {
        private Station _stationReference;

        /// <summary>
        /// Station name reference
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// List of zones available at this station for this rank
        /// </summary>
        public List<string> Zones { get; set; }

        /// <summary>
        /// Style ID for this station assignment
        /// </summary>
        public int StyleID { get; set; }

        /// <summary>
        /// Optional: Station-specific vehicle overrides
        /// </summary>
        public List<Vehicle> VehicleOverrides { get; set; }

        /// <summary>
        /// Optional: Station-specific outfit overrides
        /// </summary>
        public List<string> OutfitOverrides { get; set; }

        /// <summary>
        /// Reference to the actual station definition
        /// </summary>
        public Station StationReference
        {
            get => _stationReference;
            set
            {
                if (_stationReference != value)
                {
                    _stationReference = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// Display name with agency prefix in square brackets
        /// </summary>
        public string DisplayName
        {
            get
            {
                var agency = StationReference?.Agency ?? "";
                if (string.IsNullOrWhiteSpace(agency))
                {
                    agency = "UNKNOWN";
                }
                return $"[{agency.ToUpper()}] {StationName}";
            }
        }

        /// <summary>
        /// Indicates if this station reference exists in loaded game data
        /// </summary>
        public bool IsValid => StationReference != null;

        public StationAssignment()
        {
            StationName = string.Empty;
            Zones = new List<string>();
            StyleID = 0;
            VehicleOverrides = new List<Vehicle>();
            OutfitOverrides = new List<string>();
        }

        public StationAssignment(string stationName, List<string> zones, int styleId)
        {
            StationName = stationName;
            Zones = zones ?? new List<string>();
            StyleID = styleId;
            VehicleOverrides = new List<Vehicle>();
            OutfitOverrides = new List<string>();
        }

        public override string ToString()
        {
            return $"{StationName} (Style: {StyleID}, Zones: {Zones.Count})";
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
