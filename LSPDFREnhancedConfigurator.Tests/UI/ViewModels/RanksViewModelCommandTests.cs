using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for RanksViewModel command execution - AddRank, AddPayBand, Remove, Move, Clone, etc.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class RanksViewModelCommandTests
    {
        #region AddRankCommand Tests

        [Fact]
        public void AddRankCommand_Execute_WithNoRanks_AddsFirstRank()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act
            viewModel.AddRankCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(1, "one rank should be added");
            viewModel.RankTreeItems[0].Rank.Name.Should().Be("New Rank 1");
            viewModel.RankTreeItems[0].Rank.RequiredPoints.Should().Be(0, "first rank starts at 0 XP");
            viewModel.RankTreeItems[0].Rank.Salary.Should().Be(30, "first rank has default salary");
        }

        [Fact]
        public void AddRankCommand_Execute_WithExistingRanks_AddsRankAfterSelected()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).WithSalary(30).Build(),
                new RankHierarchyBuilder().WithName("Detective").WithXP(500).WithSalary(50).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.AddRankCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(3, "new rank should be inserted");
            viewModel.RankTreeItems[1].Rank.Name.Should().StartWith("New Rank");
        }

        [Fact]
        public void AddRankCommand_Execute_SetsSmartDefaultXPAndSalary()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).WithSalary(30).Build(),
                new RankHierarchyBuilder().WithName("Detective").WithXP(1000).WithSalary(60).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank (Officer at XP 0, $30)
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.AddRankCommand.Execute(null);

            // Assert - New rank should have midpoint values between Officer (0, $30) and Detective (1000, $60)
            var newRank = viewModel.RankTreeItems[1].Rank;
            newRank.RequiredPoints.Should().Be(500, "XP should be midpoint between 0 and 1000");
            newRank.Salary.Should().Be(45, "salary should be midpoint between $30 and $60");
        }

        [Fact]
        public void AddRankCommand_Execute_AutoSelectsNewlyAddedRank()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act
            viewModel.AddRankCommand.Execute(null);

            // Assert
            viewModel.SelectedTreeItem.Should().NotBeNull("new rank should be auto-selected");
            viewModel.SelectedTreeItem!.Rank.Name.Should().Be("New Rank 1");
        }

        [Fact]
        public void AddRankCommand_CanExecute_AlwaysReturnsTrue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.AddRankCommand.CanExecute(null).Should().BeTrue("AddRank can always execute");
        }

        #endregion

        #region AddPayBandCommand Tests

        [Fact]
        public void AddPayBandCommand_Execute_AddsPayBandToParentRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).WithSalary(30).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select parent rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.AddPayBandCommand.Execute(null);

            // Assert
            var parentRank = viewModel.RankTreeItems[0].Rank;
            parentRank.PayBands.Should().HaveCount(1, "pay band should be added");
            parentRank.PayBands[0].Name.Should().Be("Officer I", "pay band should have roman numeral");
            parentRank.PayBands[0].Parent.Should().Be(parentRank, "pay band should reference parent");
        }

        [Fact]
        public void AddPayBandCommand_Execute_InheritsParentXPAndSalary()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Detective").WithXP(500).WithSalary(50).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.AddPayBandCommand.Execute(null);

            // Assert
            var payBand = viewModel.RankTreeItems[0].Rank.PayBands[0];
            payBand.RequiredPoints.Should().Be(500, "first pay band inherits parent XP");
            payBand.Salary.Should().Be(50, "first pay band inherits parent salary");
        }

        [Fact]
        public void AddPayBandCommand_Execute_AddsMultiplePayBands_WithCorrectRomanNumerals()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Sergeant").WithXP(1000).WithSalary(70).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act - Add multiple pay bands
            viewModel.AddPayBandCommand.Execute(null);
            viewModel.AddPayBandCommand.Execute(null);
            viewModel.AddPayBandCommand.Execute(null);

            // Assert
            var parentRank = viewModel.RankTreeItems[0].Rank;
            parentRank.PayBands.Should().HaveCount(3);
            parentRank.PayBands[0].Name.Should().Be("Sergeant I");
            parentRank.PayBands[1].Name.Should().Be("Sergeant II");
            parentRank.PayBands[2].Name.Should().Be("Sergeant III");
        }

        [Fact]
        public void AddPayBandCommand_CanExecute_WithSelectedRank_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.AddPayBandCommand.CanExecute(null).Should().BeTrue("pay band can be added to selected rank");
        }

        [Fact]
        public void AddPayBandCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.AddPayBandCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        #endregion

        #region RemoveCommand Tests

        [Fact]
        public void RemoveCommand_Execute_RemovesSelectedRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.RemoveCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(1, "one rank should remain");
            viewModel.RankTreeItems[0].Rank.Name.Should().Be("Detective", "Officer should be removed");
        }

        [Fact]
        public void RemoveCommand_Execute_RemovesPayBand()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build();
            officer.PayBands.Add(new RankHierarchy("Officer I", 0, 30) { Parent = officer });
            officer.PayBands.Add(new RankHierarchy("Officer II", 100, 35) { Parent = officer });

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Tree is populated automatically in constructor
            // Select first pay band (Officer I)
            var payBandTreeItem = viewModel.RankTreeItems[0].Children[0];
            viewModel.SelectedTreeItem = payBandTreeItem;

            // Act
            viewModel.RemoveCommand.Execute(null);

            // Assert
            officer.PayBands.Should().HaveCount(1, "one pay band should remain");
            officer.PayBands[0].Name.Should().Be("Officer I", "Officer II should be renumbered to Officer I");
        }

        [Fact]
        public void RemoveCommand_CanExecute_WithSelection_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.RemoveCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void RemoveCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.RemoveCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        #endregion

        #region MoveUpCommand Tests

        [Fact]
        public void MoveUpCommand_Execute_MovesRankUp()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build(),
                new RankHierarchyBuilder().WithName("Detective").WithXP(500).Build(),
                new RankHierarchyBuilder().WithName("Sergeant").WithXP(1000).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select second rank (Detective)
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[1];

            // Act
            viewModel.MoveUpCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems[0].Rank.Name.Should().Be("Detective", "Detective moved to first position");
            viewModel.RankTreeItems[1].Rank.Name.Should().Be("Officer", "Officer moved to second position");
        }

        [Fact]
        public void MoveUpCommand_CanExecute_WithFirstRank_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.MoveUpCommand.CanExecute(null).Should().BeFalse("first rank cannot move up");
        }

        [Fact]
        public void MoveUpCommand_CanExecute_WithSecondRank_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select second rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[1];

            // Act & Assert
            viewModel.MoveUpCommand.CanExecute(null).Should().BeTrue("second rank can move up");
        }

        #endregion

        #region MoveDownCommand Tests

        [Fact]
        public void MoveDownCommand_Execute_MovesRankDown()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build(),
                new RankHierarchyBuilder().WithName("Detective").WithXP(500).Build(),
                new RankHierarchyBuilder().WithName("Sergeant").WithXP(1000).Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank (Officer)
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.MoveDownCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems[0].Rank.Name.Should().Be("Detective", "Detective moved to first position");
            viewModel.RankTreeItems[1].Rank.Name.Should().Be("Officer", "Officer moved down to second position");
        }

        [Fact]
        public void MoveDownCommand_CanExecute_WithLastRank_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select last rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[1];

            // Act & Assert
            viewModel.MoveDownCommand.CanExecute(null).Should().BeFalse("last rank cannot move down");
        }

        [Fact]
        public void MoveDownCommand_CanExecute_WithFirstRank_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Select first rank
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.MoveDownCommand.CanExecute(null).Should().BeTrue("first rank can move down");
        }

        #endregion

        #region CloneCommand Tests

        [Fact]
        public void CloneCommand_Execute_CreatesExactCopyOfRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder()
                    .WithName("Officer")
                    .WithXP(100)
                    .WithSalary(35)
                    .Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.CloneCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(2, "cloned rank should be added");

            var clone = viewModel.RankTreeItems[1].Rank;
            clone.Name.Should().StartWith("Officer");
            clone.RequiredPoints.Should().Be(100, "XP should be cloned");
            clone.Salary.Should().Be(35, "salary should be cloned");
            clone.Id.Should().NotBe(ranks[0].Id, "clone should have different ID");
        }

        [Fact]
        public void CloneCommand_Execute_ClonesPayBands()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build();
            officer.PayBands.Add(new RankHierarchy("Officer I", 0, 30) { Parent = officer });
            officer.PayBands.Add(new RankHierarchy("Officer II", 100, 35) { Parent = officer });

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Tree is populated automatically in constructor
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act
            viewModel.CloneCommand.Execute(null);

            // Assert
            var clone = viewModel.RankTreeItems[1].Rank;
            clone.PayBands.Should().HaveCount(2, "pay bands should be cloned");
            clone.PayBands[0].RequiredPoints.Should().Be(0);
            clone.PayBands[1].RequiredPoints.Should().Be(100);
        }

        [Fact]
        public void CloneCommand_CanExecute_WithSelection_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.CloneCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CloneCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.CloneCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        #endregion

        #region RemoveAllRanksCommand Tests

        // Note: RemoveAllRanksCommand.Execute() shows an Avalonia confirmation dialog
        // which cannot be tested in unit tests. We only test CanExecute logic here.
        // Execute behavior is covered by integration/UI tests.

        [Fact]
        public void RemoveAllRanksCommand_CanExecute_WithRanks_ReturnsTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Act & Assert
            viewModel.RemoveAllRanksCommand.CanExecute(null).Should().BeTrue("ranks exist");
        }

        [Fact]
        public void RemoveAllRanksCommand_CanExecute_WithNoRanks_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.RemoveAllRanksCommand.CanExecute(null).Should().BeFalse("no ranks to remove");
        }

        #endregion

        #region PromoteCommand Tests

        [Fact]
        public void PromoteCommand_Execute_PromotesPayBandToParentRank()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build();
            officer.PayBands.Add(new RankHierarchy("Officer I", 0, 30) { Parent = officer });

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Tree is populated automatically in constructor
            // Select pay band
            var payBandTreeItem = viewModel.RankTreeItems[0].Children[0];
            viewModel.SelectedTreeItem = payBandTreeItem;

            // Act
            viewModel.PromoteCommand.Execute(null);

            // Assert
            // After promotion, the pay band becomes a parent rank
            // Original Officer should have 0 pay bands
            officer.PayBands.Should().BeEmpty("pay band was promoted");

            // New rank should exist at parent level
            viewModel.RankTreeItems.Should().HaveCountGreaterOrEqualTo(2, "promoted pay band becomes parent rank");
        }

        [Fact]
        public void PromoteCommand_CanExecute_WithPayBandSelected_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build();
            officer.PayBands.Add(new RankHierarchy("Officer I", 0, 30) { Parent = officer });

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            // Tree is populated automatically in constructor
            // Select pay band
            var payBandTreeItem = viewModel.RankTreeItems[0].Children[0];
            viewModel.SelectedTreeItem = payBandTreeItem;

            // Act & Assert
            viewModel.PromoteCommand.CanExecute(null).Should().BeTrue("pay band can be promoted");
        }

        [Fact]
        public void PromoteCommand_CanExecute_WithParentRankSelected_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(ranks, mockService.Object);

            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0];

            // Act & Assert
            viewModel.PromoteCommand.CanExecute(null).Should().BeFalse("parent rank cannot be promoted");
        }

        #endregion

        #region Undo/Redo Command Tests

        [Fact]
        public void UndoCommand_Execute_UndoesAddRank()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Add a rank
            viewModel.AddRankCommand.Execute(null);
            viewModel.RankTreeItems.Should().HaveCount(1);

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().BeEmpty("undo should remove the added rank");
        }

        [Fact]
        public void RedoCommand_Execute_RedoesAddRank()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Add rank, then undo
            viewModel.AddRankCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            viewModel.RankTreeItems.Should().BeEmpty();

            // Act - Redo
            viewModel.RedoCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(1, "redo should restore the rank");
        }

        [Fact]
        public void UndoCommand_CanExecute_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeFalse("no undo history");
        }

        [Fact]
        public void UndoCommand_CanExecute_AfterAddRank_ReturnsTrue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            viewModel.AddRankCommand.Execute(null);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeTrue("undo available after add");
        }

        [Fact]
        public void RedoCommand_CanExecute_WithNoRedoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeFalse("no redo history");
        }

        [Fact]
        public void RedoCommand_CanExecute_AfterUndo_ReturnsTrue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            viewModel.AddRankCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeTrue("redo available after undo");
        }

        [Fact]
        public void UndoRedo_MultipleOperations_WorksCorrectly()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new RanksViewModel(new List<RankHierarchy>(), mockService.Object);

            // Act - Perform sequence of operations
            viewModel.AddRankCommand.Execute(null); // Add rank 1
            viewModel.AddRankCommand.Execute(null); // Add rank 2
            viewModel.AddRankCommand.Execute(null); // Add rank 3

            viewModel.RankTreeItems.Should().HaveCount(3);

            // Undo twice
            viewModel.UndoCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);

            viewModel.RankTreeItems.Should().HaveCount(1, "two undos should leave one rank");

            // Redo once
            viewModel.RedoCommand.Execute(null);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(2, "redo should restore second rank");
        }

        #endregion
    }
}
