using System;
using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.Ranks;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class RemoveAllRanksCommandTests
    {
        [Fact]
        public void RemoveAllRanksCommand_Constructor_ThrowsWhenRanksNull()
        {
            // Arrange
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveAllRanksCommand(null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void RemoveAllRanksCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveAllRanksCommand(ranks, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void RemoveAllRanksCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveAllRanksCommand(ranks, () => refreshCalled = true, null!));
        }

        [Fact]
        public void RemoveAllRanksCommand_Constructor_BackupsPreviousRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var command = new RemoveAllRanksCommand(ranks, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("2 ranks");
        }

        [Fact]
        public void RemoveAllRanksCommand_Description_SingularForm_ForSingleRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var command = new RemoveAllRanksCommand(ranks, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("1 rank");
            command.Description.Should().NotContain("ranks");
        }

        [Fact]
        public void RemoveAllRanksCommand_Description_PluralForm_ForMultipleRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var command = new RemoveAllRanksCommand(ranks, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("2 ranks");
        }

        [Fact]
        public void RemoveAllRanksCommand_Execute_ClearsAllRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build(),
                new RankHierarchyBuilder().WithName("Sergeant").Build()
            };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveAllRanksCommand(
                ranks,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            ranks.Should().BeEmpty();
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void RemoveAllRanksCommand_Execute_OnEmptyList_DoesNotThrow()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveAllRanksCommand(
                ranks,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            ranks.Should().BeEmpty();
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void RemoveAllRanksCommand_Undo_RestoresAllRanks()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var rank3 = new RankHierarchyBuilder().WithName("Sergeant").Build();

            var ranks = new List<RankHierarchy> { rank1, rank2, rank3 };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveAllRanksCommand(
                ranks,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            ranks.Should().HaveCount(3);
            ranks.Should().Contain(rank1);
            ranks.Should().Contain(rank2);
            ranks.Should().Contain(rank3);
            ranks[0].Should().Be(rank1); // Order preserved
            ranks[1].Should().Be(rank2);
            ranks[2].Should().Be(rank3);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void RemoveAllRanksCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            var command = new RemoveAllRanksCommand(ranks, () => { }, () => { });

            // Act
            command.Execute();
            ranks.Should().BeEmpty();

            command.Undo();
            ranks.Should().HaveCount(2);

            command.Execute(); // Redo
            ranks.Should().BeEmpty();

            command.Undo();
            ranks.Should().HaveCount(2);
        }

        [Fact]
        public void RemoveAllRanksCommand_Undo_PreservesRankProperties()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .Build();
            rank.RequiredPoints = 100;
            rank.Salary = 5000;
            rank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            rank.Outfits.Add("Uniform1");

            var ranks = new List<RankHierarchy> { rank };

            var command = new RemoveAllRanksCommand(ranks, () => { }, () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            ranks.Should().HaveCount(1);
            var restoredRank = ranks[0];
            restoredRank.Name.Should().Be("Officer");
            restoredRank.RequiredPoints.Should().Be(100);
            restoredRank.Salary.Should().Be(5000);
            restoredRank.Vehicles.Should().HaveCount(1);
            restoredRank.Outfits.Should().Contain("Uniform1");
        }
    }
}
