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
    /// Tests for VehiclesViewModel command execution - Remove, RemoveAll, Copy, Undo/Redo
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class VehiclesViewModelCommandTests
    {
        #region RemoveVehiclesCommand Tests

        [Fact]
        public void RemoveVehiclesCommand_CanExecute_WithNoRankSelected_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveVehiclesCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveVehiclesCommand_CanExecute_WithRankButNoVehicles_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            // Select rank
            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert - No vehicles to remove
            viewModel.RemoveVehiclesCommand.CanExecute(null).Should().BeFalse("no vehicles in rank");
        }

        #endregion

        #region RemoveAllVehiclesCommand Tests

        [Fact]
        public void RemoveAllVehiclesCommand_Execute_RemovesAllVehicles()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));
            officer.Vehicles.Add(new Vehicle("police2", "Police Interceptor", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act
            viewModel.RemoveAllVehiclesCommand.Execute(null);

            // Assert
            officer.Vehicles.Should().BeEmpty("all vehicles should be removed");
        }

        [Fact]
        public void RemoveAllVehiclesCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveAllVehiclesCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveAllVehiclesCommand_CanExecute_WithRankButNoVehicles_ReturnsFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert
            viewModel.RemoveAllVehiclesCommand.CanExecute(null).Should().BeFalse("no vehicles to remove");
        }

        [Fact]
        public void RemoveAllVehiclesCommand_CanExecute_WithVehicles_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Act & Assert
            viewModel.RemoveAllVehiclesCommand.CanExecute(null).Should().BeTrue("vehicles exist to remove");
        }

        #endregion

        #region CopyFromRankCommand Tests

        [Fact]
        public void CopyFromRankCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

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
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.SelectedCopyFromRank = viewModel.RankList[1];

            // Act & Assert
            viewModel.CopyFromRankCommand.CanExecute(null).Should().BeTrue("both ranks are selected and different");
        }

        #endregion

        #region CopyVehiclesCommand Tests

        [Fact]
        public void CopyVehiclesCommand_CanExecute_WithNoCopyFromRank_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyVehiclesCommand.CanExecute(null).Should().BeFalse("no copy from rank selected");
        }

        #endregion

        #region Undo/Redo Command Tests

        [Fact]
        public void UndoCommand_Execute_UndoesRemoveAllVehicles()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));
            officer.Vehicles.Add(new Vehicle("police2", "Police Interceptor", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Remove all vehicles
            viewModel.RemoveAllVehiclesCommand.Execute(null);
            officer.Vehicles.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Vehicles.Should().HaveCount(2, "undo should restore vehicles");
        }

        [Fact]
        public void RedoCommand_Execute_RedoesRemoveAllVehicles()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];

            // Remove all, then undo
            viewModel.RemoveAllVehiclesCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            officer.Vehicles.Should().HaveCount(1);

            // Act - Redo
            viewModel.RedoCommand.Execute(null);

            // Assert
            officer.Vehicles.Should().BeEmpty("redo should remove vehicles again");
        }

        [Fact]
        public void UndoCommand_CanExecute_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeFalse("no undo history");
        }

        [Fact]
        public void UndoCommand_CanExecute_AfterRemoveAll_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.RemoveAllVehiclesCommand.Execute(null);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeTrue("undo available after RemoveAll");
        }

        [Fact]
        public void RedoCommand_CanExecute_WithNoRedoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeFalse("no redo history");
        }

        [Fact]
        public void RedoCommand_CanExecute_AfterUndo_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

            var ranks = new List<RankHierarchy> { officer };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            viewModel.SelectedRank = viewModel.RankList[0];
            viewModel.RemoveAllVehiclesCommand.Execute(null);
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
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));

            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var ranks = new List<RankHierarchy> { officer, detective };
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            // Initially officer is selected (first rank)
            viewModel.RemoveAllVehiclesCommand.CanExecute(null).Should().BeTrue("officer has vehicles");

            // Act - Select detective (no vehicles)
            viewModel.SelectedRank = viewModel.RankList[1];

            // Assert
            viewModel.RemoveAllVehiclesCommand.CanExecute(null).Should().BeFalse("detective has no vehicles");
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
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

            // Assert
            viewModel.SelectedRank.Should().NotBeNull("first rank should be auto-selected");
            viewModel.SelectedRank!.Name.Should().Be("Officer");
        }

        [Fact]
        public void Constructor_WithNoRanks_HasNoSelection()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new VehiclesViewModel(mockService.Object, new List<RankHierarchy>());

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
            var viewModel = new VehiclesViewModel(mockService.Object, ranks);

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
            var viewModel = new VehiclesViewModel(mockService.Object, null);

            // Assert
            viewModel.RankList.Should().BeEmpty();
        }

        #endregion
    }
}
