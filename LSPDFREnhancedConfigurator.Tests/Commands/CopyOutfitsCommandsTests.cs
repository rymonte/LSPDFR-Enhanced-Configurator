using System;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.Outfits;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class CopyOutfitsCommandsTests
    {
        #region CopyOutfitsFromRankCommand Tests (Additive)

        [Fact]
        public void CopyOutfitsFromRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsFromRankCommand(null!, targetRank, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsFromRankCommand(sourceRank, null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsFromRankCommand(sourceRank, targetRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, null!));
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Constructor_SetsDescription()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act
            var command = new CopyOutfitsFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy outfits from 'Officer' to 'Detective'");
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_CopiesGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");
            sourceRank.Outfits.Add("Uniform2");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(2);
            targetRank.Outfits.Should().Contain("Uniform1");
            targetRank.Outfits.Should().Contain("Uniform2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_DoesNotCopyDuplicateGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");
            sourceRank.Outfits.Add("Uniform2");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("Uniform1"); // Already has Uniform1

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(2); // Only added Uniform2
            targetRank.Outfits.Count(o => o == "Uniform1").Should().Be(1); // Only one Uniform1
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_IsCaseInsensitiveForOutfitNames()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("uniform1");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("UNIFORM1"); // Different case

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(1); // Should not duplicate despite case difference
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_CopiesStationSpecificOutfitsToMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Outfits.Should().HaveCount(1);
            targetStation.Outfits.Should().Contain("StationUniform");
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_CopiesStationOutfitToGlobalWhenNoMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            // No matching station in target

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().Contain("StationUniform");
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_DoesNotCopyDuplicateStationOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Outfits.Add("StationUniform"); // Already has it
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Outfits.Should().HaveCount(1); // Should not duplicate
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Execute_IsCaseInsensitiveForStationNames()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "STATION1" }; // Different case
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Outfits.Should().HaveCount(1); // Should match despite case difference
            targetRank.Outfits.Should().HaveCount(0); // Should not add to global
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Undo_RemovesOnlyAddedGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");
            sourceRank.Outfits.Add("Uniform2");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("ExistingUniform");

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().Contain("ExistingUniform");
            targetRank.Outfits.Should().NotContain("Uniform1");
            targetRank.Outfits.Should().NotContain("Uniform2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_Undo_RemovesOnlyAddedStationOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("NewUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Outfits.Add("ExistingUniform");
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Act
            command.Undo();

            // Assert
            targetStation.Outfits.Should().HaveCount(1);
            targetStation.Outfits.Should().Contain("ExistingUniform");
            targetStation.Outfits.Should().NotContain("NewUniform");
        }

        [Fact]
        public void CopyOutfitsFromRankCommand_MultipleExecute_ClearsTrackingBetweenExecutions()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyOutfitsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Second execute

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
        }

        #endregion

        #region CopyOutfitsToRankCommand Tests (Destructive)

        [Fact]
        public void CopyOutfitsToRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsToRankCommand(null!, targetRank, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsToRankCommand(sourceRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyOutfitsToRankCommand(sourceRank, targetRank, null!));
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Constructor_SetsDescriptionWithSingularOutfit()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Outfits.Add("Uniform1");

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 1 outfit from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Constructor_SetsDescriptionWithPluralOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Outfits.Add("Uniform1");
            sourceRank.Outfits.Add("Uniform2");

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 2 outfits from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Execute_ClearsTargetGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("ExistingUniform1");
            targetRank.Outfits.Add("ExistingUniform2");

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().NotContain("ExistingUniform1");
            targetRank.Outfits.Should().NotContain("ExistingUniform2");
            targetRank.Outfits.Should().Contain("Uniform1");
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Execute_ClearsTargetStationOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Outfits.Add("ExistingUniform");
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Outfits.Should().BeEmpty();
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Execute_CopiesAllSourceGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("Uniform1");
            sourceRank.Outfits.Add("Uniform2");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(2);
            targetRank.Outfits.Should().Contain("Uniform1");
            targetRank.Outfits.Should().Contain("Uniform2");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Execute_CopiesStationOutfitsToMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Outfits.Should().HaveCount(1);
            targetStation.Outfits.Should().Contain("StationUniform");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Execute_CopiesStationOutfitToGlobalWhenNoMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Outfits.Add("StationUniform");
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            // No matching station

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().Contain("StationUniform");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Undo_RestoresPreviousGlobalOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("NewUniform");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("PreviousUniform");

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            command.Execute();

            // Reset flag
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().Contain("PreviousUniform");
            targetRank.Outfits.Should().NotContain("NewUniform");
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyOutfitsToRankCommand_Undo_RestoresPreviousStationOutfits()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Outfits.Add("PreviousUniform");
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            command.Execute();

            // Act
            command.Undo();

            // Assert
            targetStation.Outfits.Should().HaveCount(1);
            targetStation.Outfits.Should().Contain("PreviousUniform");
        }

        [Fact]
        public void CopyOutfitsToRankCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Outfits.Add("NewUniform");

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Outfits.Add("PreviousUniform");

            bool dataChangedCalled = false;

            var command = new CopyOutfitsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Redo

            // Assert
            targetRank.Outfits.Should().HaveCount(1);
            targetRank.Outfits.Should().Contain("NewUniform");
            targetRank.Outfits.Should().NotContain("PreviousUniform");
        }

        #endregion
    }
}
