using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class VehiclesViewUITests
{
    [AvaloniaFact]
    public void VehiclesView_Creates_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new VehiclesViewModel(mockService.Object, ranks);

        // Act
        var view = new VehiclesView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        view.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void VehiclesView_WithRanks_BindsViewModel()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

        var ranks = new List<RankHierarchy> { officer };
        var viewModel = new VehiclesViewModel(mockService.Object, ranks);

        // Act
        var view = new VehiclesView
        {
            DataContext = viewModel
        };

        // Assert
        view.DataContext.Should().Be(viewModel);
        viewModel.RankList.Should().HaveCount(1);
    }

    [AvaloniaFact]
    public void VehiclesView_EmptyRanks_CreatesView()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new VehiclesViewModel(mockService.Object, ranks);

        // Act
        var view = new VehiclesView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        viewModel.RankList.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void VehiclesView_ViewModel_CommandsInitialized()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new VehiclesViewModel(mockService.Object, ranks);

        // Act
        var view = new VehiclesView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.RemoveAllVehiclesCommand.Should().NotBeNull();
        viewModel.CopyFromRankCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void VehiclesView_WithVehicleTree_BuildsCorrectly()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));
        officer.Vehicles.Add(new Vehicle("police2", "Police Interceptor", "LSPD"));

        var ranks = new List<RankHierarchy> { officer };
        var viewModel = new VehiclesViewModel(mockService.Object, ranks);

        // Act
        var view = new VehiclesView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.VehicleTreeItems.Should().NotBeEmpty();
    }
}
