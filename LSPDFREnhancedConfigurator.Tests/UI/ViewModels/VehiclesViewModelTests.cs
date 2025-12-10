using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for VehiclesViewModel
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class VehiclesViewModelTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;

        public VehiclesViewModelTests()
        {
            _mockDataService = new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultStations()
                .WithDefaultVehicles()
                .BuildMock();
        }

        #region Initialization Tests

        [Fact]
        public void Constructor_WithNullRanks_InitializesSuccessfully()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.RankList.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithEmptyRanks_InitializesSuccessfully()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.RankList.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithRanks_PopulatesRankList()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Assert
            viewModel.RankList.Should().HaveCount(2);
            viewModel.RankList[0].Name.Should().Be("Officer");
            viewModel.RankList[1].Name.Should().Be("Detective");
        }

        [Fact]
        public void Constructor_WithRanks_SelectsFirstRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Assert
            viewModel.SelectedRank.Should().NotBeNull();
            viewModel.SelectedRank.Name.Should().Be("Officer");
        }

        [Fact]
        public void Constructor_InitializesCollections()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.RankList.Should().NotBeNull();
            viewModel.CopyFromRankList.Should().NotBeNull();
            viewModel.CopyToRankList.Should().NotBeNull();
            viewModel.VehicleTreeItems.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.AddVehiclesCommand.Should().NotBeNull();
            viewModel.RemoveVehiclesCommand.Should().NotBeNull();
            viewModel.RemoveAllVehiclesCommand.Should().NotBeNull();
            viewModel.CopyFromRankCommand.Should().NotBeNull();
            viewModel.CopyVehiclesCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.RedoCommand.Should().NotBeNull();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SelectedRank_CanBeSet()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Act
            viewModel.SelectedRank = ranks[1];

            // Assert
            viewModel.SelectedRank.Should().Be(ranks[1]);
            viewModel.SelectedRank.Name.Should().Be("Detective");
        }

        [Fact]
        public void SelectedRank_RaisesPropertyChanged()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);
            var eventRaised = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedRank))
                    eventRaised = true;
            };

            // Act
            viewModel.SelectedRank = ranks[1];

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void SelectedCopyFromRank_CanBeSet()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Act
            viewModel.SelectedCopyFromRank = ranks[0];

            // Assert
            viewModel.SelectedCopyFromRank.Should().Be(ranks[0]);
        }

        [Fact]
        public void SelectedCopyToRank_CanBeSet()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var viewModel = new VehiclesViewModel(_mockDataService.Object, ranks);

            // Act
            viewModel.SelectedCopyToRank = ranks[0];

            // Assert
            viewModel.SelectedCopyToRank.Should().Be(ranks[0]);
        }

        [Fact]
        public void VehicleAdvisory_DefaultsToEmpty()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.VehicleAdvisory.Should().BeEmpty();
        }

        [Fact]
        public void VehicleAdvisory_CanBeSet()
        {
            // Arrange
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Act
            viewModel.VehicleAdvisory = "Warning: No vehicles assigned";

            // Assert
            viewModel.VehicleAdvisory.Should().Be("Warning: No vehicles assigned");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddVehiclesCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.AddVehiclesCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveVehiclesCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.RemoveVehiclesCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveAllVehiclesCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.RemoveAllVehiclesCommand.Should().NotBeNull();
        }

        [Fact]
        public void CopyFromRankCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.CopyFromRankCommand.Should().NotBeNull();
        }

        [Fact]
        public void CopyVehiclesCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.CopyVehiclesCommand.Should().NotBeNull();
        }

        [Fact]
        public void UndoCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.UndoCommand.Should().NotBeNull();
        }

        [Fact]
        public void RedoCommand_IsCreated()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.RedoCommand.Should().NotBeNull();
        }

        #endregion

        #region TreeView Tests

        [Fact]
        public void VehicleTreeItems_IsInitialized()
        {
            // Act
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);

            // Assert
            viewModel.VehicleTreeItems.Should().NotBeNull();
            viewModel.VehicleTreeItems.Should().BeEmpty("no rank is selected initially");
        }

        [Fact]
        public void SelectedTreeItem_CanBeSet()
        {
            // Arrange
            var viewModel = new VehiclesViewModel(_mockDataService.Object, null);
            var vehicle = new Vehicle("police", "Police Cruiser", "LSPD");
            var treeItem = new VehicleTreeItemViewModel("Test", vehicle);

            // Act
            viewModel.SelectedTreeItem = treeItem;

            // Assert
            viewModel.SelectedTreeItem.Should().Be(treeItem);
        }

        #endregion
    }
}
