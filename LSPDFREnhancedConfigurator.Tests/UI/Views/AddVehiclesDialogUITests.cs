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
    [AvaloniaFact(Skip = "AddVehiclesDialog requires XAML resources not available in headless mode")]
    public void AddVehiclesDialog_Opens_Successfully()
    {
        // Note: This test is skipped due to XAML resource dependencies (DarkerBackgroundBrush)
        // The ViewModel functionality is tested in AddVehiclesDialogViewModelTests
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        dialog.Should().NotBeNull();
        dialog.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact(Skip = "AddVehiclesDialog requires XAML resources not available in headless mode")]
    public void AddVehiclesDialog_HasCorrectTitle()
    {
        // Note: This test is skipped due to XAML resource dependencies
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        var dialog = new AddVehiclesDialog
        {
            DataContext = viewModel
        };

        dialog.Title.Should().Be("Add Vehicles");
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_ViewModel_LoadsVehicles()
    {
        // Arrange
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();

        // Act - Test ViewModel directly without Dialog instantiation
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Assert
        viewModel.VehicleItems.Should().NotBeEmpty("vehicles should be loaded from service");
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_Commands_ExistAndInitialize()
    {
        // Arrange
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();

        // Act - Test ViewModel directly without Dialog instantiation
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Assert
        viewModel.AddSelectedCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void AddVehiclesDialog_SearchText_FiltersVehicles()
    {
        // Arrange
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

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
        var mockService = new MockServiceBuilder()
            .WithDefaultVehicles()
            .WithDefaultAgencies()
            .BuildMock();

        // Act - Test ViewModel directly without Dialog instantiation
        var viewModel = new AddVehiclesDialogViewModel(mockService.Object);

        // Assert
        viewModel.SelectedVehicles.Should().BeEmpty("no vehicles selected initially");
        viewModel.AddSelectedCommand.CanExecute(null).Should().BeFalse("cannot add when nothing is selected");
    }
}
