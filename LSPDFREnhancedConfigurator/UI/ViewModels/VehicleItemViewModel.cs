using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// ViewModel for a vehicle item in the AddVehiclesDialog list
    /// </summary>
    public class VehicleItemViewModel : ViewModelBase
    {
        private bool _isSelected;

        public VehicleItemViewModel(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public Vehicle Vehicle { get; }

        public string DisplayName => Vehicle.DisplayName;
        public string Model => Vehicle.Model;
        public string Agencies => string.Join(", ", Vehicle.Agencies.Select(a => a.ToUpper()));

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
