using System.Collections.ObjectModel;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class CopyFromRankDialogViewModel : ViewModelBase
    {
        private RankHierarchy? _selectedRank;
        private string _title = "Copy From Rank";
        private string _description = "Select a rank to copy data from";

        public CopyFromRankDialogViewModel()
        {
            AvailableRanks = new ObservableCollection<RankHierarchy>();
        }

        public ObservableCollection<RankHierarchy> AvailableRanks { get; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public RankHierarchy? SelectedRank
        {
            get => _selectedRank;
            set
            {
                if (SetProperty(ref _selectedRank, value))
                {
                    OnPropertyChanged(nameof(CanCopy));
                }
            }
        }

        public bool CanCopy => SelectedRank != null;
    }
}
