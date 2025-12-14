using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.Vehicles;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands.Vehicles
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class BulkVehicleCommandsTests
    {
        #region BulkAddVehiclesCommand Tests

        [Fact]
        public void BulkAddVehiclesCommand_Execute_AddsVehiclesToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehicle1 = new Vehicle("police", "Police Cruiser", "LSPD");
            var vehicle2 = new Vehicle("police2", "Police Interceptor", "LSPD");
            var vehiclesToAdd = new List<Vehicle> { vehicle1, vehicle2 };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkAddVehiclesCommand(
                rank,
                vehiclesToAdd,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Vehicles.Should().Contain(vehicle1);
            rank.Vehicles.Should().Contain(vehicle2);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkAddVehiclesCommand_Undo_RemovesVehiclesFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehicle1 = new Vehicle("police", "Police Cruiser", "LSPD");
            var vehicle2 = new Vehicle("police2", "Police Interceptor", "LSPD");
            var vehiclesToAdd = new List<Vehicle> { vehicle1, vehicle2 };

            var command = new BulkAddVehiclesCommand(
                rank,
                vehiclesToAdd,
                () => { },
                () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            rank.Vehicles.Should().NotContain(vehicle1);
            rank.Vehicles.Should().NotContain(vehicle2);
        }

        [Fact]
        public void BulkAddVehiclesCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehiclesToAdd = new List<Vehicle>
            {
                new Vehicle("police", "Police Cruiser", "LSPD"),
                new Vehicle("police2", "Police Interceptor", "LSPD")
            };

            // Act
            var command = new BulkAddVehiclesCommand(
                rank,
                vehiclesToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 vehicles");
            command.Description.Should().Contain("Officer");
        }

        [Fact]
        public void BulkAddVehiclesCommand_Description_SingleVehicle_UsesCorrectGrammar()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehiclesToAdd = new List<Vehicle>
            {
                new Vehicle("police", "Police Cruiser", "LSPD")
            };

            // Act
            var command = new BulkAddVehiclesCommand(
                rank,
                vehiclesToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("1 vehicle");
            command.Description.Should().NotContain("vehicles");
        }

        #endregion

        #region BulkRemoveVehiclesCommand Tests

        [Fact]
        public void BulkRemoveVehiclesCommand_Execute_RemovesVehiclesFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehicle1 = new Vehicle("police", "Police Cruiser", "LSPD");
            var vehicle2 = new Vehicle("police2", "Police Interceptor", "LSPD");
            rank.Vehicles.Add(vehicle1);
            rank.Vehicles.Add(vehicle2);

            var vehiclesToRemove = new List<Vehicle> { vehicle1, vehicle2 };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkRemoveVehiclesCommand(
                rank,
                vehiclesToRemove,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Vehicles.Should().NotContain(vehicle1);
            rank.Vehicles.Should().NotContain(vehicle2);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkRemoveVehiclesCommand_Undo_RestoresVehiclesToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehicle1 = new Vehicle("police", "Police Cruiser", "LSPD");
            var vehicle2 = new Vehicle("police2", "Police Interceptor", "LSPD");
            rank.Vehicles.Add(vehicle1);
            rank.Vehicles.Add(vehicle2);

            var vehiclesToRemove = new List<Vehicle> { vehicle1, vehicle2 };

            var command = new BulkRemoveVehiclesCommand(
                rank,
                vehiclesToRemove,
                () => { },
                () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            rank.Vehicles.Should().Contain(vehicle1);
            rank.Vehicles.Should().Contain(vehicle2);
        }

        [Fact]
        public void BulkRemoveVehiclesCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var vehiclesToRemove = new List<Vehicle>
            {
                new Vehicle("police", "Police Cruiser", "LSPD"),
                new Vehicle("police2", "Police Interceptor", "LSPD")
            };

            // Act
            var command = new BulkRemoveVehiclesCommand(
                rank,
                vehiclesToRemove,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 vehicles");
            command.Description.Should().Contain("Officer");
        }

        #endregion
    }
}
