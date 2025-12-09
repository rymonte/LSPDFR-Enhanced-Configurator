using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for CopyFromRankDialogViewModel covering initialization, property changes, and selection behavior
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("ViewModel", "CopyFromRankDialog")]
    public class CopyFromRankDialogViewModelTests
    {
        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesWithDefaults()
        {
            // Act
            var viewModel = new CopyFromRankDialogViewModel();

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.AvailableRanks.Should().NotBeNull().And.BeEmpty();
            viewModel.Title.Should().Be("Copy From Rank");
            viewModel.Description.Should().Be("Select a rank to copy data from");
            viewModel.SelectedRank.Should().BeNull();
            viewModel.CanCopy.Should().BeFalse("no rank selected initially");
        }

        #endregion

        #region Property Change Tests

        [Fact]
        public void Title_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.Title))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Title = "Custom Title";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.Title.Should().Be("Custom Title");
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.Description))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Description = "Custom description";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.Description.Should().Be("Custom description");
        }

        [Fact]
        public void SelectedRank_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var rank = RankHierarchyBuilder.CreateDefault();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedRank))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SelectedRank = rank;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.SelectedRank.Should().Be(rank);
        }

        [Fact]
        public void SelectedRank_WhenSet_RaisesCanCopyPropertyChanged()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var rank = RankHierarchyBuilder.CreateDefault();
            var canCopyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.CanCopy))
                    canCopyChangedRaised = true;
            };

            // Act
            viewModel.SelectedRank = rank;

            // Assert
            canCopyChangedRaised.Should().BeTrue("CanCopy should update when SelectedRank changes");
        }

        #endregion

        #region CanCopy Tests

        [Fact]
        public void CanCopy_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();

            // Act & Assert
            viewModel.CanCopy.Should().BeFalse("cannot copy without a selected rank");
        }

        [Fact]
        public void CanCopy_WithSelection_ReturnsTrue()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var rank = RankHierarchyBuilder.CreateDefault();

            // Act
            viewModel.SelectedRank = rank;

            // Assert
            viewModel.CanCopy.Should().BeTrue("should be able to copy when rank is selected");
        }

        [Fact]
        public void CanCopy_WhenSelectionCleared_ReturnsFalse()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var rank = RankHierarchyBuilder.CreateDefault();
            viewModel.SelectedRank = rank;

            // Act
            viewModel.SelectedRank = null;

            // Assert
            viewModel.CanCopy.Should().BeFalse("should not be able to copy when selection is cleared");
        }

        #endregion

        #region AvailableRanks Tests

        [Fact]
        public void AvailableRanks_CanAddRanks()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Sergeant").Build();

            // Act
            viewModel.AvailableRanks.Add(rank1);
            viewModel.AvailableRanks.Add(rank2);

            // Assert
            viewModel.AvailableRanks.Should().HaveCount(2);
            viewModel.AvailableRanks.Should().Contain(rank1);
            viewModel.AvailableRanks.Should().Contain(rank2);
        }

        [Fact]
        public void AvailableRanks_CanClearRanks()
        {
            // Arrange
            var viewModel = new CopyFromRankDialogViewModel();
            viewModel.AvailableRanks.Add(RankHierarchyBuilder.CreateDefault());
            viewModel.AvailableRanks.Add(RankHierarchyBuilder.CreateDefault());

            // Act
            viewModel.AvailableRanks.Clear();

            // Assert
            viewModel.AvailableRanks.Should().BeEmpty();
        }

        #endregion
    }
}
