using System;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands
{
    /// <summary>
    /// Tests for PropertyChangeCommand generic command
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class PropertyChangeCommandTests
    {
        #region Test Class

        private class TestObject
        {
            public string Name { get; set; } = "Initial";
            public int Value { get; set; } = 100;
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_CreatesCommand()
        {
            // Arrange
            var testObj = new TestObject();

            // Act
            var command = new PropertyChangeCommand<string>(
                value => testObj.Name = value,
                "Old Name",
                "New Name",
                "Name",
                "Test Object");

            // Assert
            command.Should().NotBeNull();
            command.Description.Should().Contain("Name");
            command.Description.Should().Contain("Old Name");
            command.Description.Should().Contain("New Name");
        }

        [Fact]
        public void Constructor_WithNullSetter_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new PropertyChangeCommand<string>(
                null!,
                "Old",
                "New",
                "Property",
                "Target");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("setter");
        }

        [Fact]
        public void Description_ContainsPropertyName()
        {
            // Arrange
            var testObj = new TestObject();
            var command = new PropertyChangeCommand<int>(
                value => testObj.Value = value,
                100,
                200,
                "Value",
                "Test");

            // Assert
            command.Description.Should().Contain("Value");
        }

        [Fact]
        public void Description_ContainsOldAndNewValues()
        {
            // Arrange
            var testObj = new TestObject();
            var command = new PropertyChangeCommand<int>(
                value => testObj.Value = value,
                100,
                200,
                "Score",
                "Player");

            // Assert
            command.Description.Should().Contain("100");
            command.Description.Should().Contain("200");
        }

        #endregion

        #region Execute Tests

        [Fact]
        public void Execute_SetsPropertyToNewValue()
        {
            // Arrange
            var testObj = new TestObject { Name = "Initial" };
            var command = new PropertyChangeCommand<string>(
                value => testObj.Name = value,
                "Initial",
                "Updated",
                "Name",
                "Test Object");

            // Act
            command.Execute();

            // Assert
            testObj.Name.Should().Be("Updated");
        }

        [Fact]
        public void Execute_WithIntProperty_SetsCorrectValue()
        {
            // Arrange
            var testObj = new TestObject { Value = 100 };
            var command = new PropertyChangeCommand<int>(
                value => testObj.Value = value,
                100,
                500,
                "Value",
                "Test Object");

            // Act
            command.Execute();

            // Assert
            testObj.Value.Should().Be(500);
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void Undo_RestoresPropertyToOldValue()
        {
            // Arrange
            var testObj = new TestObject { Name = "Updated" };
            var command = new PropertyChangeCommand<string>(
                value => testObj.Name = value,
                "Original",
                "Updated",
                "Name",
                "Test Object");

            // Act
            command.Undo();

            // Assert
            testObj.Name.Should().Be("Original");
        }

        [Fact]
        public void Undo_WithIntProperty_RestoresCorrectValue()
        {
            // Arrange
            var testObj = new TestObject { Value = 500 };
            var command = new PropertyChangeCommand<int>(
                value => testObj.Value = value,
                100,
                500,
                "Value",
                "Test Object");

            // Act
            command.Undo();

            // Assert
            testObj.Value.Should().Be(100);
        }

        #endregion

        #region Execute/Undo Cycle Tests

        [Fact]
        public void ExecuteAndUndo_CycleWorks()
        {
            // Arrange
            var testObj = new TestObject { Name = "Start" };
            var command = new PropertyChangeCommand<string>(
                value => testObj.Name = value,
                "Start",
                "End",
                "Name",
                "Test");

            // Act & Assert
            command.Execute();
            testObj.Name.Should().Be("End");

            command.Undo();
            testObj.Name.Should().Be("Start");

            command.Execute();
            testObj.Name.Should().Be("End");
        }

        [Fact]
        public void MultipleUndoRedo_PreservesState()
        {
            // Arrange
            var testObj = new TestObject { Value = 100 };
            var command = new PropertyChangeCommand<int>(
                value => testObj.Value = value,
                100,
                200,
                "Value",
                "Test");

            // Act & Assert - Multiple cycles
            for (int i = 0; i < 3; i++)
            {
                command.Execute();
                testObj.Value.Should().Be(200, $"Execute cycle {i}");

                command.Undo();
                testObj.Value.Should().Be(100, $"Undo cycle {i}");
            }
        }

        #endregion
    }
}
