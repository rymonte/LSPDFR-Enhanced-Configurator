using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

using OutfitVariationBuilder = LSPDFREnhancedConfigurator.Tests.Builders.OutfitVariationBuilder;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for OutfitsViewModel command execution - Remove, RemoveAll, Copy, Undo/Redo
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class OutfitsViewModelCommandTests
    {
        #region RemoveOutfitsCommand Tests

        [Fact]
        public void RemoveOutfitsCommand_CanExecute_WithNoRankSelected_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveOutfitsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveOutfitsCommand_CanExecute_WithRankButNoOutfits_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            // Select rank
            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert - No outfits to remove
            viewModel.RemoveOutfitsCommand.CanExecute(null).Should().BeFalse("no outfits in rank");
        }

        #endregion

        #region RemoveAllOutfitsCommand Tests

        [Fact]
        public void RemoveAllOutfitsCommand_Execute_RemovesAllOutfits()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);
            officer.Outfits.Add(OutfitVariationBuilder.CreateFemaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act
            viewModel.RemoveAllOutfitsCommand.Execute(null);

            // Assert
            officer.Outfits.Should().BeEmpty("all outfits should be removed");
        }

        [Fact]
        public void RemoveAllOutfitsCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveAllOutfitsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveAllOutfitsCommand_CanExecute_WithRankButNoOutfits_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert
            viewModel.RemoveAllOutfitsCommand.CanExecute(null).Should().BeFalse("no outfits to remove");
        }

        [Fact]
        public void RemoveAllOutfitsCommand_CanExecute_WithOutfits_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert
            viewModel.RemoveAllOutfitsCommand.CanExecute(null).Should().BeTrue("outfits exist to remove");
        }

        #endregion

        #region CopyFromRankCommand Tests

        [Fact]
        public void CopyFromRankCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyFromRankCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void CopyFromRankCommand_CanExecute_WithBothRanksSelected_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.SelectedCopyFromRank = viewModel.RankList[1];

            // Act & Assert
            viewModel.CopyFromRankCommand.CanExecute(null).Should().BeTrue("both ranks are selected and different");
        }

        #endregion

        #region CopyOutfitsCommand Tests

        [Fact]
        public void CopyOutfitsCommand_CanExecute_WithNoCopyFromRank_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyOutfitsCommand.CanExecute(null).Should().BeFalse("no copy from rank selected");
        }

        #endregion

        #region Undo/Redo Command Tests

        [Fact]
        public void UndoCommand_Execute_UndoesRemoveAllOutfits()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);
            officer.Outfits.Add(OutfitVariationBuilder.CreateFemaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Remove all outfits
            viewModel.RemoveAllOutfitsCommand.Execute(null);
            officer.Outfits.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Outfits.Should().HaveCount(2, "undo should restore outfits");
        }

        [Fact]
        public void RedoCommand_Execute_RedoesRemoveAllOutfits()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Remove all, then undo
            viewModel.RemoveAllOutfitsCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            officer.Outfits.Should().HaveCount(1);

            // Act - Redo
            viewModel.RedoCommand.Execute(null);

            // Assert
            officer.Outfits.Should().BeEmpty("redo should remove outfits again");
        }

        [Fact]
        public void UndoCommand_CanExecute_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeFalse("no undo history");
        }

        [Fact]
        public void UndoCommand_CanExecute_AfterRemoveAll_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.RemoveAllOutfitsCommand.Execute(null);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeTrue("undo available after RemoveAll");
        }

        [Fact]
        public void RedoCommand_CanExecute_WithNoRedoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeFalse("no redo history");
        }

        [Fact]
        public void RedoCommand_CanExecute_AfterUndo_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.RemoveAllOutfitsCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeTrue("redo available after undo");
        }

        #endregion

        #region SelectedRank Tests

        [Fact]
        public void SelectedRank_Set_UpdatesCommandStates()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);

            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var ranks = new List<RankHierarchy> { officer, detective };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            // Initially officer is selected (first rank)
            viewModel.RemoveAllOutfitsCommand.CanExecute(null).Should().BeTrue("officer has outfits");

            // Act - Select detective (no outfits)
            viewModel.SelectedRank = viewModel.RankList[1];

            // Assert
            viewModel.RemoveAllOutfitsCommand.CanExecute(null).Should().BeFalse("detective has no outfits");
        }

        [Fact]
        public void Constructor_WithMultipleRanks_SelectsFirstRank()
        {
            // Arrange & Act
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            // Assert
            viewModel.SelectedRank.Should().NotBeNull("first rank should be auto-selected");
            viewModel.SelectedRank!.Name.Should().Be("Officer");
        }

        [Fact]
        public void Constructor_WithNoRanks_HasNoSelection()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, new List<RankHierarchy>());

            // Assert
            viewModel.SelectedRank.Should().BeNull("no ranks available");
        }

        #endregion

        #region RankList Population Tests

        [Fact]
        public void Constructor_PopulatesRankList()
        {
            // Arrange & Act
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build(),
                new RankHierarchyBuilder().WithName("Sergeant").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, ranks);

            // Assert
            viewModel.RankList.Should().HaveCount(3);
            viewModel.RankList[0].Name.Should().Be("Officer");
            viewModel.RankList[1].Name.Should().Be("Detective");
            viewModel.RankList[2].Name.Should().Be("Sergeant");
        }

        [Fact]
        public void Constructor_WithNullRanks_HasEmptyRankList()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new OutfitsViewModel(mockService.Object, null);

            // Assert
            viewModel.RankList.Should().BeEmpty();
        }

        #endregion
    }
}
