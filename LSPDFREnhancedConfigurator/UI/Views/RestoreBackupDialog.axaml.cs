using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class RestoreBackupDialog : Window
    {
        public RestoreBackupDialog()
        {
            InitializeComponent();
        }

        private async void ChooseButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not RestoreBackupDialogViewModel vm) return;

            var backupRoot = vm.GetBackupRoot();

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose Backup File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("XML Backup Files") { Patterns = new[] { "*.xml" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                },
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(backupRoot)
            });

            if (files.Count > 0)
            {
                var selectedPath = files[0].Path.LocalPath;
                vm.UpdateAfterFileSelection(selectedPath);
            }
        }

        private void ConfirmButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not RestoreBackupDialogViewModel vm) return;

            // If no backups exist, just close
            if (!vm.HasBackups)
            {
                Close(false);
                return;
            }

            if (string.IsNullOrEmpty(vm.SelectedBackupPath) || !File.Exists(vm.SelectedBackupPath))
            {
                Logger.Error("Selected backup file does not exist");
                // TODO: Show error dialog
                return;
            }

            // Perform the restoration
            // TODO: Add confirmation dialog
            try
            {
                // Restore the backup
                var ranksPath = vm.GetRanksPath();
                var backupFileName = Path.GetFileName(vm.SelectedBackupPath);

                // Create a backup of the current file before overwriting
                if (File.Exists(ranksPath))
                {
                    var tempBackupPath = ranksPath + ".before_restore.xml";
                    File.Copy(ranksPath, tempBackupPath, true);
                    Logger.Info($"Created temporary backup before restore: {tempBackupPath}");
                }

                // Copy backup file to ranks location
                File.Copy(vm.SelectedBackupPath, ranksPath, true);

                Logger.Info($"[USER] Successfully restored backup: {backupFileName}");
                // TODO: Show success dialog

                Close(true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore backup: {ex.Message}", ex);
                // TODO: Show error dialog
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Logger.Info("[USER] Restore backup cancelled");
            Close(false);
        }
    }
}
