using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class ProfileSelectionDialogUITests
{
    private SettingsManager CreateTestSettingsManager()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_settings_{System.Guid.NewGuid()}.ini");
        return new SettingsManager(tempPath);
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_Opens_Successfully()
    {
        // Arrange
        var profiles = new List<string> { "Default", "Custom", "Test" };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);

        // Act
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        dialog.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_ViewModel_LoadsProfiles()
    {
        // Arrange
        var profiles = new List<string> { "Default", "Custom", "Test" };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);

        // Act
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.Profiles.Should().HaveCount(3);
        viewModel.Profiles.Should().Contain("Default");
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_ViewModel_SortsProfiles()
    {
        // Arrange
        var profiles = new List<string> { "Zebra", "Alpha", "Beta" };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);

        // Act
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.Profiles[0].Should().Be("Alpha");
        viewModel.Profiles[1].Should().Be("Beta");
        viewModel.Profiles[2].Should().Be("Zebra");
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_SelectCommand_InitiallyDisabled()
    {
        // Arrange
        var profiles = new List<string> { "Default" };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);

        // Act
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Clear any default selection
        viewModel.SelectedProfile = null;

        // Assert
        viewModel.SelectCommand.Should().NotBeNull();
        viewModel.SelectCommand.CanExecute(null).Should().BeFalse("no profile selected initially");
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_SelectCommand_EnabledWhenProfileSelected()
    {
        // Arrange
        var profiles = new List<string> { "Default", "Custom" };
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Act
        viewModel.SelectedProfile = "Default";

        // Assert
        viewModel.SelectCommand.CanExecute(null).Should().BeTrue("profile is selected");
    }

    [AvaloniaFact]
    public void ProfileSelectionDialog_WithEmptyProfileList_CreatesDialog()
    {
        // Arrange
        var profiles = new List<string>();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new ProfileSelectionDialogViewModel(profiles, settingsManager);

        // Act
        var dialog = new ProfileSelectionDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        viewModel.Profiles.Should().BeEmpty();
    }
}
