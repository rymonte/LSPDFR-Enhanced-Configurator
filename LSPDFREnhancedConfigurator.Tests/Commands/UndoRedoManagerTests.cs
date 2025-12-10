using System;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    /// <summary>
    /// Tests for UndoRedoManager covering command execution, undo/redo operations, and stack management
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class UndoRedoManagerTests
    {
        #region Test Command Implementation

        /// <summary>
        /// Simple test command that tracks execute/undo calls
        /// </summary>
        private class TestCommand : IUndoRedoCommand
        {
            public int ExecuteCount { get; private set; }
            public int UndoCount { get; private set; }
            public string Description { get; set; }

            public TestCommand(string description = "Test Command")
            {
                Description = description;
            }

            public void Execute()
            {
                ExecuteCount++;
            }

            public void Undo()
            {
                UndoCount++;
            }
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Constructor_WithDefaultStackSize_InitializesCorrectly()
        {
            // Act
            var manager = new UndoRedoManager();

            // Assert
            manager.CanUndo.Should().BeFalse();
            manager.CanRedo.Should().BeFalse();
            manager.UndoStackSize.Should().Be(0);
            manager.RedoStackSize.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithCustomStackSize_InitializesCorrectly()
        {
            // Act
            var manager = new UndoRedoManager(maxStackSize: 100);

            // Assert
            manager.CanUndo.Should().BeFalse();
            manager.CanRedo.Should().BeFalse();
        }

        #endregion

        #region ExecuteCommand Tests

        [Fact]
        public void ExecuteCommand_ExecutesCommandAndAddsToUndoStack()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();

            // Act
            manager.ExecuteCommand(command);

            // Assert
            command.ExecuteCount.Should().Be(1, "command should be executed");
            manager.CanUndo.Should().BeTrue();
            manager.UndoStackSize.Should().Be(1);
        }

        [Fact]
        public void ExecuteCommand_ClearsRedoStack()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command1 = new TestCommand("Command 1");
            var command2 = new TestCommand("Command 2");

            manager.ExecuteCommand(command1);
            manager.Undo();
            manager.CanRedo.Should().BeTrue("redo should be available after undo");

            // Act
            manager.ExecuteCommand(command2);

            // Assert
            manager.CanRedo.Should().BeFalse("executing new command should clear redo stack");
            manager.RedoStackSize.Should().Be(0);
        }

        [Fact]
        public void ExecuteCommand_RaisesStacksChangedEvent()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            var eventRaised = false;
            manager.StacksChanged += (s, e) => eventRaised = true;

            // Act
            manager.ExecuteCommand(command);

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void ExecuteCommand_EnforcesMaxStackSize()
        {
            // Arrange
            var manager = new UndoRedoManager(maxStackSize: 3);

            // Act - Add 5 commands (exceeds max of 3)
            for (int i = 0; i < 5; i++)
            {
                manager.ExecuteCommand(new TestCommand($"Command {i}"));
            }

            // Assert
            manager.UndoStackSize.Should().Be(3, "stack size should be limited to max");
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void Undo_WithEmptyStack_ReturnsFalse()
        {
            // Arrange
            var manager = new UndoRedoManager();

            // Act
            var result = manager.Undo();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Undo_UndoesLastCommand()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);

            // Act
            var result = manager.Undo();

            // Assert
            result.Should().BeTrue();
            command.UndoCount.Should().Be(1, "command should be undone");
        }

        [Fact]
        public void Undo_MovesCommandToRedoStack()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);

            // Act
            manager.Undo();

            // Assert
            manager.CanRedo.Should().BeTrue();
            manager.RedoStackSize.Should().Be(1);
            manager.CanUndo.Should().BeFalse();
            manager.UndoStackSize.Should().Be(0);
        }

        [Fact]
        public void Undo_RaisesStacksChangedEvent()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);

            var eventRaised = false;
            manager.StacksChanged += (s, e) => eventRaised = true;

            // Act
            manager.Undo();

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void Undo_MultipleCommands_UndoesInReverseOrder()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command1 = new TestCommand("First");
            var command2 = new TestCommand("Second");
            var command3 = new TestCommand("Third");

            manager.ExecuteCommand(command1);
            manager.ExecuteCommand(command2);
            manager.ExecuteCommand(command3);

            // Act
            manager.Undo(); // Should undo command3
            manager.Undo(); // Should undo command2
            manager.Undo(); // Should undo command1

            // Assert
            command3.UndoCount.Should().Be(1);
            command2.UndoCount.Should().Be(1);
            command1.UndoCount.Should().Be(1);
            manager.CanUndo.Should().BeFalse();
        }

        #endregion

        #region Redo Tests

        [Fact]
        public void Redo_WithEmptyStack_ReturnsFalse()
        {
            // Arrange
            var manager = new UndoRedoManager();

            // Act
            var result = manager.Redo();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Redo_ReExecutesLastUndoneCommand()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);
            manager.Undo();

            // Act
            var result = manager.Redo();

            // Assert
            result.Should().BeTrue();
            command.ExecuteCount.Should().Be(2, "command should be executed again");
        }

        [Fact]
        public void Redo_MovesCommandBackToUndoStack()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);
            manager.Undo();

            // Act
            manager.Redo();

            // Assert
            manager.CanUndo.Should().BeTrue();
            manager.UndoStackSize.Should().Be(1);
            manager.CanRedo.Should().BeFalse();
            manager.RedoStackSize.Should().Be(0);
        }

        [Fact]
        public void Redo_RaisesStacksChangedEvent()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand();
            manager.ExecuteCommand(command);
            manager.Undo();

            var eventRaised = false;
            manager.StacksChanged += (s, e) => eventRaised = true;

            // Act
            manager.Redo();

            // Assert
            eventRaised.Should().BeTrue();
        }

        #endregion

        #region Clear Tests

        [Fact]
        public void Clear_EmptiesBothStacks()
        {
            // Arrange
            var manager = new UndoRedoManager();
            manager.ExecuteCommand(new TestCommand());
            manager.ExecuteCommand(new TestCommand());
            manager.Undo();

            // Act
            manager.Clear();

            // Assert
            manager.CanUndo.Should().BeFalse();
            manager.CanRedo.Should().BeFalse();
            manager.UndoStackSize.Should().Be(0);
            manager.RedoStackSize.Should().Be(0);
        }

        [Fact]
        public void Clear_RaisesStacksChangedEvent()
        {
            // Arrange
            var manager = new UndoRedoManager();
            manager.ExecuteCommand(new TestCommand());

            var eventRaised = false;
            manager.StacksChanged += (s, e) => eventRaised = true;

            // Act
            manager.Clear();

            // Assert
            eventRaised.Should().BeTrue();
        }

        #endregion

        #region Description Tests

        [Fact]
        public void GetUndoDescription_WithCommands_ReturnsTopCommandDescription()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand("My Test Command");
            manager.ExecuteCommand(command);

            // Act
            var description = manager.GetUndoDescription();

            // Assert
            description.Should().Be("My Test Command");
        }

        [Fact]
        public void GetUndoDescription_WithEmptyStack_ReturnsNull()
        {
            // Arrange
            var manager = new UndoRedoManager();

            // Act
            var description = manager.GetUndoDescription();

            // Assert
            description.Should().BeNull();
        }

        [Fact]
        public void GetRedoDescription_WithCommands_ReturnsTopCommandDescription()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command = new TestCommand("My Test Command");
            manager.ExecuteCommand(command);
            manager.Undo();

            // Act
            var description = manager.GetRedoDescription();

            // Assert
            description.Should().Be("My Test Command");
        }

        [Fact]
        public void GetRedoDescription_WithEmptyStack_ReturnsNull()
        {
            // Arrange
            var manager = new UndoRedoManager();

            // Act
            var description = manager.GetRedoDescription();

            // Assert
            description.Should().BeNull();
        }

        #endregion

        #region Integration Scenarios

        [Fact]
        public void Scenario_CompleteUndoRedoCycle_WorksCorrectly()
        {
            // Arrange
            var manager = new UndoRedoManager();
            var command1 = new TestCommand("First");
            var command2 = new TestCommand("Second");

            // Act & Assert
            manager.ExecuteCommand(command1);
            manager.ExecuteCommand(command2);
            manager.UndoStackSize.Should().Be(2);

            manager.Undo(); // Undo command2
            manager.UndoStackSize.Should().Be(1);
            manager.RedoStackSize.Should().Be(1);

            manager.Redo(); // Redo command2
            manager.UndoStackSize.Should().Be(2);
            manager.RedoStackSize.Should().Be(0);

            manager.Undo(); // Undo command2 again
            manager.Undo(); // Undo command1
            manager.UndoStackSize.Should().Be(0);
            manager.RedoStackSize.Should().Be(2);

            manager.Redo(); // Redo command1
            manager.Redo(); // Redo command2
            manager.UndoStackSize.Should().Be(2);
            manager.RedoStackSize.Should().Be(0);
        }

        #endregion
    }
}
