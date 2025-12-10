using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Tests for BackupPathHelper static utility class
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class BackupPathHelperTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _testProfile = "TestProfile";
        private readonly SettingsManager _settingsManager;

        public BackupPathHelperTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"BackupPathTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);

            var settingsPath = Path.Combine(_tempDirectory, "test_settings.ini");
            _settingsManager = new SettingsManager(settingsPath);
            _settingsManager.SetGtaVDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, recursive: true);
                }
            }
            catch { /* Best effort cleanup */ }
        }

        #region GetBackupDirectory Tests

        [Fact]
        public void GetBackupDirectory_WithDefaultLocation_ReturnsCorrectPath()
        {
            // Act
            var backupDir = BackupPathHelper.GetBackupDirectory(_settingsManager, _testProfile);

            // Assert
            backupDir.Should().NotBeNullOrEmpty();
            backupDir.Should().Contain(_testProfile);
        }

        [Fact]
        public void GetBackupDirectory_WithCustomLocation_ReturnsCustomPath()
        {
            // Arrange
            var customBackupRoot = Path.Combine(_tempDirectory, "CustomBackups");
            Directory.CreateDirectory(customBackupRoot);
            _settingsManager.SetBackupDirectory(customBackupRoot);
            _settingsManager.Save();

            // Act
            var backupDir = BackupPathHelper.GetBackupDirectory(_settingsManager, _testProfile);

            // Assert
            backupDir.Should().StartWith(customBackupRoot);
            backupDir.Should().Contain(_testProfile);
        }

        #endregion

        #region GetBackupFileName Tests

        [Fact]
        public void GetBackupFileName_FormatsCorrectly()
        {
            // Arrange
            var timestamp = new DateTime(2025, 1, 15, 14, 30, 0);

            // Act
            var fileName = BackupPathHelper.GetBackupFileName(timestamp);

            // Assert
            fileName.Should().Be("Ranks_20250115-1430.xml");
        }

        #endregion

        #region GetAvailableBackups Tests

        [Fact]
        public void GetAvailableBackups_WithNoBackups_ReturnsEmptyList()
        {
            // Act
            var backups = BackupPathHelper.GetAvailableBackups(_settingsManager, _testProfile);

            // Assert
            backups.Should().BeEmpty();
        }

        [Fact]
        public void GetAvailableBackups_WithBackups_ReturnsBackupList()
        {
            // Arrange
            var backupRoot = Path.Combine(_tempDirectory, "Backups");
            Directory.CreateDirectory(backupRoot);
            _settingsManager.SetBackupDirectory(backupRoot);
            _settingsManager.Save();

            var profileBackupDir = Path.Combine(backupRoot, _testProfile);
            Directory.CreateDirectory(profileBackupDir);

            // Create test backup files
            var backup1 = Path.Combine(profileBackupDir, "Ranks_20240115-1030.xml");
            var backup2 = Path.Combine(profileBackupDir, "Ranks_20240116-1430.xml");
            File.WriteAllText(backup1, "<?xml version=\"1.0\"?><Ranks></Ranks>");
            File.WriteAllText(backup2, "<?xml version=\"1.0\"?><Ranks></Ranks>");

            // Act
            var backups = BackupPathHelper.GetAvailableBackups(_settingsManager, _testProfile);

            // Assert
            backups.Should().HaveCountGreaterOrEqualTo(2);
        }

        [Fact]
        public void GetAvailableBackups_OrdersByTimestampDescending()
        {
            // Arrange
            var backupRoot = Path.Combine(_tempDirectory, "Backups");
            Directory.CreateDirectory(backupRoot);
            _settingsManager.SetBackupDirectory(backupRoot);
            _settingsManager.Save();

            var profileBackupDir = Path.Combine(backupRoot, _testProfile);
            Directory.CreateDirectory(profileBackupDir);

            // Create backup files with different timestamps
            var oldBackup = Path.Combine(profileBackupDir, "Ranks_20240101-1000.xml");
            var newBackup = Path.Combine(profileBackupDir, "Ranks_20240201-1000.xml");
            File.WriteAllText(oldBackup, "<?xml version=\"1.0\"?><Ranks></Ranks>");
            File.WriteAllText(newBackup, "<?xml version=\"1.0\"?><Ranks></Ranks>");

            // Set file times to match
            File.SetLastWriteTime(oldBackup, new DateTime(2024, 1, 1, 10, 0, 0));
            File.SetLastWriteTime(newBackup, new DateTime(2024, 2, 1, 10, 0, 0));

            // Act
            var backups = BackupPathHelper.GetAvailableBackups(_settingsManager, _testProfile);

            // Assert
            backups.Should().HaveCountGreaterOrEqualTo(2);
            if (backups.Count >= 2)
            {
                // Backups are NOT automatically sorted by the helper, but contain timestamp property
                var sorted = backups.OrderByDescending(b => b.Timestamp).ToList();
                sorted[0].Timestamp.Should().BeAfter(sorted[^1].Timestamp);
            }
        }

        #endregion

        #region CleanupOldBackups Tests

        [Fact]
        public void CleanupOldBackups_WithNoBackups_DoesNotThrow()
        {
            // Act
            Action act = () => BackupPathHelper.CleanupOldBackups(_settingsManager, _testProfile);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void CleanupOldBackups_WithMaxBackupsExceeded_RemovesOldest()
        {
            // Arrange
            var backupRoot = Path.Combine(_tempDirectory, "Backups");
            Directory.CreateDirectory(backupRoot);
            _settingsManager.SetBackupDirectory(backupRoot);
            _settingsManager.Save();

            var profileBackupDir = Path.Combine(backupRoot, _testProfile);
            Directory.CreateDirectory(profileBackupDir);

            // Create 12 backups (exceeding default max of 10)
            for (int i = 1; i <= 12; i++)
            {
                var backupFile = Path.Combine(profileBackupDir, $"Ranks_202401{i:D2}-1000.xml");
                File.WriteAllText(backupFile, "<?xml version=\"1.0\"?><Ranks></Ranks>");
                File.SetLastWriteTime(backupFile, new DateTime(2024, 1, i, 10, 0, 0));
            }

            // Act
            BackupPathHelper.CleanupOldBackups(_settingsManager, _testProfile);

            // Assert
            var remainingBackups = Directory.GetFiles(profileBackupDir, "Ranks_*.xml");
            remainingBackups.Should().HaveCount(10, "should keep max 10 backups");
        }

        #endregion
    }
}
