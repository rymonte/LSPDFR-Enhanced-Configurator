using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class RestoreBackupDialogViewModel : ViewModelBase
    {
        private readonly string _gtaRootPath;
        private readonly string _profileName;
        private readonly SettingsManager _settingsManager;
        private List<Services.BackupFileInfo> _availableBackups = new List<Services.BackupFileInfo>();
        private string? _selectedBackupPath;
        private string _instructionsText = "Would you like to restore your latest backup?";
        private string _fileDetailsText = string.Empty;
        private string _chooseButtonText = "Choose Backup File";
        private string _confirmButtonText = "Yes";
        private bool _hasBackups = true;

        private enum DialogState
        {
            Initial,      // Latest backup shown
            FileChosen    // Custom backup chosen
        }

        private DialogState _currentState = DialogState.Initial;

        public RestoreBackupDialogViewModel(string gtaRootPath, string profileName, SettingsManager settingsManager)
        {
            _gtaRootPath = gtaRootPath;
            _profileName = profileName;
            _settingsManager = settingsManager;

            ChooseBackupCommand = new RelayCommand(OnChooseBackup);
            ConfirmCommand = new RelayCommand(OnConfirm);

            LoadAvailableBackups();
        }

        #region Properties

        public string InstructionsText
        {
            get => _instructionsText;
            set => SetProperty(ref _instructionsText, value);
        }

        public string FileDetailsText
        {
            get => _fileDetailsText;
            set => SetProperty(ref _fileDetailsText, value);
        }

        public string ChooseButtonText
        {
            get => _chooseButtonText;
            set => SetProperty(ref _chooseButtonText, value);
        }

        public string ConfirmButtonText
        {
            get => _confirmButtonText;
            set => SetProperty(ref _confirmButtonText, value);
        }

        public bool HasBackups
        {
            get => _hasBackups;
            set => SetProperty(ref _hasBackups, value);
        }

        public string? SelectedBackupPath => _selectedBackupPath;

        #endregion

        #region Commands

        public ICommand ChooseBackupCommand { get; }
        public ICommand ConfirmCommand { get; }

        #endregion

        #region Command Handlers

        private void OnChooseBackup()
        {
            // This will trigger file dialog in code-behind
        }

        private void OnConfirm()
        {
            // This will be handled in code-behind with message boxes
        }

        #endregion

        #region Helper Methods

        private void LoadAvailableBackups()
        {
            try
            {
                // Use BackupPathHelper to get backups
                _availableBackups = Services.BackupPathHelper.GetAvailableBackups(_settingsManager, _profileName)
                    .OrderByDescending(b => b.Timestamp)
                    .ToList();

                if (_availableBackups.Count == 0)
                {
                    ShowNoBackupsMessage();
                    return;
                }

                // Show latest backup by default
                _selectedBackupPath = _availableBackups[0].FilePath;
                DisplayBackupDetails(_availableBackups[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load available backups", ex);
                ShowNoBackupsMessage();
            }
        }

        private void ShowNoBackupsMessage()
        {
            HasBackups = false;
            InstructionsText = "No backups found for this profile.";
            FileDetailsText = "No backup files exist for this profile.\n\n" +
                             "Backups are created automatically when you generate Ranks.xml.";
            ChooseButtonText = "Choose File Manually";
            ConfirmButtonText = "Close";
        }

        private void DisplayBackupDetails(Services.BackupFileInfo backup)
        {
            var details = $"Backup File: {backup.FileName}\n" +
                         $"Created: {backup.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                         $"File Size: {FormatFileSize(backup.FileSize)}\n" +
                         $"\nFull Path:\n{backup.FilePath}";

            FileDetailsText = details;
            Logger.Info($"Displaying backup details: {backup.FileName}");
        }

        public void UpdateAfterFileSelection(string selectedPath)
        {
            _selectedBackupPath = selectedPath;
            var backup = new Services.BackupFileInfo
            {
                FilePath = selectedPath,
                FileName = Path.GetFileName(selectedPath),
                Timestamp = File.GetLastWriteTime(selectedPath),
                FileSize = new FileInfo(selectedPath).Length
            };
            DisplayBackupDetails(backup);

            _currentState = DialogState.FileChosen;
            UpdateButtonStates();

            Logger.Info($"[USER] Selected backup file: {Path.GetFileName(selectedPath)}");
        }

        private void UpdateButtonStates()
        {
            if (_currentState == DialogState.Initial)
            {
                ChooseButtonText = "Choose Backup File";
                ConfirmButtonText = "Yes";
                InstructionsText = "Would you like to restore your latest backup?";
            }
            else // DialogState.FileChosen
            {
                ChooseButtonText = "Choose Another";
                ConfirmButtonText = "Confirm";
                InstructionsText = "Confirm restoration from the selected backup file:";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public string GetBackupRoot()
        {
            return Services.BackupPathHelper.GetBackupDirectory(_settingsManager, _profileName);
        }

        public string GetRanksPath()
        {
            return Path.Combine(_gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced",
                "Profiles", _profileName, "Ranks.xml");
        }

        #endregion
    }
}
