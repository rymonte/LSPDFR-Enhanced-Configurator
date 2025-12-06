using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class ProfileSelectionDialogViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private string? _selectedProfile;

        public ProfileSelectionDialogViewModel(List<string> profiles, SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            Profiles = new ObservableCollection<string>(profiles.OrderBy(p => p));

            SelectCommand = new RelayCommand(OnSelect, CanSelect);

            LoadSelectedProfile();
        }

        #region Properties

        public ObservableCollection<string> Profiles { get; }

        public string? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (SetProperty(ref _selectedProfile, value))
                {
                    ((RelayCommand)SelectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SelectCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanSelect()
        {
            return !string.IsNullOrEmpty(SelectedProfile);
        }

        private void OnSelect()
        {
            if (!string.IsNullOrEmpty(SelectedProfile))
            {
                Logger.Info($"User selected profile: {SelectedProfile}");
                _settingsManager.SetSelectedProfile(SelectedProfile);
            }
        }

        #endregion

        #region Helper Methods

        private void LoadSelectedProfile()
        {
            Logger.Info($"Loading {Profiles.Count} profile(s) for selection");

            // Only auto-select if there's a saved profile in settings
            var savedProfile = _settingsManager.GetSelectedProfile();
            if (!string.IsNullOrEmpty(savedProfile) && Profiles.Contains(savedProfile))
            {
                SelectedProfile = savedProfile;
                Logger.Info($"Auto-selected last used profile: {savedProfile}");
            }
            else
            {
                Logger.Info("No saved profile found - user must select from list");
            }
        }

        #endregion
    }
}
