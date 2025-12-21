using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class CompositeCommandTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDescription_SetsDescription()
        {
            // Act
            var command = new CompositeCommand("Batch operation");

            // Assert
            command.Description.Should().Be("Batch operation");
        }

        [Fact]
        public void Constructor_WithNullDescription_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new CompositeCommand(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("description");
        }

        [Fact]
        public void Constructor_InitializesEmptyCommandsList()
        {
            // Act
            var command = new CompositeCommand("Test");

            // Assert
            command.CommandCount.Should().Be(0);
        }

        #endregion

        #region AddCommand Tests

        [Fact]
        public void AddCommand_WithValidCommand_IncreasesCommandCount()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var mockCommand = new Mock<IUndoRedoCommand>();

            // Act
            composite.AddCommand(mockCommand.Object);

            // Assert
            composite.CommandCount.Should().Be(1);
        }

        [Fact]
        public void AddCommand_WithMultipleCommands_IncreasesCommandCount()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var mockCommand1 = new Mock<IUndoRedoCommand>();
            var mockCommand2 = new Mock<IUndoRedoCommand>();
            var mockCommand3 = new Mock<IUndoRedoCommand>();

            // Act
            composite.AddCommand(mockCommand1.Object);
            composite.AddCommand(mockCommand2.Object);
            composite.AddCommand(mockCommand3.Object);

            // Assert
            composite.CommandCount.Should().Be(3);
        }

        [Fact]
        public void AddCommand_WithNullCommand_ThrowsArgumentNullException()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");

            // Act
            var act = () => composite.AddCommand(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("command");
        }

        #endregion

        #region Execute Tests

        [Fact]
        public void Execute_WithNoCommands_DoesNotThrow()
        {
            // Arrange
            var composite = new CompositeCommand("Empty batch");

            // Act
            var act = () => composite.Execute();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Execute_WithOneCommand_ExecutesCommand()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var mockCommand = new Mock<IUndoRedoCommand>();
            composite.AddCommand(mockCommand.Object);

            // Act
            composite.Execute();

            // Assert
            mockCommand.Verify(c => c.Execute(), Times.Once);
        }

        [Fact]
        public void Execute_WithMultipleCommands_ExecutesAllCommandsInOrder()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var executionOrder = new List<int>();

            var mockCommand1 = new Mock<IUndoRedoCommand>();
            mockCommand1.Setup(c => c.Execute()).Callback(() => executionOrder.Add(1));

            var mockCommand2 = new Mock<IUndoRedoCommand>();
            mockCommand2.Setup(c => c.Execute()).Callback(() => executionOrder.Add(2));

            var mockCommand3 = new Mock<IUndoRedoCommand>();
            mockCommand3.Setup(c => c.Execute()).Callback(() => executionOrder.Add(3));

            composite.AddCommand(mockCommand1.Object);
            composite.AddCommand(mockCommand2.Object);
            composite.AddCommand(mockCommand3.Object);

            // Act
            composite.Execute();

            // Assert
            executionOrder.Should().Equal(1, 2, 3);
            mockCommand1.Verify(c => c.Execute(), Times.Once);
            mockCommand2.Verify(c => c.Execute(), Times.Once);
            mockCommand3.Verify(c => c.Execute(), Times.Once);
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void Undo_WithNoCommands_DoesNotThrow()
        {
            // Arrange
            var composite = new CompositeCommand("Empty batch");

            // Act
            var act = () => composite.Undo();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Undo_WithOneCommand_UndoesCommand()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var mockCommand = new Mock<IUndoRedoCommand>();
            composite.AddCommand(mockCommand.Object);

            // Act
            composite.Undo();

            // Assert
            mockCommand.Verify(c => c.Undo(), Times.Once);
        }

        [Fact]
        public void Undo_WithMultipleCommands_UndoesAllCommandsInReverseOrder()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var undoOrder = new List<int>();

            var mockCommand1 = new Mock<IUndoRedoCommand>();
            mockCommand1.Setup(c => c.Undo()).Callback(() => undoOrder.Add(1));

            var mockCommand2 = new Mock<IUndoRedoCommand>();
            mockCommand2.Setup(c => c.Undo()).Callback(() => undoOrder.Add(2));

            var mockCommand3 = new Mock<IUndoRedoCommand>();
            mockCommand3.Setup(c => c.Undo()).Callback(() => undoOrder.Add(3));

            composite.AddCommand(mockCommand1.Object);
            composite.AddCommand(mockCommand2.Object);
            composite.AddCommand(mockCommand3.Object);

            // Act
            composite.Undo();

            // Assert - Should undo in reverse order: 3, 2, 1
            undoOrder.Should().Equal(3, 2, 1);
            mockCommand1.Verify(c => c.Undo(), Times.Once);
            mockCommand2.Verify(c => c.Undo(), Times.Once);
            mockCommand3.Verify(c => c.Undo(), Times.Once);
        }

        #endregion

        #region GetSubCommandDescriptions Tests

        [Fact]
        public void GetSubCommandDescriptions_WithNoCommands_ReturnsEmptyList()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");

            // Act
            var descriptions = composite.GetSubCommandDescriptions();

            // Assert
            descriptions.Should().BeEmpty();
        }

        [Fact]
        public void GetSubCommandDescriptions_WithOneCommand_ReturnsOneDescription()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");
            var mockCommand = new Mock<IUndoRedoCommand>();
            mockCommand.Setup(c => c.Description).Returns("Command 1");
            composite.AddCommand(mockCommand.Object);

            // Act
            var descriptions = composite.GetSubCommandDescriptions();

            // Assert
            descriptions.Should().HaveCount(1);
            descriptions[0].Should().Be("Command 1");
        }

        [Fact]
        public void GetSubCommandDescriptions_WithMultipleCommands_ReturnsAllDescriptions()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");

            var mockCommand1 = new Mock<IUndoRedoCommand>();
            mockCommand1.Setup(c => c.Description).Returns("Command 1");

            var mockCommand2 = new Mock<IUndoRedoCommand>();
            mockCommand2.Setup(c => c.Description).Returns("Command 2");

            var mockCommand3 = new Mock<IUndoRedoCommand>();
            mockCommand3.Setup(c => c.Description).Returns("Command 3");

            composite.AddCommand(mockCommand1.Object);
            composite.AddCommand(mockCommand2.Object);
            composite.AddCommand(mockCommand3.Object);

            // Act
            var descriptions = composite.GetSubCommandDescriptions();

            // Assert
            descriptions.Should().HaveCount(3);
            descriptions.Should().Equal("Command 1", "Command 2", "Command 3");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ExecuteAndUndo_WithMultipleCommands_RestoresOriginalState()
        {
            // Arrange
            var composite = new CompositeCommand("Batch operation");
            var counter = 0;

            var mockCommand1 = new Mock<IUndoRedoCommand>();
            mockCommand1.Setup(c => c.Execute()).Callback(() => counter += 10);
            mockCommand1.Setup(c => c.Undo()).Callback(() => counter -= 10);

            var mockCommand2 = new Mock<IUndoRedoCommand>();
            mockCommand2.Setup(c => c.Execute()).Callback(() => counter += 20);
            mockCommand2.Setup(c => c.Undo()).Callback(() => counter -= 20);

            composite.AddCommand(mockCommand1.Object);
            composite.AddCommand(mockCommand2.Object);

            // Act - Execute then Undo
            composite.Execute();
            var afterExecute = counter;
            composite.Undo();
            var afterUndo = counter;

            // Assert
            afterExecute.Should().Be(30);
            afterUndo.Should().Be(0);
        }

        [Fact]
        public void CommandCount_AfterAddingCommands_ReflectsCorrectCount()
        {
            // Arrange
            var composite = new CompositeCommand("Batch");

            // Act & Assert - Start with 0
            composite.CommandCount.Should().Be(0);

            // Add 1 command
            composite.AddCommand(new Mock<IUndoRedoCommand>().Object);
            composite.CommandCount.Should().Be(1);

            // Add 2 more commands
            composite.AddCommand(new Mock<IUndoRedoCommand>().Object);
            composite.AddCommand(new Mock<IUndoRedoCommand>().Object);
            composite.CommandCount.Should().Be(3);
        }

        #endregion
    }
}
