using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class StationAssignmentsViewUITests
{
    [AvaloniaFact]
    public void StationAssignmentsView_Creates_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new StationAssignmentsViewModel(mockService.Object);

        // Act
        var view = new StationAssignmentsView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        view.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void StationAssignmentsView_WithRanks_BindsViewModel()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new StationAssignmentsViewModel(mockService.Object);
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        var ranks = new List<RankHierarchy> { officer };

        viewModel.LoadRanks(ranks);

        // Act
        var view = new StationAssignmentsView
        {
            DataContext = viewModel
        };

        // Assert
        view.DataContext.Should().Be(viewModel);
        viewModel.RankList.Should().HaveCount(1);
    }

    [AvaloniaFact]
    public void StationAssignmentsView_EmptyRanks_CreatesView()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new StationAssignmentsViewModel(mockService.Object);
        viewModel.LoadRanks(new List<RankHierarchy>());

        // Act
        var view = new StationAssignmentsView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        viewModel.RankList.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void StationAssignmentsView_ViewModel_CommandsInitialized()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var viewModel = new StationAssignmentsViewModel(mockService.Object);

        // Act
        var view = new StationAssignmentsView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.AddStationsCommand.Should().NotBeNull();
        viewModel.RemoveStationsCommand.Should().NotBeNull();
        viewModel.CopyFromRankCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void StationAssignmentsView_WithStations_LoadsCorrectly()
    {
        // Arrange
        var mockService = new MockServiceBuilder().WithDefaultStations().BuildMock();
        var viewModel = new StationAssignmentsViewModel(mockService.Object);
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

        var ranks = new List<RankHierarchy> { officer };
        viewModel.LoadRanks(ranks);
        viewModel.SelectedRank = officer; // This triggers FilterAvailableStations()

        // Act
        var view = new StationAssignmentsView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.AvailableStations.Should().NotBeEmpty();
    }
}
