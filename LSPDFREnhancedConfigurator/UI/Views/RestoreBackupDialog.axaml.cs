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
            Logger.Info("[RestoreBackupDialog] Confirm button clicked");

            if (DataContext is not RestoreBackupDialogViewModel vm)
            {
                Logger.Error("[RestoreBackupDialog] DataContext is not RestoreBackupDialogViewModel");
                return;
            }

            // If no backups exist, just close
            if (!vm.HasBackups)
            {
                Logger.Info("[RestoreBackupDialog] No backups exist, closing dialog");
                Close(false);
                return;
            }

            if (string.IsNullOrEmpty(vm.SelectedBackupPath) || !File.Exists(vm.SelectedBackupPath))
            {
                Logger.Error("[RestoreBackupDialog] Selected backup file does not exist");
                // TODO: Show error dialog
                return;
            }

            // Perform the restoration
            // TODO: Add confirmation dialog
            try
            {
                Logger.Info($"[RestoreBackupDialog] Starting restore from: {vm.SelectedBackupPath}");

                // Restore the backup
                var ranksPath = vm.GetRanksPath();
                Logger.Info($"[RestoreBackupDialog] Target Ranks.xml path: {ranksPath}");

                var backupFileName = Path.GetFileName(vm.SelectedBackupPath);

                // Create a backup of the current file before overwriting
                if (File.Exists(ranksPath))
                {
                    var tempBackupPath = ranksPath + ".before_restore.xml";
                    Logger.Info($"[RestoreBackupDialog] Creating safety backup at: {tempBackupPath}");
                    File.Copy(ranksPath, tempBackupPath, true);
                    Logger.Info($"[RestoreBackupDialog] Safety backup created successfully");
                }

                // Copy backup file to ranks location
                Logger.Info($"[RestoreBackupDialog] Copying backup file to Ranks.xml location");
                File.Copy(vm.SelectedBackupPath, ranksPath, true);
                Logger.Info($"[RestoreBackupDialog] File copy completed successfully");

                Logger.Info($"[USER] Successfully restored backup: {backupFileName}");
                // TODO: Show success dialog

                Logger.Info("[RestoreBackupDialog] Closing dialog with result=true");
                Close(true);
                Logger.Info("[RestoreBackupDialog] Dialog Close() method returned");
            }
            catch (Exception ex)
            {
                Logger.Error($"[RestoreBackupDialog] Failed to restore backup: {ex.Message}", ex);
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
