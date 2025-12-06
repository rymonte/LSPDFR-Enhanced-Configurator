using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// ViewModel for an outfit item in the AddOutfitsDialog list
    /// </summary>
    public class OutfitItemViewModel : ViewModelBase
    {
        private bool _isSelected;

        public OutfitItemViewModel(OutfitVariation outfit)
        {
            Outfit = outfit;
        }

        public OutfitVariation Outfit { get; }

        public string CombinedName => Outfit.CombinedName;

        public string OutfitName => Outfit.ParentOutfit?.Name ?? Outfit.Name;

        public string VariationName => Outfit.ParentOutfit != null ? Outfit.Name : string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
