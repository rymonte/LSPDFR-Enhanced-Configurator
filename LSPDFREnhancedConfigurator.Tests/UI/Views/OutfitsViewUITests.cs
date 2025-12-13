using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class OutfitsViewUITests
{
    [AvaloniaFact]
    public void OutfitsView_Creates_Successfully()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new OutfitsViewModel(mockService.Object, ranks);

        // Act
        var view = new OutfitsView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        view.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void OutfitsView_WithRanks_BindsViewModel()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

        var ranks = new List<RankHierarchy> { officer };
        var viewModel = new OutfitsViewModel(mockService.Object, ranks);

        // Act
        var view = new OutfitsView
        {
            DataContext = viewModel
        };

        // Assert
        view.DataContext.Should().Be(viewModel);
        viewModel.RankList.Should().HaveCount(1);
    }

    [AvaloniaFact]
    public void OutfitsView_EmptyRanks_CreatesView()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new OutfitsViewModel(mockService.Object, ranks);

        // Act
        var view = new OutfitsView
        {
            DataContext = viewModel
        };

        // Assert
        view.Should().NotBeNull();
        viewModel.RankList.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void OutfitsView_ViewModel_CommandsInitialized()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var ranks = new List<RankHierarchy>();
        var viewModel = new OutfitsViewModel(mockService.Object, ranks);

        // Act
        var view = new OutfitsView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.RemoveAllOutfitsCommand.Should().NotBeNull();
        viewModel.CopyFromRankCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void OutfitsView_WithOutfitTree_BuildsCorrectly()
    {
        // Arrange
        var mockService = new MockServiceBuilder().BuildMock();
        var officer = new RankHierarchyBuilder().WithName("Officer").Build();
        officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);
        officer.Outfits.Add(OutfitVariationBuilder.CreateFemaleVariation().CombinedName);

        var ranks = new List<RankHierarchy> { officer };
        var viewModel = new OutfitsViewModel(mockService.Object, ranks);

        // Act
        var view = new OutfitsView
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.OutfitTreeItems.Should().NotBeEmpty();
    }
}
