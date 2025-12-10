using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Parsers;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Integration
{
    /// <summary>
    /// Integration tests verifying components work together correctly
    /// </summary>
    [Trait("Category", "Integration")]
    [Trait("Component", "Integration")]
    public class ComponentIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public ComponentIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        #region DataLoadingService Integration

        [Fact]
        public void DataLoadingService_LoadsAllDataFiles_Successfully()
        {
            // Arrange & Act
            var dataService = _fixture.DataService;

            // Assert - Verify data was loaded from test files
            dataService.Agencies.Should().NotBeEmpty("agencies.xml should be loaded");
            dataService.Stations.Should().NotBeEmpty("stations.xml should be loaded");

            // Verify specific test data
            dataService.Agencies.Should().Contain(a => a.Name == "Los Santos Police Department");
            dataService.Stations.Should().Contain(s => s.Name == "Mission Row Police Station");
        }

        [Fact]
        public void FileDiscoveryService_FindsTestDataFiles()
        {
            // Arrange & Act
            var fileDiscovery = _fixture.FileDiscoveryService;
            var agencyFiles = fileDiscovery.FindAgencyFiles();
            var stationFiles = fileDiscovery.FindStationFiles();

            // Assert
            agencyFiles.Should().NotBeEmpty("should find agencies.xml");
            stationFiles.Should().NotBeEmpty("should find stations.xml");
        }

        #endregion

        #region MainWindowViewModel Integration

        [Fact]
        public void MainWindowViewModel_InitializesWithLoadedData()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build()
            };

            var settingsManager = new SettingsManager(
                Path.Combine(_fixture.TempGtaDirectory, "integration_test1.ini"));

            // Act
            var mainViewModel = new MainWindowViewModel(
                _fixture.DataService,
                ranks,
                _fixture.TempGtaDirectory,
                _fixture.ProfileName,
                settingsManager);

            // Assert - ViewModels should be initialized
            mainViewModel.RanksViewModel.Should().NotBeNull();
            mainViewModel.StationAssignmentsViewModel.Should().NotBeNull();
            mainViewModel.VehiclesViewModel.Should().NotBeNull();
            mainViewModel.OutfitsViewModel.Should().NotBeNull();
            mainViewModel.SettingsViewModel.Should().NotBeNull();
        }

        [Fact]
        public void MainWindowViewModel_CommandsAreInitialized()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            var settingsManager = new SettingsManager(
                Path.Combine(_fixture.TempGtaDirectory, "integration_test2.ini"));

            // Act
            var mainViewModel = new MainWindowViewModel(
                _fixture.DataService,
                ranks,
                _fixture.TempGtaDirectory,
                _fixture.ProfileName,
                settingsManager);

            // Assert - Commands should be created
            mainViewModel.GenerateCommand.Should().NotBeNull();
            mainViewModel.UndoCommand.Should().NotBeNull();
            mainViewModel.RedoCommand.Should().NotBeNull();
            mainViewModel.RestoreBackupCommand.Should().NotBeNull();
        }

        #endregion

        #region Validation Service Integration

        [Fact]
        public void ValidationService_CanBeCreatedWithDataLoadingService()
        {
            // Arrange & Act
            var validationService = new ValidationService(_fixture.DataService);

            // Assert
            validationService.Should().NotBeNull();
        }

        #endregion

        #region RanksViewModel Integration

        [Fact]
        public void RanksViewModel_InitializesWithRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            var mockService = new MockServiceBuilder().BuildMock();

            // Act
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Assert
            viewModel.Should().NotBeNull();
        }

        #endregion

        #region StationAssignmentsViewModel Integration

        [Fact]
        public void StationAssignmentsViewModel_InitializesWithDataService()
        {
            // Arrange & Act
            var viewModel = new StationAssignmentsViewModel(_fixture.DataService);

            // Assert
            viewModel.Should().NotBeNull();
        }

        #endregion

        #region VehiclesViewModel Integration

        [Fact]
        public void VehiclesViewModel_InitializesWithDataServiceAndRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new VehiclesViewModel(_fixture.DataService, ranks);

            // Assert
            viewModel.Should().NotBeNull();
        }

        #endregion

        #region OutfitsViewModel Integration

        [Fact]
        public void OutfitsViewModel_InitializesWithDataServiceAndRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new OutfitsViewModel(_fixture.DataService, ranks);

            // Assert
            viewModel.Should().NotBeNull();
        }

        #endregion

        #region SettingsViewModel Integration

        [Fact]
        public void SettingsViewModel_InitializesWithSettingsManager()
        {
            // Arrange
            var settingsManager = new SettingsManager(
                Path.Combine(_fixture.TempGtaDirectory, "settings_test.ini"));

            // Act
            var viewModel = new SettingsViewModel(settingsManager);

            // Assert
            viewModel.Should().NotBeNull();
        }

        #endregion

        #region BackupPathHelper Integration

        [Fact]
        public void BackupPathHelper_WorksWithSettingsManager()
        {
            // Arrange
            var settingsManager = new SettingsManager(
                Path.Combine(_fixture.TempGtaDirectory, "backup_integration_test.ini"));
            settingsManager.SetGtaVDirectory(_fixture.TempGtaDirectory);

            var backupRoot = Path.Combine(_fixture.TempGtaDirectory, "TestBackups");
            Directory.CreateDirectory(backupRoot);
            settingsManager.SetBackupDirectory(backupRoot);
            settingsManager.Save();

            // Act
            var backupDir = BackupPathHelper.GetBackupDirectory(settingsManager, _fixture.ProfileName);

            // Assert
            backupDir.Should().NotBeNullOrEmpty();
            backupDir.Should().Contain(_fixture.ProfileName);
        }

        #endregion

        #region Validation Dismissal Integration

        [Fact]
        public void ValidationDismissalService_CanBeCreatedWithSettingsManager()
        {
            // Arrange
            var settingsPath = Path.Combine(_fixture.TempGtaDirectory, "dismissal_integration.ini");
            var settingsManager = new SettingsManager(settingsPath);

            // Act
            var dismissalService = new ValidationDismissalService(settingsManager);

            // Assert
            dismissalService.Should().NotBeNull();
        }

        #endregion

        #region XML Generation and Parsing Round-Trip

        [Fact]
        public void XmlGenerationAndParsing_RoundTrip_PreservesBasicData()
        {
            // Arrange
            var originalRanks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder()
                    .WithName("Integration Test Rank")
                    .WithXP(750)
                    .WithSalary(3500)
                    .Build()
            };

            var tempFile = Path.Combine(_fixture.TempGtaDirectory, "roundtrip_integration_test.xml");

            // Act - Generate and parse
            var xml = RanksXmlGenerator.GenerateXml(originalRanks);
            File.WriteAllText(tempFile, xml);
            var parsedRanks = RanksParser.ParseRanksFile(tempFile);

            // Assert
            parsedRanks.Should().HaveCount(1);
            parsedRanks[0].Name.Should().Be("Integration Test Rank");
            parsedRanks[0].RequiredPoints.Should().Be(750);
            parsedRanks[0].Salary.Should().Be(3500);
        }

        #endregion
    }
}
