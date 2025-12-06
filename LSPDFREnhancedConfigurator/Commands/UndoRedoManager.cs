using System;
using System.Collections.Generic;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Commands
{
    /// <summary>
    /// Manages undo/redo command stacks for a ViewModel.
    /// Each ViewModel instance has its own UndoRedoManager for independent history tracking.
    /// </summary>
    public class UndoRedoManager
    {
        private readonly Stack<IUndoRedoCommand> _undoStack = new Stack<IUndoRedoCommand>();
        private readonly Stack<IUndoRedoCommand> _redoStack = new Stack<IUndoRedoCommand>();
        private readonly int _maxStackSize;

        /// <summary>
        /// Fired when the undo/redo stacks change (for UI updates).
        /// </summary>
        public event EventHandler? StacksChanged;

        /// <summary>
        /// Gets whether undo is available.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Gets whether redo is available.
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Gets the current undo stack size.
        /// </summary>
        public int UndoStackSize => _undoStack.Count;

        /// <summary>
        /// Gets the current redo stack size.
        /// </summary>
        public int RedoStackSize => _redoStack.Count;

        /// <summary>
        /// Creates a new UndoRedoManager with the specified maximum stack size.
        /// </summary>
        /// <param name="maxStackSize">Maximum number of operations to keep in undo stack (default: 50)</param>
        public UndoRedoManager(int maxStackSize = 50)
        {
            _maxStackSize = maxStackSize;
        }

        /// <summary>
        /// Executes a command and adds it to the undo stack.
        /// Clears the redo stack as new actions invalidate forward history.
        /// </summary>
        /// <param name="command">The command to execute</param>
        public void ExecuteCommand(IUndoRedoCommand command)
        {
            // Execute the command
            command.Execute();

            // Add to undo stack
            _undoStack.Push(command);

            // Enforce stack size limit
            if (_undoStack.Count > _maxStackSize)
            {
                // Remove oldest command (at the bottom of the stack)
                var tempStack = new Stack<IUndoRedoCommand>(_undoStack.Reverse().Skip(1).Reverse());
                _undoStack.Clear();
                foreach (var cmd in tempStack.Reverse())
                {
                    _undoStack.Push(cmd);
                }
            }

            // Clear redo stack (new action invalidates forward history)
            _redoStack.Clear();

            // Notify listeners that stacks have changed
            RaiseStacksChanged();
        }

        /// <summary>
        /// Undoes the most recent command.
        /// </summary>
        /// <returns>True if undo was performed, false if undo stack is empty</returns>
        public bool Undo()
        {
            if (!CanUndo)
                return false;

            // Pop command from undo stack
            var command = _undoStack.Pop();

            // Undo the command
            command.Undo();

            // Add to redo stack
            _redoStack.Push(command);

            // Notify listeners that stacks have changed
            RaiseStacksChanged();

            return true;
        }

        /// <summary>
        /// Redoes the most recently undone command.
        /// </summary>
        /// <returns>True if redo was performed, false if redo stack is empty</returns>
        public bool Redo()
        {
            if (!CanRedo)
                return false;

            // Pop command from redo stack
            var command = _redoStack.Pop();

            // Re-execute the command
            command.Execute();

            // Add back to undo stack
            _undoStack.Push(command);

            // Notify listeners that stacks have changed
            RaiseStacksChanged();

            return true;
        }

        /// <summary>
        /// Clears both undo and redo stacks.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            RaiseStacksChanged();
        }

        /// <summary>
        /// Gets a description of the next command that would be undone.
        /// </summary>
        /// <returns>Command description, or null if undo stack is empty</returns>
        public string? GetUndoDescription()
        {
            return CanUndo ? _undoStack.Peek().Description : null;
        }

        /// <summary>
        /// Gets a description of the next command that would be redone.
        /// </summary>
        /// <returns>Command description, or null if redo stack is empty</returns>
        public string? GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : null;
        }

        private void RaiseStacksChanged()
        {
            StacksChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
