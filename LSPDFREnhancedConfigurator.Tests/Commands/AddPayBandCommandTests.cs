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
    public class AddPayBandCommandTests
    {
        #region Constructor Tests

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenRanksNull()
        {
            // Arrange
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(null!, newPayBand, parent, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenNewPayBandNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(ranks, null!, parent, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenParentNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(ranks, newPayBand, null!, 0, (p) => { }, () => { }, () => { }));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenRenumberCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(ranks, newPayBand, parent, 0, null!, () => { }, () => { }));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenRefreshCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(ranks, newPayBand, parent, 0, (p) => { }, null!, () => { }));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_ThrowsWhenDataChangedCallbackNull()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AddPayBandCommand(ranks, newPayBand, parent, 0, (p) => { }, () => { }, null!));
        }

        [Fact]
        public void AddPayBandCommand_Constructor_SetsDescription()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new AddPayBandCommand(ranks, newPayBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.Description.Should().Contain("Pay Band I");
            command.Description.Should().Contain("Officer");
        }

        [Fact]
        public void AddPayBandCommand_Constructor_SetsParentRankId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new AddPayBandCommand(ranks, newPayBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.ParentRankId.Should().Be(parent.Id);
        }

        [Fact]
        public void AddPayBandCommand_Constructor_SetsNewPayBandId()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            var command = new AddPayBandCommand(ranks, newPayBand, parent, 0, (p) => { }, () => { }, () => { });

            // Assert
            command.NewPayBandId.Should().Be(newPayBand.Id);
        }

        #endregion

        #region Execute Tests

        [Fact]
        public void AddPayBandCommand_Execute_InsertsPayBandAtValidIndex()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var existingPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(existingPayBand);

            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 1, // Insert at end
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().HaveCount(2);
            parent.PayBands[1].Should().Be(newPayBand);
        }

        [Fact]
        public void AddPayBandCommand_Execute_AppendsPayBandWhenIndexOutOfBounds()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 999, // Out of bounds
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.PayBands.Should().HaveCount(1);
            parent.PayBands[0].Should().Be(newPayBand);
        }

        [Fact]
        public void AddPayBandCommand_Execute_SetsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            newPayBand.Parent.Should().Be(parent);
        }

        [Fact]
        public void AddPayBandCommand_Execute_SetsIsParentFlag()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            parent.IsParent.Should().BeFalse(); // Initially false

            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            parent.IsParent.Should().BeTrue();
        }

        [Fact]
        public void AddPayBandCommand_Execute_CallsRenumberPayBands()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            RankHierarchy? renumberedParent = null;
            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => renumberedParent = p, () => { }, () => { });

            // Act
            command.Execute();

            // Assert
            renumberedParent.Should().Be(parent);
        }

        [Fact]
        public void AddPayBandCommand_Execute_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
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
        public void AddPayBandCommand_Execute_ThrowsWhenParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy>(); // Parent not in list
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Execute())
                .Message.Should().Contain("Parent rank not found");
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void AddPayBandCommand_Undo_RemovesPayBand()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            parent.PayBands.Should().BeEmpty();
        }

        [Fact]
        public void AddPayBandCommand_Undo_ClearsIsParentWhenLastPayBandRemoved()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.IsParent.Should().BeTrue();

            // Act
            command.Undo();

            // Assert
            parent.IsParent.Should().BeFalse();
        }

        [Fact]
        public void AddPayBandCommand_Undo_CallsRenumberWhenPayBandsRemain()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var existingPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();
            parent.PayBands.Add(existingPayBand);
            parent.IsParent = true;

            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band II").Build();

            var renumberCallCount = 0;
            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 1,
                (p) => renumberCallCount++, () => { }, () => { });

            command.Execute();
            renumberCallCount = 0; // Reset after execute

            // Act
            command.Undo();

            // Assert
            renumberCallCount.Should().Be(1);
            parent.PayBands.Should().HaveCount(1);
            parent.IsParent.Should().BeTrue(); // Still true because one remains
        }

        [Fact]
        public void AddPayBandCommand_Undo_ClearsParentReference()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            newPayBand.Parent.Should().Be(parent);

            // Act
            command.Undo();

            // Assert
            newPayBand.Parent.Should().BeNull();
        }

        [Fact]
        public void AddPayBandCommand_Undo_CallsRefreshAndDataChanged()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
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
        public void AddPayBandCommand_Undo_ThrowsWhenParentNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            ranks.Clear(); // Remove parent to simulate not found

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo())
                .Message.Should().Contain("Parent rank not found");
        }

        [Fact]
        public void AddPayBandCommand_Undo_ThrowsWhenPayBandNotFound()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            command.Execute();
            parent.PayBands.Clear(); // Remove pay band to simulate not found

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => command.Undo())
                .Message.Should().Contain("Pay band");
        }

        #endregion

        #region Undo/Redo Tests

        [Fact]
        public void AddPayBandCommand_UndoRedo_WorksCorrectly()
        {
            // Arrange
            var parent = new RankHierarchyBuilder().WithName("Officer").Build();
            var ranks = new List<RankHierarchy> { parent };
            var newPayBand = new RankHierarchyBuilder().WithName("Pay Band I").Build();

            var command = new AddPayBandCommand(
                ranks, newPayBand, parent, 0,
                (p) => { }, () => { }, () => { });

            // Act & Assert
            command.Execute();
            parent.PayBands.Should().Contain(newPayBand);
            parent.IsParent.Should().BeTrue();
            newPayBand.Parent.Should().Be(parent);

            command.Undo();
            parent.PayBands.Should().NotContain(newPayBand);
            parent.IsParent.Should().BeFalse();
            newPayBand.Parent.Should().BeNull();

            command.Execute(); // Redo
            parent.PayBands.Should().Contain(newPayBand);
            parent.IsParent.Should().BeTrue();
            newPayBand.Parent.Should().Be(parent);
        }

        #endregion
    }
}
