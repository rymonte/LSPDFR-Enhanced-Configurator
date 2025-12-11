using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.IO;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class WelcomeWindowUITests
{
    private SettingsManager CreateTestSettingsManager()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_settings_{System.Guid.NewGuid()}.ini");
        return new SettingsManager(tempPath);
    }

    [AvaloniaFact]
    public void WelcomeWindow_Opens_Successfully()
    {
        // Arrange
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new WelcomeWindowViewModel(settingsManager);

        // Act
        var window = new WelcomeWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Should().NotBeNull();
        window.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void WelcomeWindow_HasCorrectTitle()
    {
        // Arrange
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new WelcomeWindowViewModel(settingsManager);

        // Act
        var window = new WelcomeWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Title.Should().Be("LSPDFR Enhanced Configurator");
    }

    [AvaloniaFact]
    public void WelcomeWindow_ViewModel_InitialState()
    {
        // Arrange
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new WelcomeWindowViewModel(settingsManager);

        // Act
        var window = new WelcomeWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.GtaDirectory.Should().BeEmpty();
        viewModel.ProceedCommand.CanExecute(null).Should().BeFalse("no valid directory selected");
    }

    [AvaloniaFact]
    public void WelcomeWindow_BrowseCommand_IsNotNull()
    {
        // Arrange
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new WelcomeWindowViewModel(settingsManager);

        // Act
        var window = new WelcomeWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.BrowseCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void WelcomeWindow_ProceedCommand_InitiallyDisabled()
    {
        // Arrange
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new WelcomeWindowViewModel(settingsManager);

        // Act
        var window = new WelcomeWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.ProceedCommand.Should().NotBeNull();
        viewModel.ProceedCommand.CanExecute(null).Should().BeFalse("no directory selected initially");
    }
}
