using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class RanksViewUITests
{
    [AvaloniaFact]
    public void RanksView_Creates_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new RanksViewModel(ranks, mockService.Object);

        // Act
        var view = new RanksView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        view.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void RanksView_WithRanks_BindsViewModel()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>
        {
            new RankHierarchyBuilder().WithName("Officer").Build(),
            new RankHierarchyBuilder().WithName("Detective").Build()
        };
        var viewModel = new RanksViewModel(ranks, mockService.Object);

        // Act
        var view = new RanksView
        {
            DataContext = viewModel
        };

        // Assert
        view.DataContext.Should().Be(viewModel);
        viewModel.RankTreeItems.Should().HaveCount(2);
    }

    [AvaloniaFact]
    public void RanksView_EmptyRanks_CreatesView()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new RanksViewModel(ranks, mockService.Object);

        // Act
        var view = new RanksView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        viewModel.RankTreeItems.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void RanksView_WithPayBands_CreatesHierarchy()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var officer = new RankHierarchyBuilder()
            .WithName("Officer")
            .Build();
        officer.PayBands.Add(new RankHierarchyBuilder().WithName("Officer I").Build());
        officer.PayBands.Add(new RankHierarchyBuilder().WithName("Officer II").Build());

        var ranks = new List<RankHierarchy> { officer };
        var viewModel = new RanksViewModel(ranks, mockService.Object);

        // Act
        var view = new RanksView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.RankTreeItems.Should().HaveCount(1);
        viewModel.RankTreeItems[0].Children.Should().HaveCount(2);
    }

    [AvaloniaFact]
    public void RanksView_ViewModel_CommandsInitialized()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new RanksViewModel(ranks, mockService.Object);

        // Act
        var view = new RanksView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.AddRankCommand.Should().NotBeNull();
        viewModel.AddPayBandCommand.Should().NotBeNull();
        viewModel.RemoveCommand.Should().NotBeNull();
    }
}
