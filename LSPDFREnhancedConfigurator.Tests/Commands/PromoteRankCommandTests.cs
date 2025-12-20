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
    public class PromoteRankCommandTests
    {
        #region Constructor Tests

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(null!, payBand, parent, 0, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenPayBandNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(ranks, null!, parent, 0, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenOriginalParentNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(ranks, payBand, null!, 0, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenRenumberCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(ranks, payBand, parent, 0, 0, null!, () => { }, () => { }));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(ranks, payBand, parent, 0, 0, (p) => { }, null!, () => { }));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PromoteRankCommand(ranks, payBand, parent, 0, 0, (p) => { }, () => { }, null!));
        }

        [Fact]
        public void PromoteRankCommand_Constructor_SetsDescription()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new PromoteRankCommand(ranks, payBand, parent, 0, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("Pay Band I");
            command.Description.Should().Contain("Officer");
            command.Description.Should().Contain("Promote");
        }

        [Fact]
        public void PromoteRankCommand_Constructor_SetsPromotedRankId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new PromoteRankCommand(ranks, payBand, parent, 0, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.PromotedRankId.Should().Be(payBand.Id);
        }

        #endregion

        #region Execute Tests

        [Fact]
        public void PromoteRankCommand_Execute_RemovesFromParentPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().BeEmpty();
        }

        [Fact]
        public void PromoteRankCommand_Execute_ClearsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            payBand.Parent.Should().BeNull();
        }

        [Fact]
        public void PromoteRankCommand_Execute_InsertsAtValidIndex()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var sergeant = new RankHierarchyBuilder().WithName("Sergeant").Build();
            var ranks = new List<RankHierarchy> { parent, sergeant };

            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1, // Insert at index 1 (between parent and sergeant)
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(3);
            ranks[1].Should().Be(payBand);
            ranks[0].Should().Be(parent);
            ranks[2].Should().Be(sergeant);
        }

        [Fact]
        public void PromoteRankCommand_Execute_AppendsWhenIndexOutOfBounds()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 999, // Out of bounds
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(2);
            ranks[1].Should().Be(payBand);
        }

        [Fact]
        public void PromoteRankCommand_Execute_ClearsIsParentWhenLastPayBandRemoved()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeFalse();
        }

        [Fact]
        public void PromoteRankCommand_Execute_KeepsIsParentWhenPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand1 = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var payBand2 = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);
            payBand1.Parent = parent;
            payBand2.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand1, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeTrue();
            parent.PayBands.Should().HaveCount(1);
        }

        [Fact]
        public void PromoteRankCommand_Execute_CallsRenumberWhenPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand1 = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var payBand2 = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);
            payBand1.Parent = parent;
            payBand2.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            RankHierarchy? renumberedParent = null;
            var command = new PromoteRankCommand(
                ranks, payBand1, parent, 0, 1,
                (p) => renumberedParent = p, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            renumberedParent.Should().Be(parent);
        }

        [Fact]
        public void PromoteRankCommand_Execute_DoesNotCallRenumberWhenNoPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var renumberCalled = false;
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => renumberCalled = true, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            renumberCalled.Should().BeFalse();
        }

        [Fact]
        public void PromoteRankCommand_Execute_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { },
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void PromoteRankCommand_Execute_ThrowsWhenOriginalParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy>(); // Parent not in list
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("Original parent rank not found");
        }

        [Fact]
        public void PromoteRankCommand_Execute_ThrowsWhenPayBandNotFoundInParent()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            // Don't add payBand to parent.PayBands

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("not found in parent's list");
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void PromoteRankCommand_Undo_RestoresToParentPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.PayBands.Should().BeEmpty();

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().HaveCount(1);
            parent.PayBands[0].Should().Be(payBand);
        }

        [Fact]
        public void PromoteRankCommand_Undo_RemovesFromRanksList()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            ranks.Should().Contain(payBand);

            // Act
            command.Undo();

            // Assert
            ranks.Should().NotContain(payBand);
            ranks.Should().HaveCount(1); // Only parent remains
        }

        [Fact]
        public void PromoteRankCommand_Undo_RestoresParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            payBand.Parent.Should().BeNull();

            // Act
            command.Undo();

            // Assert
            payBand.Parent.Should().Be(parent);
        }

        [Fact]
        public void PromoteRankCommand_Undo_RestoresAtOriginalIndex()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand1 = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var payBand2 = new RankHierarchyBuilder().WithName("Pay Band II").Build();
            var payBand3 = new RankHierarchyBuilder().WithName("Pay Band III").Build();
            parent.PayBands.Add(payBand1);
            parent.PayBands.Add(payBand2);
            parent.PayBands.Add(payBand3);
            payBand1.Parent = parent;
            payBand2.Parent = parent;
            payBand3.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand2, parent, 1, // Original index is 1
                1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.PayBands.Should().HaveCount(2);

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().HaveCount(3);
            parent.PayBands[1].Should().Be(payBand2); // Restored at index 1
        }

        [Fact]
        public void PromoteRankCommand_Undo_AppendsWhenOriginalIndexOutOfBounds()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 999, // Invalid original index
                1,
                (p) => { }, () => { }, () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().HaveCount(1);
            parent.PayBands[0].Should().Be(payBand);
        }

        [Fact]
        public void PromoteRankCommand_Undo_SetsIsParentFlag()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.IsParent.Should().BeFalse();

            // Act
            command.Undo();

            // Assert
            parent.IsParent.Should().BeTrue();
        }

        [Fact]
        public void PromoteRankCommand_Undo_CallsRenumber()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var renumberCount = 0;
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => renumberCount++, () => { }, () => { });

            command.Execute();
            renumberCount = 0; // Reset after execute

            // Act
            command.Undo();

            // Assert
            renumberCount.Should().Be(1);
        }

        [Fact]
        public void PromoteRankCommand_Undo_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
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
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void PromoteRankCommand_Undo_ThrowsWhenOriginalParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            ranks.Clear(); // Remove parent to simulate not found

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo())
                .Message.Should().Contain("Original parent rank not found");
        }

        [Fact]
        public void PromoteRankCommand_Undo_ThrowsWhenRankNotFoundInRanksList()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            command.Execute();
            ranks.Remove(payBand); // Remove promoted rank to simulate not found

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo())
                .Message.Should().Contain("not found in ranks list");
        }

        #endregion

        #region Undo/Redo Tests

        [Fact]
        public void PromoteRankCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new PromoteRankCommand(
                ranks, payBand, parent, 0, 1,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            command.Execute();
            ranks.Should().Contain(payBand);
            parent.PayBands.Should().BeEmpty();
            payBand.Parent.Should().BeNull();
            parent.IsParent.Should().BeFalse();

            command.Undo();
            ranks.Should().NotContain(payBand);
            parent.PayBands.Should().Contain(payBand);
            payBand.Parent.Should().Be(parent);
            parent.IsParent.Should().BeTrue();

            command.Execute(); // Redo
            ranks.Should().Contain(payBand);
            parent.PayBands.Should().BeEmpty();
            payBand.Parent.Should().BeNull();
            parent.IsParent.Should().BeFalse();
        }

        #endregion
    }
}
