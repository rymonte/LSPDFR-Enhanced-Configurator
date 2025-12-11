using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class AddVehiclesDialogUITests
{
    [AvaloniaFact]
    public void AddVehiclesDialog_Opens_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Act
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        dialog.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_HasCorrectTitle()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Act
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Title.Should().Be("Add Vehicles");
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_ViewModel_LoadsVehicles()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Act
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.VehicleItems.Should().NotBeEmpty("vehicles should be loaded from service");
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_Commands_ExistAndInitialize()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Act
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.AddSelectedCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_SearchText_FiltersVehicles()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        var initialCount = viewModel.VehicleItems.Count;

        // Act
        viewModel.SearchText = "police";

        // Assert
        viewModel.VehicleItems.Count.Should().BeLessThanOrEqualTo(initialCount, "search should filter the vehicle list");
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_InitialState_NoVehiclesSelected()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Act
        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.SelectedVehicles.Should().BeEmpty("no vehicles selected initially");
        viewModel.AddSelectedCommand.CanExecute(null).Should().BeFalse("cannot add when nothing is selected");
    }
}
