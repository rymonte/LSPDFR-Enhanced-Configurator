using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class CopyFromRankDialogUITests
{
    [AvaloniaFact]
    public void CopyFromRankDialog_Opens_Successfully()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();
        viewModel.AvailableRanks.Add(new RankHierarchyBuilder().WithName("Officer").Build());
        viewModel.AvailableRanks.Add(new RankHierarchyBuilder().WithName("Detective").Build());

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        dialog.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void CopyFromRankDialog_HasDefaultTitle()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.Title.Should().Be("Copy From Rank");
    }

    [AvaloniaFact]
    public void CopyFromRankDialog_ViewModel_CanLoadRanks()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();
        viewModel.AvailableRanks.Add(new RankHierarchyBuilder().WithName("Officer").Build());
        viewModel.AvailableRanks.Add(new RankHierarchyBuilder().WithName("Detective").Build());
        viewModel.AvailableRanks.Add(new RankHierarchyBuilder().WithName("Sergeant").Build());

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.AvailableRanks.Should().HaveCount(3);
    }

    [AvaloniaFact]
    public void CopyFromRankDialog_InitialState_NoRankSelected()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.SelectedRank.Should().BeNull("no rank selected initially");
    }

    [AvaloniaFact]
    public void CopyFromRankDialog_WithEmptyRankList_CreatesDialog()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        viewModel.AvailableRanks.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void CopyFromRankDialog_Description_DisplaysCorrectly()
    {
        // Arrange
        var viewModel = new CopyFromRankDialogViewModel();

        // Act
        var dialog = new CopyFromRankDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.Description.Should().Be("Select a rank to copy data from");
    }
}
