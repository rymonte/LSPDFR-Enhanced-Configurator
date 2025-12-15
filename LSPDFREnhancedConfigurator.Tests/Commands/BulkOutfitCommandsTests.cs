using System;
using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.Outfits;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class BulkOutfitCommandsTests
    {
        #region BulkAddOutfitsCommand Tests

        [Fact]
        public void BulkAddOutfitsCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var outfits = new List<string> { "Uniform1" };
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkAddOutfitsCommand(null!, outfits, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkAddOutfitsCommand_Constructor_ThrowsWhenOutfitsNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkAddOutfitsCommand(rank, null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkAddOutfitsCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfits = new List<string> { "Uniform1" };
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkAddOutfitsCommand(rank, outfits, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkAddOutfitsCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfits = new List<string> { "Uniform1" };
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkAddOutfitsCommand(rank, outfits, () => refreshCalled = true, null!));
        }

        [Fact]
        public void BulkAddOutfitsCommand_Execute_AddsOutfitsToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfitsToAdd = new List<string> { "Uniform1", "Uniform2" };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkAddOutfitsCommand(
                rank,
                outfitsToAdd,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Outfits.Should().Contain("Uniform1");
            rank.Outfits.Should().Contain("Uniform2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkAddOutfitsCommand_Undo_RemovesOutfitsFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfitsToAdd = new List<string> { "Uniform1", "Uniform2" };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkAddOutfitsCommand(
                rank,
                outfitsToAdd,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            rank.Outfits.Should().NotContain("Uniform1");
            rank.Outfits.Should().NotContain("Uniform2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkAddOutfitsCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfitsToAdd = new List<string> { "Uniform1", "Uniform2" };

            // Act
            var command = new BulkAddOutfitsCommand(
                rank,
                outfitsToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 outfits");
            command.Description.Should().Contain("Officer");
        }

        [Fact]
        public void BulkAddOutfitsCommand_Description_SingleOutfit_UsesCorrectGrammar()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfitsToAdd = new List<string> { "Uniform1" };

            // Act
            var command = new BulkAddOutfitsCommand(
                rank,
                outfitsToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("1 outfit");
            command.Description.Should().NotContain("outfits");
        }

        [Fact]
        public void BulkAddOutfitsCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfitsToAdd = new List<string> { "Uniform1" };

            var command = new BulkAddOutfitsCommand(
                rank,
                outfitsToAdd,
                () => { },
                () => { });

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Redo

            // Assert
            rank.Outfits.Should().Contain("Uniform1");
        }

        #endregion

        #region BulkRemoveOutfitsCommand Tests

        [Fact]
        public void BulkRemoveOutfitsCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var outfits = new List<string> { "Uniform1" };
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkRemoveOutfitsCommand(null!, outfits, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Constructor_ThrowsWhenOutfitsNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkRemoveOutfitsCommand(rank, null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfits = new List<string> { "Uniform1" };
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkRemoveOutfitsCommand(rank, outfits, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var outfits = new List<string> { "Uniform1" };
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BulkRemoveOutfitsCommand(rank, outfits, () => refreshCalled = true, null!));
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Execute_RemovesOutfitsFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            rank.Outfits.Add("Uniform1");
            rank.Outfits.Add("Uniform2");
            rank.Outfits.Add("Uniform3");

            var outfitsToRemove = new List<string> { "Uniform1", "Uniform2" };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkRemoveOutfitsCommand(
                rank,
                outfitsToRemove,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Outfits.Should().NotContain("Uniform1");
            rank.Outfits.Should().NotContain("Uniform2");
            rank.Outfits.Should().Contain("Uniform3"); // Should still be there
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Undo_RestoresOutfitsToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            rank.Outfits.Add("Uniform1");
            rank.Outfits.Add("Uniform2");

            var outfitsToRemove = new List<string> { "Uniform1", "Uniform2" };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkRemoveOutfitsCommand(
                rank,
                outfitsToRemove,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            rank.Outfits.Should().Contain("Uniform1");
            rank.Outfits.Should().Contain("Uniform2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            rank.Outfits.Add("Uniform1");
            rank.Outfits.Add("Uniform2");

            var outfitsToRemove = new List<string> { "Uniform1", "Uniform2" };

            // Act
            var command = new BulkRemoveOutfitsCommand(
                rank,
                outfitsToRemove,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 outfits");
            command.Description.Should().Contain("Officer");
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_Description_SingleOutfit_UsesCorrectGrammar()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            rank.Outfits.Add("Uniform1");

            var outfitsToRemove = new List<string> { "Uniform1" };

            // Act
            var command = new BulkRemoveOutfitsCommand(
                rank,
                outfitsToRemove,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("1 outfit");
            command.Description.Should().NotContain("outfits");
        }

        [Fact]
        public void BulkRemoveOutfitsCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            rank.Outfits.Add("Uniform1");

            var outfitsToRemove = new List<string> { "Uniform1" };

            var command = new BulkRemoveOutfitsCommand(
                rank,
                outfitsToRemove,
                () => { },
                () => { });

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Redo

            // Assert
            rank.Outfits.Should().NotContain("Uniform1");
        }

        #endregion
    }
}
