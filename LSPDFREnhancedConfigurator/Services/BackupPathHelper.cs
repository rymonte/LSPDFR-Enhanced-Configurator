using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Helper class for managing backup file paths and operations
    /// </summary>
    public static class BackupPathHelper
    {
        /// <summary>
        /// Get the backup directory for a specific profile
        /// </summary>
        public static string GetBackupDirectory(SettingsManager settings, string profileName)
        {
            var effectiveBackupRoot = settings.GetEffectiveBackupDirectory();
            if (string.IsNullOrEmpty(effectiveBackupRoot))
                return string.Empty;

            return Path.Combine(effectiveBackupRoot, profileName);
        }

        /// <summary>
        /// Get the full backup file path for a specific profile and timestamp
        /// </summary>
        public static string GetBackupFilePath(SettingsManager settings, string profileName, DateTime timestamp)
        {
            var backupDir = GetBackupDirectory(settings, profileName);
            var fileName = GetBackupFileName(timestamp);
            return Path.Combine(backupDir, fileName);
        }

        /// <summary>
        /// Generate backup file name from timestamp
        /// Format: Ranks_YYYYMMDD-HHMM.xml
        /// </summary>
        public static string GetBackupFileName(DateTime timestamp)
        {
            return $"Ranks_{timestamp:yyyyMMdd-HHmm}.xml";
        }

        /// <summary>
        /// Get list of available backup files for a specific profile
        /// </summary>
        public static List<BackupFileInfo> GetAvailableBackups(SettingsManager settings, string profileName)
        {
            var backups = new List<BackupFileInfo>();
            var backupDir = GetBackupDirectory(settings, profileName);

            if (string.IsNullOrEmpty(backupDir) || !Directory.Exists(backupDir))
                return backups;

            try
            {
                var files = Directory.GetFiles(backupDir, "Ranks_*.xml");

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var timestamp = ParseBackupTimestamp(fileName);

                    if (timestamp.HasValue)
                    {
                        backups.Add(new BackupFileInfo
                        {
                            FilePath = file,
                            FileName = fileName,
                            Timestamp = timestamp.Value,
                            FileSize = new FileInfo(file).Length
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading backups from {backupDir}: {ex.Message}");
            }

            return backups;
        }

        /// <summary>
        /// Parse timestamp from backup filename
        /// Format: Ranks_YYYYMMDD-HHMM.xml
        /// </summary>
        private static DateTime? ParseBackupTimestamp(string fileName)
        {
            var pattern = @"Ranks_(\d{8})-(\d{4})\.xml";
            var match = Regex.Match(fileName, pattern);

            if (!match.Success)
                return null;

            try
            {
                var dateStr = match.Groups[1].Value; // YYYYMMDD
                var timeStr = match.Groups[2].Value; // HHMM

                var year = int.Parse(dateStr.Substring(0, 4));
                var month = int.Parse(dateStr.Substring(4, 2));
                var day = int.Parse(dateStr.Substring(6, 2));
                var hour = int.Parse(timeStr.Substring(0, 2));
                var minute = int.Parse(timeStr.Substring(2, 2));

                return new DateTime(year, month, day, hour, minute, 0);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Clean up old backups, keeping only the most recent ones
        /// </summary>
        public static void CleanupOldBackups(SettingsManager settings, string profileName)
        {
            try
            {
                var maxBackups = settings.GetInt("maxBackups", 10);
                var backups = GetAvailableBackups(settings, profileName);

                if (backups.Count <= maxBackups)
                    return; // No cleanup needed

                // Sort by timestamp descending (newest first)
                var sortedBackups = backups.OrderByDescending(b => b.Timestamp).ToList();

                // Delete backups beyond the max limit
                var backupsToDelete = sortedBackups.Skip(maxBackups).ToList();

                foreach (var backup in backupsToDelete)
                {
                    try
                    {
                        File.Delete(backup.FilePath);
                        Logger.Info($"Deleted old backup: {backup.FileName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Failed to delete old backup {backup.FileName}: {ex.Message}");
                    }
                }

                if (backupsToDelete.Count > 0)
                {
                    Logger.Info($"Cleaned up {backupsToDelete.Count} old backup(s), keeping {maxBackups} most recent");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during backup cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrate old backup files (*.backup_*.xml) to new format and location
        /// </summary>
        public static void MigrateOldBackups(string gtaRootPath, string profileName, SettingsManager settings)
        {
            try
            {
                // Find Ranks.xml location
                var ranksPath = RanksXmlLoader.FindRanksXml(gtaRootPath, profileName);
                if (ranksPath == null)
                {
                    Logger.Debug($"No Ranks.xml found for profile {profileName}, skipping backup migration");
                    return;
                }

                var ranksDirectory = Path.GetDirectoryName(ranksPath);
                if (string.IsNullOrEmpty(ranksDirectory))
                    return;

                // Find old backup files (*.backup_*.xml)
                var oldBackupPattern = "*.backup_*.xml";
                var oldBackups = Directory.GetFiles(ranksDirectory, oldBackupPattern);

                if (oldBackups.Length == 0)
                {
                    Logger.Debug($"No old backups found for migration in {ranksDirectory}");
                    return;
                }

                Logger.Info($"Found {oldBackups.Length} old backup(s) to migrate for profile {profileName}");

                var newBackupDir = GetBackupDirectory(settings, profileName);
                if (string.IsNullOrEmpty(newBackupDir))
                {
                    Logger.Warn("Cannot migrate backups - no valid backup directory configured");
                    return;
                }

                // Create backup directory if it doesn't exist
                if (!Directory.Exists(newBackupDir))
                {
                    Directory.CreateDirectory(newBackupDir);
                    Logger.Info($"Created backup directory: {newBackupDir}");
                }

                int migratedCount = 0;
                foreach (var oldBackup in oldBackups)
                {
                    try
                    {
                        var fileName = Path.GetFileName(oldBackup);
                        var timestamp = ParseOldBackupTimestamp(fileName);

                        if (!timestamp.HasValue)
                        {
                            Logger.Warn($"Could not parse timestamp from old backup: {fileName}");
                            continue;
                        }

                        // Generate new filename and path
                        var newFileName = GetBackupFileName(timestamp.Value);
                        var newFilePath = Path.Combine(newBackupDir, newFileName);

                        // Skip if new backup already exists
                        if (File.Exists(newFilePath))
                        {
                            Logger.Debug($"Backup already exists at new location: {newFileName}");
                            continue;
                        }

                        // Copy to new location
                        File.Copy(oldBackup, newFilePath, overwrite: false);
                        Logger.Info($"Migrated backup: {fileName} -> {newFileName}");
                        migratedCount++;

                        // Optionally delete old backup after successful migration
                        // Commented out for safety - users can manually delete old backups
                        // File.Delete(oldBackup);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to migrate backup {Path.GetFileName(oldBackup)}: {ex.Message}");
                    }
                }

                if (migratedCount > 0)
                {
                    Logger.Info($"Successfully migrated {migratedCount} backup(s) for profile {profileName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during backup migration: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse timestamp from old backup filename format
        /// Format: Ranks.xml.backup_YYYYMMDD_HHMMSS.xml
        /// </summary>
        private static DateTime? ParseOldBackupTimestamp(string fileName)
        {
            var pattern = @"\.backup_(\d{8})_(\d{6})\.xml";
            var match = Regex.Match(fileName, pattern);

            if (!match.Success)
                return null;

            try
            {
                var dateStr = match.Groups[1].Value; // YYYYMMDD
                var timeStr = match.Groups[2].Value; // HHMMSS

                var year = int.Parse(dateStr.Substring(0, 4));
                var month = int.Parse(dateStr.Substring(4, 2));
                var day = int.Parse(dateStr.Substring(6, 2));
                var hour = int.Parse(timeStr.Substring(0, 2));
                var minute = int.Parse(timeStr.Substring(2, 2));
                var second = int.Parse(timeStr.Substring(4, 2));

                return new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Information about a backup file
    /// </summary>
    public class BackupFileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public long FileSize { get; set; }

        public string DisplayName => $"{Timestamp:yyyy-MM-dd HH:mm} ({FormatFileSize(FileSize)})";

        private static string FormatFileSize(long bytes)
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
    }
}
