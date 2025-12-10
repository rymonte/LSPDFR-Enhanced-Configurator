using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Services
{
    /// <summary>
    /// Tests for SelectionStateService - cross-ViewModel rank selection synchronization
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Services")]
    public class SelectionStateServiceTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            // Act
            var service = new SelectionStateService();

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void SelectedRank_DefaultsToNull()
        {
            // Act
            var service = new SelectionStateService();

            // Assert
            service.SelectedRank.Should().BeNull();
        }

        #endregion

        #region SelectedRank Property Tests

        [Fact]
        public void SelectedRank_CanBeSet()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            service.SelectedRank = rank;

            // Assert
            service.SelectedRank.Should().Be(rank);
        }

        [Fact]
        public void SelectedRank_CanBeSetToNull()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            service.SelectedRank = rank;

            // Act
            service.SelectedRank = null;

            // Assert
            service.SelectedRank.Should().BeNull();
        }

        [Fact]
        public void SelectedRank_RaisesRankSelectionChangedEvent()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Detective").Build();
            RankHierarchy? receivedRank = null;

            service.RankSelectionChanged += (s, e) => receivedRank = e.SelectedRank;

            // Act
            service.SelectedRank = rank;

            // Assert
            receivedRank.Should().Be(rank);
        }

        [Fact]
        public void SelectedRank_WithSameValue_DoesNotRaiseEvent()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            service.SelectedRank = rank;

            var eventRaised = false;
            service.RankSelectionChanged += (s, e) => eventRaised = true;

            // Act
            service.SelectedRank = rank; // Set to same value

            // Assert
            eventRaised.Should().BeFalse("event should not fire for same value");
        }

        [Fact]
        public void SelectedRank_PreventsCircularUpdates()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();

            var eventCount = 0;
            service.RankSelectionChanged += (s, e) =>
            {
                eventCount++;
                // Attempt to set rank again during event handler
                if (eventCount == 1)
                {
                    service.SelectedRank = rank2; // Should be ignored
                }
            };

            // Act
            service.SelectedRank = rank1;

            // Assert
            eventCount.Should().Be(1, "circular update should be prevented");
            service.SelectedRank.Should().Be(rank1, "original value should be preserved");
        }

        #endregion

        #region Event Tests

        [Fact]
        public void RankSelectionChanged_EventArgsContainsSelectedRank()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Sergeant").Build();
            RankSelectionChangedEventArgs? eventArgs = null;

            service.RankSelectionChanged += (s, e) => eventArgs = e;

            // Act
            service.SelectedRank = rank;

            // Assert
            eventArgs.Should().NotBeNull();
            eventArgs!.SelectedRank.Should().Be(rank);
        }

        [Fact]
        public void RankSelectionChanged_CanHaveMultipleSubscribers()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Lieutenant").Build();

            var subscriber1Called = false;
            var subscriber2Called = false;

            service.RankSelectionChanged += (s, e) => subscriber1Called = true;
            service.RankSelectionChanged += (s, e) => subscriber2Called = true;

            // Act
            service.SelectedRank = rank;

            // Assert
            subscriber1Called.Should().BeTrue("subscriber 1 should be notified");
            subscriber2Called.Should().BeTrue("subscriber 2 should be notified");
        }

        [Fact]
        public void RankSelectionChanged_WithNullRank_RaisesEventWithNull()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Captain").Build();
            service.SelectedRank = rank;

            RankHierarchy? receivedRank = rank; // Initialize with non-null
            service.RankSelectionChanged += (s, e) => receivedRank = e.SelectedRank;

            // Act
            service.SelectedRank = null;

            // Assert
            receivedRank.Should().BeNull("event should be raised with null");
        }

        #endregion

        #region Integration Scenario Tests

        [Fact]
        public void MultipleViewModels_CanSynchronizeThroughService()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            RankHierarchy? viewModel1Rank = null;
            RankHierarchy? viewModel2Rank = null;

            // Simulate two ViewModels subscribing
            service.RankSelectionChanged += (s, e) => viewModel1Rank = e.SelectedRank;
            service.RankSelectionChanged += (s, e) => viewModel2Rank = e.SelectedRank;

            // Act
            service.SelectedRank = rank;

            // Assert
            viewModel1Rank.Should().Be(rank, "ViewModel 1 should receive update");
            viewModel2Rank.Should().Be(rank, "ViewModel 2 should receive update");
        }

        [Fact]
        public void RankChange_NotifiesAllSubscribers()
        {
            // Arrange
            var service = new SelectionStateService();
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();

            var notificationCount = 0;
            service.RankSelectionChanged += (s, e) => notificationCount++;

            // Act
            service.SelectedRank = rank1;
            service.SelectedRank = rank2;

            // Assert
            notificationCount.Should().Be(2, "both changes should notify subscribers");
        }

        #endregion
    }
}
