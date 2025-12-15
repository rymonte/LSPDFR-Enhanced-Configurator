using System;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.Vehicles;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class CopyVehiclesCommandsTests
    {
        #region CopyVehiclesFromRankCommand Tests (Additive)

        [Fact]
        public void CopyVehiclesFromRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesFromRankCommand(null!, targetRank, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesFromRankCommand(sourceRank, null!, () => refreshCalled = true, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesFromRankCommand(sourceRank, targetRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, null!));
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Constructor_SetsDescription()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act
            var command = new CopyVehiclesFromRankCommand(sourceRank, targetRank, () => refreshCalled = true, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy vehicles from 'Officer' to 'Detective'");
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_CopiesGlobalVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(2);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateSUV("lspd").Model);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_DoesNotCopyDuplicateGlobalVehicles()
        {
            // Arrange
            var patrol = VehicleBuilder.CreateLSPDPatrol();

            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(patrol);
            sourceRank.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // Already has patrol

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(2); // Only added SUV, not patrol
            targetRank.Vehicles.Count(v => v.Model == patrol.Model).Should().Be(1); // Only one patrol
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_CopiesStationSpecificVehiclesToMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1);
            targetStation.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_CopiesStationVehicleToGlobalWhenNoMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            // No matching station in target

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_DoesNotCopyDuplicateStationVehicles()
        {
            // Arrange
            var patrol = VehicleBuilder.CreateLSPDPatrol();

            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(patrol);
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol()); // Already has patrol
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1); // Should not duplicate
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Execute_IsCaseInsensitiveForStationNames()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "STATION1" }; // Different case
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1); // Should match despite case difference
            targetRank.Vehicles.Should().HaveCount(0); // Should not add to global
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Undo_RemovesOnlyAddedGlobalVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var existingVehicle = new Vehicle { Model = "existing" };
            targetRank.Vehicles.Add(existingVehicle);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
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
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().Contain(existingVehicle);
            targetRank.Vehicles.Should().NotContain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_Undo_RemovesOnlyAddedStationVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            var existingVehicle = new Vehicle { Model = "existing" };
            targetStation.Vehicles.Add(existingVehicle);
            targetRank.Stations.Add(targetStation);

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Act
            command.Undo();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1);
            targetStation.Vehicles.Should().Contain(existingVehicle);
            targetStation.Vehicles.Should().NotContain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
        }

        [Fact]
        public void CopyVehiclesFromRankCommand_MultipleExecute_ClearsTrackingBetweenExecutions()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool refreshCalled = false;
            bool dataChangedCalled = false;

            var command = new CopyVehiclesFromRankCommand(
                sourceRank,
                targetRank,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Second execute

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
        }

        #endregion

        #region CopyVehiclesToRankCommand Tests (Destructive)

        [Fact]
        public void CopyVehiclesToRankCommand_Constructor_ThrowsWhenSourceRankNull()
        {
            // Arrange
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesToRankCommand(null!, targetRank, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Constructor_ThrowsWhenTargetRankNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesToRankCommand(sourceRank, null!, () => dataChangedCalled = true));
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CopyVehiclesToRankCommand(sourceRank, targetRank, null!));
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Constructor_SetsDescriptionWithSingularVehicle()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 1 vehicle from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Constructor_SetsDescriptionWithPluralVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Officer").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var targetRank = new RankHierarchyBuilder().WithName("Detective").Build();
            bool dataChangedCalled = false;

            // Act
            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Assert
            command.Description.Should().Be("Copy 2 vehicles from 'Officer' to 'Detective' (overwrite)");
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Execute_ClearsTargetGlobalVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            targetRank.Vehicles.Add(new Vehicle { Model = "existing1" });
            targetRank.Vehicles.Add(new Vehicle { Model = "existing2" });

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().NotContain(v => v.Model == "existing1");
            targetRank.Vehicles.Should().NotContain(v => v.Model == "existing2");
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Execute_ClearsTargetStationVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetStation.Vehicles.Add(new Vehicle { Model = "existing" });
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Vehicles.Should().BeEmpty();
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Execute_CopiesAllSourceGlobalVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Vehicles.Add(VehicleBuilder.CreateSUV("lspd"));

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(2);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateSUV("lspd").Model);
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Execute_CopiesStationVehiclesToMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1);
            targetStation.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Execute_CopiesStationVehicleToGlobalWhenNoMatchingStation()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            var sourceStation = new StationAssignment { StationName = "Station1" };
            sourceStation.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());
            sourceRank.Stations.Add(sourceStation);

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            // No matching station

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Undo_RestoresPreviousGlobalVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var previousVehicle = new Vehicle { Model = "previous" };
            targetRank.Vehicles.Add(previousVehicle);

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            command.Execute();

            // Reset flag
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().Contain(previousVehicle);
            targetRank.Vehicles.Should().NotContain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CopyVehiclesToRankCommand_Undo_RestoresPreviousStationVehicles()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var targetStation = new StationAssignment { StationName = "Station1" };
            var previousVehicle = new Vehicle { Model = "previous" };
            targetStation.Vehicles.Add(previousVehicle);
            targetRank.Stations.Add(targetStation);

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            command.Execute();

            // Act
            command.Undo();

            // Assert
            targetStation.Vehicles.Should().HaveCount(1);
            targetStation.Vehicles.Should().Contain(previousVehicle);
        }

        [Fact]
        public void CopyVehiclesToRankCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var sourceRank = new RankHierarchyBuilder().WithName("Source").Build();
            sourceRank.Vehicles.Add(VehicleBuilder.CreateLSPDPatrol());

            var targetRank = new RankHierarchyBuilder().WithName("Target").Build();
            var previousVehicle = new Vehicle { Model = "previous" };
            targetRank.Vehicles.Add(previousVehicle);

            bool dataChangedCalled = false;

            var command = new CopyVehiclesToRankCommand(sourceRank, targetRank, () => dataChangedCalled = true);

            // Act
            command.Execute();
            command.Undo();
            command.Execute(); // Redo

            // Assert
            targetRank.Vehicles.Should().HaveCount(1);
            targetRank.Vehicles.Should().Contain(v => v.Model == VehicleBuilder.CreateLSPDPatrol().Model);
            targetRank.Vehicles.Should().NotContain(previousVehicle);
        }

        #endregion
    }
}
