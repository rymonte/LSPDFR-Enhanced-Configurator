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
    public class CloneRankCommandTests
    {
        #region Constructor Tests - Parent Rank

        [Fact]
        public void CloneRankCommand_ParentRankConstructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    null!,
                    clonedRank,
                    0,
                    "Officer",
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_ParentRankConstructor_ThrowsWhenClonedRankNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    null!,
                    0,
                    "Officer",
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_ParentRankConstructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedRank,
                    0,
                    "Officer",
                    null!,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_ParentRankConstructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedRank,
                    0,
                    "Officer",
                    () => refreshCalled = true,
                    null!));
        }

        #endregion

        #region Constructor Tests - Pay Band

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    null!,
                    clonedPayBand,
                    parent,
                    0,
                    "Pay Band I",
                    (p) => { },
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenClonedPayBandNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    null!,
                    parent,
                    0,
                    "Pay Band I",
                    (p) => { },
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenParentNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedPayBand,
                    null!,
                    0,
                    "Pay Band I",
                    (p) => { },
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenRenumberCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedPayBand,
                    parent,
                    0,
                    "Pay Band I",
                    null!,
                    () => refreshCalled = true,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            bool dataChangedCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedPayBand,
                    parent,
                    0,
                    "Pay Band I",
                    (p) => { },
                    null!,
                    () => dataChangedCalled = true));
        }

        [Fact]
        public void CloneRankCommand_PayBandConstructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            bool refreshCalled = false;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CloneRankCommand(
                    ranks,
                    clonedPayBand,
                    parent,
                    0,
                    "Pay Band I",
                    (p) => { },
                    () => refreshCalled = true,
                    null!));
        }

        #endregion

        #region Parent Rank Cloning Tests

        [Fact]
        public void CloneRankCommand_Execute_InsertsParentRankAtValidIndex()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var rank2 = new RankHierarchyBuilder().WithName("Detective").Build();
            var ranks = new List<RankHierarchy> { rank1, rank2 };

            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                1, // Insert between rank1 and rank2
                "Officer",
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(3);
            ranks[1].Should().Be(clonedRank);
            ranks[0].Should().Be(rank1);
            ranks[2].Should().Be(rank2);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CloneRankCommand_Execute_AppendsParentRankWhenIndexOutOfBounds()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank1 };

            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                999, // Out of bounds
                "Officer",
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(2);
            ranks[1].Should().Be(clonedRank);
        }

        [Fact]
        public void CloneRankCommand_Execute_InsertsAtIndexZero()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank1 };

            var clonedRank = new RankHierarchyBuilder().WithName("Probationary Officer").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0, // Insert at beginning
                "Officer",
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(2);
            ranks[0].Should().Be(clonedRank);
            ranks[1].Should().Be(rank1);
        }

        [Fact]
        public void CloneRankCommand_Undo_RemovesParentRank()
        {
            // Arrange
            var rank1 = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank1 };

            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                1,
                "Officer",
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            ranks.Should().HaveCount(1);
            ranks.Should().NotContain(clonedRank);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CloneRankCommand_Description_IncludesRankNames()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            // Act
            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0,
                "Officer",
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("Officer");
            command.Description.Should().Contain("Officer Clone");
        }

        [Fact]
        public void CloneRankCommand_ClonedRankId_ReturnsCorrectId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            // Act
            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0,
                "Officer",
                () => { },
                () => { });

            // Assert
            command.ClonedRankId.Should().Be(clonedRank.Id);
        }

        [Fact]
        public void CloneRankCommand_ParentRankId_IsNullForParentRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            // Act
            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0,
                "Officer",
                () => { },
                () => { });

            // Assert
            command.ParentRankId.Should().BeNull();
        }

        [Fact]
        public void CloneRankCommand_UndoRedo_WorksCorrectlyForParentRank()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0,
                "Officer",
                () => { },
                () => { });

            // Act & Assert
            command.Execute();
            ranks.Should().Contain(clonedRank);

            command.Undo();
            ranks.Should().NotContain(clonedRank);

            command.Execute(); // Redo
            ranks.Should().Contain(clonedRank);
        }

        #endregion

        #region Pay Band Cloning Tests

        [Fact]
        public void CloneRankCommand_Execute_InsertsPayBandAtValidIndex()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand1 = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand1);

            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();

            var renumberCalled = false;
            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                1, // Insert after payBand1
                "Pay Band I",
                (p) => renumberCalled = true,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().HaveCount(2);
            parent.PayBands[1].Should().Be(clonedPayBand);
            clonedPayBand.Parent.Should().Be(parent);
            parent.IsParent.Should().BeTrue();
            renumberCalled.Should().BeTrue();
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CloneRankCommand_Execute_AppendsPayBandWhenIndexOutOfBounds()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand1 = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand1);

            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                999, // Out of bounds
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().HaveCount(2);
            parent.PayBands[1].Should().Be(clonedPayBand);
        }

        [Fact]
        public void CloneRankCommand_Execute_SetsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            clonedPayBand.Parent.Should().Be(parent);
        }

        [Fact]
        public void CloneRankCommand_Execute_SetsIsParentFlag()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            parent.IsParent.Should().BeFalse(); // Initially false

            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeTrue();
        }

        [Fact]
        public void CloneRankCommand_Execute_CallsRenumberPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            RankHierarchy? renumberedParent = null;
            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => renumberedParent = p,
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            renumberedParent.Should().Be(parent);
        }

        [Fact]
        public void CloneRankCommand_Undo_RemovesPayBandAndClearsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            command.Execute();

            // Reset flags
            refreshCalled = false;
            dataChangedCalled = false;

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().BeEmpty();
            clonedPayBand.Parent.Should().BeNull();
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void CloneRankCommand_Undo_ResetsIsParentWhenLastPayBandRemoved()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            command.Execute();
            parent.IsParent.Should().BeTrue();

            // Act
            command.Undo();

            // Assert
            parent.IsParent.Should().BeFalse();
        }

        [Fact]
        public void CloneRankCommand_Undo_CallsRenumberPayBandsIfPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var existingPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(existingPayBand);
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();

            var renumberCallCount = 0;
            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                1,
                "Pay Band I",
                (p) => renumberCallCount++,
                () => { },
                () => { });

            command.Execute();
            renumberCallCount = 0; // Reset count after execute

            // Act
            command.Undo();

            // Assert
            renumberCallCount.Should().Be(1);
            parent.PayBands.Should().HaveCount(1);
            parent.IsParent.Should().BeTrue(); // Still true because one pay band remains
        }

        [Fact]
        public void CloneRankCommand_ParentRankId_ReturnsParentIdForPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Assert
            command.ParentRankId.Should().Be(parent.Id);
        }

        [Fact]
        public void CloneRankCommand_UndoRedo_WorksCorrectlyForPayBand()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Act & Assert
            command.Execute();
            parent.PayBands.Should().Contain(clonedPayBand);
            parent.IsParent.Should().BeTrue();

            command.Undo();
            parent.PayBands.Should().NotContain(clonedPayBand);
            parent.IsParent.Should().BeFalse();

            command.Execute(); // Redo
            parent.PayBands.Should().Contain(clonedPayBand);
            parent.IsParent.Should().BeTrue();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void CloneRankCommand_Execute_ThrowsWhenParentNotFoundForPayBand()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy>(); // Parent not in list

            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute());
        }

        [Fact]
        public void CloneRankCommand_Undo_ThrowsWhenRankNotFoundForParentRank()
        {
            // Arrange
            var clonedRank = new RankHierarchyBuilder().WithName("Officer Clone").Build();
            var ranks = new List<RankHierarchy>();

            var command = new CloneRankCommand(
                ranks,
                clonedRank,
                0,
                "Officer",
                () => { },
                () => { });

            command.Execute();
            ranks.Clear(); // Clear the list to simulate not found scenario

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo());
        }

        [Fact]
        public void CloneRankCommand_Undo_ThrowsWhenParentNotFoundForPayBand()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var ranks = new List<RankHierarchy> { parent };

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            command.Execute();
            ranks.Remove(parent); // Manually remove parent to simulate not found scenario

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo());
        }

        [Fact]
        public void CloneRankCommand_Undo_ThrowsWhenPayBandNotFoundInParent()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var clonedPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var ranks = new List<RankHierarchy> { parent };

            var command = new CloneRankCommand(
                ranks,
                clonedPayBand,
                parent,
                0,
                "Pay Band I",
                (p) => { },
                () => { },
                () => { });

            command.Execute();
            parent.PayBands.Remove(clonedPayBand); // Manually remove to simulate not found scenario

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo());
        }

        #endregion
    }
}
