namespace LSPDFREnhancedConfigurator.Commands
{
    /// <summary>
    /// Interface for undoable commands in the Command Pattern.
    /// Commands encapsulate both the action and its reversal.
    /// </summary>
    public interface IUndoRedoCommand
    {
        /// <summary>
        /// Executes the command, applying the changes.
        /// </summary>
        void Execute();

        /// <summary>
        /// Undoes the command, reverting the changes.
        /// </summary>
        void Undo();

        /// <summary>
        /// Gets a human-readable description of the command for debugging and UI display.
        /// </summary>
        string Description { get; }
    }
}
