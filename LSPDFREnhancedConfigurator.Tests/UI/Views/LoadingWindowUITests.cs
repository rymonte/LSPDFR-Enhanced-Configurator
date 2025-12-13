using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class LoadingWindowUITests
{
    [AvaloniaFact]
    public void LoadingWindow_Opens_Successfully()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();

        // Act
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        // Assert
        window.Should().NotBeNull();
        window.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void LoadingWindow_ViewModel_InitialState()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();

        // Act
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.StatusText.Should().Be("Loading...");
        viewModel.DetailText.Should().Be("Initializing...");
        viewModel.Progress.Should().Be(0);
        viewModel.IsIndeterminate.Should().BeTrue();
    }

    [AvaloniaFact]
    public void LoadingWindow_UpdateProgress_UpdatesViewModel()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        // Act
        viewModel.UpdateProgress("Downloading...", "Getting data", 50.0);

        // Assert
        viewModel.StatusText.Should().Be("Downloading...");
        viewModel.DetailText.Should().Be("Getting data");
        viewModel.Progress.Should().Be(50.0);
        viewModel.IsIndeterminate.Should().BeFalse();
    }

    [AvaloniaFact]
    public void LoadingWindow_SetIndeterminate_UpdatesViewModel()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        viewModel.UpdateProgress("Test", "Test detail", 75.0);

        // Act
        viewModel.SetIndeterminate("Processing...", "Please wait");

        // Assert
        viewModel.StatusText.Should().Be("Processing...");
        viewModel.DetailText.Should().Be("Please wait");
        viewModel.IsIndeterminate.Should().BeTrue();
    }

    [AvaloniaFact]
    public void LoadingWindow_ProgressProperty_CanBeSet()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        // Act
        viewModel.Progress = 33.33;

        // Assert
        viewModel.Progress.Should().Be(33.33);
    }

    [AvaloniaFact]
    public void LoadingWindow_StatusText_CanBeChanged()
    {
        // Arrange
        var viewModel = new LoadingWindowViewModel();
        var window = new LoadingWindow
        {
            DataContext = viewModel
        };

        // Act
        viewModel.StatusText = "Custom status";

        // Assert
        viewModel.StatusText.Should().Be("Custom status");
    }
}
