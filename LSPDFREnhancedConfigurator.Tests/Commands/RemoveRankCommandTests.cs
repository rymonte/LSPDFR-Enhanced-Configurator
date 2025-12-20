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
    public class RemoveRankCommandTests
    {
        #region Constructor Tests - Parent Rank

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(null!, rank, 0, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_ThrowsWhenRankNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, null!, 0, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, rank, 0, null!, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, rank, 0, () => { }, null!));
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_SetsDescription()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new RemoveRankCommand(ranks, rank, 0, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("Officer");
            command.Description.Should().Contain("Remove rank");
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_SetsRemovedRankId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new RemoveRankCommand(ranks, rank, 0, () => { }, () => { });

            // Assert
            command.RemovedRankId.Should().Be(rank.Id);
        }

        [Fact]
        public void RemoveRankCommand_ParentRankConstructor_SetsParentRankIdToNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new RemoveRankCommand(ranks, rank, 0, () => { }, () => { });

            // Assert
            command.ParentRankId.Should().BeNull();
        }

        #endregion

        #region Constructor Tests - Pay Band

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(null!, payBand, parent, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenPayBandNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, null!, parent, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenParentNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, payBand, null!, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenRenumberCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, payBand, parent, 0, null!, () => { }, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, payBand, parent, 0, (p) => { }, null!, () => { }));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new RemoveRankCommand(ranks, payBand, parent, 0, (p) => { }, () => { }, null!));
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_SetsDescription()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new RemoveRankCommand(ranks, payBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("Pay Band I");
            command.Description.Should().Contain("Officer");
            command.Description.Should().Contain("Remove pay band");
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_SetsRemovedRankId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new RemoveRankCommand(ranks, payBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.RemovedRankId.Should().Be(payBand.Id);
        }

        [Fact]
        public void RemoveRankCommand_PayBandConstructor_SetsParentRankId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act
            var command = new RemoveRankCommand(ranks, payBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.ParentRankId.Should().Be(parent.Id);
        }

        #endregion

        #region Execute Tests - Parent Rank

        [Fact]
        public void RemoveRankCommand_Execute_ParentRank_RemovesFromRanksList()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var sergeant = new RankHierarchyBuilder().WithName("Sergeant").Build();
            var ranks = new List<RankHierarchy> { officer, sergeant };

            var command = new RemoveRankCommand(ranks, officer, 0, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            ranks.Should().HaveCount(1);
            ranks.Should().NotContain(officer);
            ranks[0].Should().Be(sergeant);
        }

        [Fact]
        public void RemoveRankCommand_Execute_ParentRank_CallsRefreshAndDataChanged()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveRankCommand(
                ranks, rank, 0,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void RemoveRankCommand_Execute_ParentRank_ThrowsWhenRankNotFound()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy>(); // Rank not in list

            var command = new RemoveRankCommand(ranks, rank, 0, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("not found in list");
        }

        #endregion

        #region Execute Tests - Pay Band

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_RemovesFromParentPayBands()
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
            var command = new RemoveRankCommand(
                ranks, payBand1, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().HaveCount(1);
            parent.PayBands.Should().NotContain(payBand1);
            parent.PayBands[0].Should().Be(payBand2);
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_ClearsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            payBand.Parent.Should().BeNull();
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_ClearsIsParentWhenLastPayBandRemoved()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeFalse();
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_KeepsIsParentWhenPayBandsRemain()
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
            var command = new RemoveRankCommand(
                ranks, payBand1, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeTrue();
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_CallsRenumberWhenPayBandsRemain()
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
            var command = new RemoveRankCommand(
                ranks, payBand1, parent, 0,
                (p) => renumberedParent = p, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            renumberedParent.Should().Be(parent);
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_DoesNotCallRenumberWhenNoPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var renumberCalled = false;
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => renumberCalled = true, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            renumberCalled.Should().BeFalse();
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
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
        public void RemoveRankCommand_Execute_PayBand_ThrowsWhenParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);

            var ranks = new List<RankHierarchy>(); // Parent not in list
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("Parent rank not found");
        }

        [Fact]
        public void RemoveRankCommand_Execute_PayBand_ThrowsWhenPayBandNotFoundInParent()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            // Don't add payBand to parent.PayBands

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("not found in parent's list");
        }

        #endregion

        #region Undo Tests - Parent Rank

        [Fact]
        public void RemoveRankCommand_Undo_ParentRank_RestoresToRanksListAtOriginalIndex()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var sergeant = new RankHierarchyBuilder().WithName("Sergeant").Build();
            var lieutenant = new RankHierarchyBuilder().WithName("Lieutenant").Build();
            var ranks = new List<RankHierarchy> { officer, sergeant, lieutenant };

            var command = new RemoveRankCommand(ranks, sergeant, 1, () => { }, () => { });
            command.Execute();
            ranks.Should().HaveCount(2);

            // Act
            command.Undo();

            // Assert
            ranks.Should().HaveCount(3);
            ranks[1].Should().Be(sergeant); // Restored at original index
        }

        [Fact]
        public void RemoveRankCommand_Undo_ParentRank_AppendsWhenOriginalIndexOutOfBounds()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank };

            var command = new RemoveRankCommand(ranks, rank, 999, () => { }, () => { }); // Invalid index
            command.Execute();

            // Act
            command.Undo();

            // Assert
            ranks.Should().HaveCount(1);
            ranks[0].Should().Be(rank);
        }

        [Fact]
        public void RemoveRankCommand_Undo_ParentRank_CallsRefreshAndDataChanged()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { rank };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveRankCommand(
                ranks, rank, 0,
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

        #endregion

        #region Undo Tests - Pay Band

        [Fact]
        public void RemoveRankCommand_Undo_PayBand_RestoresToParentPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
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
        public void RemoveRankCommand_Undo_PayBand_RestoresAtOriginalIndex()
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
            var command = new RemoveRankCommand(
                ranks, payBand2, parent, 1,
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
        public void RemoveRankCommand_Undo_PayBand_AppendsWhenOriginalIndexOutOfBounds()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 999, // Invalid index
                (p) => { }, () => { }, () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().HaveCount(1);
            parent.PayBands[0].Should().Be(payBand);
        }

        [Fact]
        public void RemoveRankCommand_Undo_PayBand_RestoresParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            payBand.Parent.Should().BeNull();

            // Act
            command.Undo();

            // Assert
            payBand.Parent.Should().Be(parent);
        }

        [Fact]
        public void RemoveRankCommand_Undo_PayBand_SetsIsParentFlag()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.IsParent.Should().BeFalse();

            // Act
            command.Undo();

            // Assert
            parent.IsParent.Should().BeTrue();
        }

        [Fact]
        public void RemoveRankCommand_Undo_PayBand_CallsRenumber()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var renumberCount = 0;
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => renumberCount++, () => { }, () => { });

            command.Execute();
            renumberCount = 0; // Reset after execute

            // Act
            command.Undo();

            // Assert
            renumberCount.Should().Be(1);
        }

        [Fact]
        public void RemoveRankCommand_Undo_PayBand_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
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
        public void RemoveRankCommand_Undo_PayBand_ThrowsWhenParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            ranks.Clear(); // Remove parent to simulate not found

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo())
                .Message.Should().Contain("Parent rank not found");
        }

        #endregion

        #region Undo/Redo Tests

        [Fact]
        public void RemoveRankCommand_UndoRedo_ParentRank_WorksCorrectly()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var sergeant = new RankHierarchyBuilder().WithName("Sergeant").Build();
            var ranks = new List<RankHierarchy> { officer, sergeant };

            var command = new RemoveRankCommand(ranks, officer, 0, () => { }, () => { });

            // Act & Assert
            command.Execute();
            ranks.Should().HaveCount(1);
            ranks.Should().NotContain(officer);

            command.Undo();
            ranks.Should().HaveCount(2);
            ranks.Should().Contain(officer);
            ranks[0].Should().Be(officer);

            command.Execute(); // Redo
            ranks.Should().HaveCount(1);
            ranks.Should().NotContain(officer);
        }

        [Fact]
        public void RemoveRankCommand_UndoRedo_PayBand_WorksCorrectly()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var payBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(payBand);
            payBand.Parent = parent;
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var command = new RemoveRankCommand(
                ranks, payBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            command.Execute();
            parent.PayBands.Should().BeEmpty();
            payBand.Parent.Should().BeNull();
            parent.IsParent.Should().BeFalse();

            command.Undo();
            parent.PayBands.Should().Contain(payBand);
            payBand.Parent.Should().Be(parent);
            parent.IsParent.Should().BeTrue();

            command.Execute(); // Redo
            parent.PayBands.Should().BeEmpty();
            payBand.Parent.Should().BeNull();
            parent.IsParent.Should().BeFalse();
        }

        #endregion
    }
}
