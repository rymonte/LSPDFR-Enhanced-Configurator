using System;
using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.StationAssignments;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class CopyStationAssignmentsCommandsTests
    {
        #region CopyStationAssignmentsFromRankCommand Tests (Additive)

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsFromRankCommand(null!, targetRank, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsFromRankCommand(sourceRank, null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsFromRankCommand(sourceRank, targetRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, null!));
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Constructor_SetsDescription()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act
            var command = new CopyStationAssignmentsFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy stations from 'Officer' to 'Detective'");
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Execute_CopiesStationAssignments()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));
            sourceRank.Stations.Add(new StationAssignment("Station2", new List<string> { "Zone2" }, 2));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(2);
            targetRank.Stations.Should().Contain(s => s.StationName == "Station1");
            targetRank.Stations.Should().Contain(s => s.StationName == "Station2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Execute_CreatesDeepCopies()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment("Station1", new List<string> { "Zone1" }, 1);
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(1);
            var copiedStation = targetRank.Stations[0];
            copiedStation.Should().NotBeSameAs(sourceStation); // Different object
            copiedStation.StationName.Should().Be(sourceStation.StationName);
            copiedStation.StyleID.Should().Be(sourceStation.StyleID);
            copiedStation.Zones.Should().Equal(sourceStation.Zones);
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Execute_DoesNotCopyDuplicateStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));
            sourceRank.Stations.Add(new StationAssignment("Station2", new List<string> { "Zone2" }, 2));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1)); // Already has Station1

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(2); // Only added Station2
            targetRank.Stations.Count(s => s.StationName == "Station1").Should().Be(1); // Only one Station1
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_Undo_RemovesOnlyAddedStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));
            sourceRank.Stations.Add(new StationAssignment("Station2", new List<string> { "Zone2" }, 2));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var existingStation = new StationAssignment("ExistingStation", new List<string> { "Zone0" }, 0);
            targetRank.Stations.Add(existingStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsFromRankCommand(
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
            targetRank.Stations.Should().HaveCount(1);
            targetRank.Stations.Should().Contain(existingStation);
            targetRank.Stations.Should().NotContain(s => s.StationName == "Station1");
            targetRank.Stations.Should().NotContain(s => s.StationName == "Station2");
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyStationAssignmentsFromRankCommand_MultipleExecute_ClearsTrackingBetweenExecutions()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Second execute

            // Assert
            targetRank.Stations.Should().HaveCount(1);
        }

        #endregion

        #region CopyStationAssignmentsToRankCommand Tests (Destructive)

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsToRankCommand(null!, targetRank, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsToRankCommand(sourceRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, null!));
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Constructor_SetsDescriptionWithSingularStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 1 station from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Constructor_SetsDescriptionWithPluralStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));
            sourceRank.Stations.Add(new StationAssignment("Station2", new List<string> { "Zone2" }, 2));

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 2 stations from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Execute_ClearsTargetStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Stations.Add(new StationAssignment("ExistingStation1", new List<string> { "Zone0" }, 0));
            targetRank.Stations.Add(new StationAssignment("ExistingStation2", new List<string> { "Zone0" }, 0));

            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(1);
            targetRank.Stations.Should().NotContain(s => s.StationName == "ExistingStation1");
            targetRank.Stations.Should().NotContain(s => s.StationName == "ExistingStation2");
            targetRank.Stations.Should().Contain(s => s.StationName == "Station1");
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Execute_CopiesAllSourceStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("Station1", new List<string> { "Zone1" }, 1));
            sourceRank.Stations.Add(new StationAssignment("Station2", new List<string> { "Zone2" }, 2));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(2);
            targetRank.Stations.Should().Contain(s => s.StationName == "Station1");
            targetRank.Stations.Should().Contain(s => s.StationName == "Station2");
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Execute_CreatesDeepCopies()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment("Station1", new List<string> { "Zone1" }, 1);
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Stations.Should().HaveCount(1);
            var copiedStation = targetRank.Stations[0];
            copiedStation.Should().NotBeSameAs(sourceStation); // Different object
            copiedStation.StationName.Should().Be(sourceStation.StationName);
            copiedStation.StyleID.Should().Be(sourceStation.StyleID);
            copiedStation.Zones.Should().Equal(sourceStation.Zones);
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_Undo_RestoresPreviousStations()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("NewStation", new List<string> { "Zone1" }, 1));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var previousStation = new StationAssignment("PreviousStation", new List<string> { "Zone0" }, 0);
            targetRank.Stations.Add(previousStation);

            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            command.Execute();

            // Reset flag
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            targetRank.Stations.Should().HaveCount(1);
            targetRank.Stations.Should().Contain(s => s.StationName == "PreviousStation");
            targetRank.Stations.Should().NotContain(s => s.StationName == "NewStation");
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyStationAssignmentsToRankCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Stations.Add(new StationAssignment("NewStation", new List<string> { "Zone1" }, 1));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Stations.Add(new StationAssignment("PreviousStation", new List<string> { "Zone0" }, 0));

            bool dataChangedCalled = false;

            var command = new CopyStationAssignmentsToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Redo

            // Assert
            targetRank.Stations.Should().HaveCount(1);
            targetRank.Stations.Should().Contain(s => s.StationName == "NewStation");
            targetRank.Stations.Should().NotContain(s => s.StationName == "PreviousStation");
        }

        #endregion
    }
}
