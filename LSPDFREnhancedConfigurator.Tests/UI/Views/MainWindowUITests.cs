using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class MainWindowUITests
{
    private SettingsManager CreateTestSettingsManager()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_settings_{System.Guid.NewGuid()}.ini");
        return new SettingsManager(tempPath);
    }

    [AvaloniaFact]
    public void MainWindow_Opens_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Should().NotBeNull();
        window.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void MainWindow_HasCorrectTitle()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Title.Should().Be("LSPDFR Enhanced Configurator");
    }

    [AvaloniaFact]
    public void MainWindow_ViewModel_InitializesChildViewModels()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>
        {
            new RankHierarchyBuilder().WithName("Officer").Build()
        };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.RanksViewModel.Should().NotBeNull();
        viewModel.StationAssignmentsViewModel.Should().NotBeNull();
        viewModel.VehiclesViewModel.Should().NotBeNull();
        viewModel.OutfitsViewModel.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void MainWindow_WithEmptyRanks_CreatesWindow()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Should().NotBeNull();
        viewModel.RanksViewModel.RankTreeItems.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void MainWindow_WithMultipleRanks_LoadsAllRanks()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>
        {
            new RankHierarchyBuilder().WithName("Officer").Build(),
            new RankHierarchyBuilder().WithName("Detective").Build(),
            new RankHierarchyBuilder().WithName("Sergeant").Build()
        };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.RanksViewModel.RankTreeItems.Should().HaveCount(3);
    }

    [AvaloniaFact]
    public void MainWindow_GenerateCommand_InitiallyEnabled()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>
        {
            new RankHierarchyBuilder().WithName("Officer").Build()
        };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.GenerateCommand.Should().NotBeNull();
        viewModel.GenerateCommand.CanExecute(null).Should().BeTrue("should be able to generate with ranks present");
    }

    [AvaloniaFact]
    public void MainWindow_UndoRedoCommands_ExistAndDelegate()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.UndoCommand.Should().NotBeNull();
        viewModel.RedoCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void MainWindow_ToggleXmlPreviewCommand_Exists()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.ToggleXmlPreviewCommand.Should().NotBeNull();
        viewModel.ToggleXmlPreviewCommand.CanExecute(null).Should().BeTrue();
    }

    [AvaloniaFact]
    public void MainWindow_ValidationProperties_InitializeCorrectly()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.ValidationErrorCount.Should().Be(0);
        viewModel.ValidationErrorItems.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void MainWindow_IsLoading_InitiallyFalse()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new MainWindowViewModel(mockService.Object, ranks, @"C:\TestGTA", "Default", settingsManager);

        // Act
        var window = new MainWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.IsLoading.Should().BeFalse("should not be loading initially");
    }
}
