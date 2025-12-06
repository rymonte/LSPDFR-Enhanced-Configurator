using System;

namespace LSPDFREnhancedConfigurator.Commands
{
    /// <summary>
    /// Generic command for property changes that stores old and new values.
    /// Used for undoable property modifications (XP, Salary, Name, etc.).
    /// </summary>
    /// <typeparam name="T">Type of the property value</typeparam>
    public class PropertyChangeCommand<T> : IUndoRedoCommand
    {
        private readonly Action<T> _setter;
        private readonly T _oldValue;
        private readonly T _newValue;
        private readonly string _propertyName;
        private readonly string _targetDescription;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new property change command.
        /// </summary>
        /// <param name="setter">Action to set the property value</param>
        /// <param name="oldValue">Value before the change</param>
        /// <param name="newValue">Value after the change</param>
        /// <param name="propertyName">Name of the property being changed</param>
        /// <param name="targetDescription">Description of the target object (e.g., rank name)</param>
        public PropertyChangeCommand(
            Action<T> setter,
            T oldValue,
            T newValue,
            string propertyName,
            string targetDescription)
        {
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _oldValue = oldValue;
            _newValue = newValue;
            _propertyName = propertyName;
            _targetDescription = targetDescription;

            Description = $"Change {propertyName} of '{targetDescription}' from {oldValue} to {newValue}";
        }

        /// <summary>
        /// Executes the command by setting the property to the new value.
        /// </summary>
        public void Execute()
        {
            _setter(_newValue);
        }

        /// <summary>
        /// Undoes the command by setting the property back to the old value.
        /// </summary>
        public void Undo()
        {
            _setter(_oldValue);
        }
    }
}
