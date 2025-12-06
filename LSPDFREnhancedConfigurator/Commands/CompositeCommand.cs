using System;
using System.Collections.Generic;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Commands
{
    /// <summary>
    /// Command that batches multiple sub-commands together to be executed/undone as a single operation.
    /// Used to group related changes (e.g., changing multiple properties at once).
    /// </summary>
    public class CompositeCommand : IUndoRedoCommand
    {
        private readonly List<IUndoRedoCommand> _commands = new List<IUndoRedoCommand>();
        private readonly string _description;

        /// <summary>
        /// Gets a human-readable description of the composite command.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Gets the number of sub-commands in this composite.
        /// </summary>
        public int CommandCount => _commands.Count;

        /// <summary>
        /// Creates a new composite command with the specified description.
        /// </summary>
        /// <param name="description">Description of the batch operation</param>
        public CompositeCommand(string description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        /// Adds a sub-command to this composite.
        /// </summary>
        /// <param name="command">The command to add</param>
        public void AddCommand(IUndoRedoCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _commands.Add(command);
        }

        /// <summary>
        /// Executes all sub-commands in order.
        /// </summary>
        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        /// <summary>
        /// Undoes all sub-commands in reverse order.
        /// </summary>
        public void Undo()
        {
            // Undo in reverse order to properly restore state
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }

        /// <summary>
        /// Gets descriptions of all sub-commands.
        /// </summary>
        /// <returns>List of command descriptions</returns>
        public List<string> GetSubCommandDescriptions()
        {
            return _commands.Select(c => c.Description).ToList();
        }
    }
}
