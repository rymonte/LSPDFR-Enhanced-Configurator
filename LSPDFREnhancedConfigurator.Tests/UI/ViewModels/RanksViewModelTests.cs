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
    /// Tests for RanksViewModel core functionality (non-validation)
    /// Note: Validation tests are in RanksViewModelValidationTests.cs (89 tests)
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class RanksViewModelTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;

        public RanksViewModelTests()
        {
            _mockDataService = new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultStations()
                .BuildMock();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRanks_InitializesSuccessfully()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.RankTreeItems.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithEmptyRanks_InitializesSuccessfully()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.RankTreeItems.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithRanks_PopulatesTreeView()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(2);
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.AddRankCommand.Should().NotBeNull();
            viewModel.AddPayBandCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.RedoCommand.Should().NotBeNull();
            viewModel.PromoteCommand.Should().NotBeNull();
            viewModel.CloneCommand.Should().NotBeNull();
            viewModel.RemoveCommand.Should().NotBeNull();
            viewModel.RemoveAllRanksCommand.Should().NotBeNull();
            viewModel.MoveUpCommand.Should().NotBeNull();
            viewModel.MoveDownCommand.Should().NotBeNull();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void RankTreeItems_IsInitialized()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RankTreeItems.Should().NotBeNull();
        }

        [Fact]
        public void RankHierarchies_ReturnsInternalRanksList()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert
            viewModel.RankHierarchies.Should().BeSameAs(ranks);
            viewModel.RankHierarchies.Should().HaveCount(1);
        }

        [Fact]
        public void SelectedTreeItem_CanBeSet()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Act
            viewModel.SelectedTreeItem = viewModel.RankTreeItems.First();

            // Assert
            viewModel.SelectedTreeItem.Should().NotBeNull();
        }

        [Fact]
        public void SelectedTreeItem_RaisesPropertyChanged()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);
            var eventRaised = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedTreeItem))
                    eventRaised = true;
            };

            // Act
            viewModel.SelectedTreeItem = viewModel.RankTreeItems.First();

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void RankName_DefaultsToEmpty()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RankName.Should().BeEmpty();
        }

        [Fact]
        public void RequiredPoints_DefaultsToNull()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RequiredPoints.Should().BeNull();
        }

        [Fact]
        public void Salary_DefaultsToNull()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.Salary.Should().BeNull();
        }

        [Fact]
        public void ShowXpSalaryFields_DefaultsToFalse()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.ShowXpSalaryFields.Should().BeFalse("no rank selected initially");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddRankCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.AddRankCommand.Should().NotBeNull();
        }

        [Fact]
        public void AddPayBandCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.AddPayBandCommand.Should().NotBeNull();
        }

        [Fact]
        public void PromoteCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.PromoteCommand.Should().NotBeNull();
        }

        [Fact]
        public void CloneCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.CloneCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RemoveCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveAllRanksCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RemoveAllRanksCommand.Should().NotBeNull();
        }

        [Fact]
        public void MoveUpCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.MoveUpCommand.Should().NotBeNull();
        }

        [Fact]
        public void MoveDownCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.MoveDownCommand.Should().NotBeNull();
        }

        [Fact]
        public void UndoCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.UndoCommand.Should().NotBeNull();
        }

        [Fact]
        public void RedoCommand_IsCreated()
        {
            // Act
            var viewModel = new RanksViewModel(null, _mockDataService.Object);

            // Assert
            viewModel.RedoCommand.Should().NotBeNull();
        }

        #endregion

        #region TreeView Tests

        [Fact]
        public void RankTreeItems_WithRanks_PopulatesCorrectly()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").WithXP(0).Build(),
                new RankHierarchyBuilder().WithName("Detective").WithXP(500).Build(),
                new RankHierarchyBuilder().WithName("Sergeant").WithXP(1000).Build()
            };

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(3);
            viewModel.RankTreeItems[0].DisplayText.Should().Contain("Officer");
            viewModel.RankTreeItems[1].DisplayText.Should().Contain("Detective");
            viewModel.RankTreeItems[2].DisplayText.Should().Contain("Sergeant");
        }

        [Fact]
        public void RankTreeItems_WithPayBands_ShowsHierarchy()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Detective")
                .WithXP(500)
                .WithPayBands(3)
                .Build();

            var ranks = new List<RankHierarchy> { rank };

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert
            viewModel.RankTreeItems.Should().HaveCount(1);
            viewModel.RankTreeItems[0].Children.Should().HaveCount(3, "should have 3 pay bands");
        }

        [Fact]
        public void RankTreeItems_DisplaysXPAndSalary()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Officer")
                .WithXP(100)
                .WithSalary(2000)
                .Build();

            var ranks = new List<RankHierarchy> { rank };

            // Act
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Assert - Format is "Name (XP: 100+ | $2,000)"
            viewModel.RankTreeItems[0].DisplayText.Should().Contain("XP: 100");
            viewModel.RankTreeItems[0].DisplayText.Should().Contain("$2,000");
        }

        #endregion

        #region Selection Tests

        [Fact]
        public void SelectingRank_PopulatesFields()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Detective")
                .WithXP(500)
                .WithSalary(3500)
                .Build();

            var ranks = new List<RankHierarchy> { rank };
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Act
            viewModel.SelectedTreeItem = viewModel.RankTreeItems.First();

            // Assert
            viewModel.RankName.Should().Be("Detective");
            viewModel.RequiredPoints.Should().Be(500);
            viewModel.Salary.Should().Be(3500);
            viewModel.ShowXpSalaryFields.Should().BeTrue();
        }

        [Fact]
        public void SelectingPayBand_PopulatesFields()
        {
            // Arrange
            var rank = new RankHierarchyBuilder()
                .WithName("Detective")
                .WithXP(500)
                .WithPayBands(2)
                .Build();

            var ranks = new List<RankHierarchy> { rank };
            var viewModel = new RanksViewModel(ranks, _mockDataService.Object);

            // Act - Select first pay band
            viewModel.SelectedTreeItem = viewModel.RankTreeItems[0].Children[0];

            // Assert
            viewModel.RankName.Should().Contain("Detective");
            viewModel.RequiredPoints.Should().BeGreaterThan(500);
            viewModel.ShowXpSalaryFields.Should().BeTrue();
        }

        #endregion

        #region Event Tests

        [Fact]
        public void RequestFocusRanksTab_EventExists()
        {
            // Arrange
            var viewModel = new RanksViewModel(null, _mockDataService.Object);
            var eventRaised = false;

            viewModel.RequestFocusRanksTab += (s, e) => eventRaised = true;

            // Act
            // Trigger would come from internal operations
            // This just verifies the event exists and can be subscribed to

            // Assert
            eventRaised.Should().BeFalse("event not triggered in this test");
        }

        [Fact]
        public void StatusMessageChanged_EventExists()
        {
            // Arrange
            var viewModel = new RanksViewModel(null, _mockDataService.Object);
            var eventRaised = false;

            viewModel.StatusMessageChanged += (s, e) => eventRaised = true;

            // Act
            // Trigger would come from internal operations

            // Assert
            eventRaised.Should().BeFalse("event not triggered in this test");
        }

        [Fact]
        public void UndoRedoStateChanged_EventExists()
        {
            // Arrange
            var viewModel = new RanksViewModel(null, _mockDataService.Object);
            var eventRaised = false;

            viewModel.UndoRedoStateChanged += (s, e) => eventRaised = true;

            // Act
            // Trigger would come from undo/redo operations

            // Assert
            eventRaised.Should().BeFalse("event not triggered in this test");
        }

        #endregion
    }
}
