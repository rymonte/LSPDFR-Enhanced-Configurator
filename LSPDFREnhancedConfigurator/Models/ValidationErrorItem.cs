using System.Windows.Input;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Represents a single validation error that can be displayed in a table
    /// </summary>
    public class ValidationErrorItem
    {
        public string Type { get; set; } = string.Empty; // "Error", "Warning", or "Advisory"
        public string Severity { get; set; } = string.Empty; // "Vehicle", "Station", "Outfit", or "Rank"
        public string RankName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ICommand? RemoveCommand { get; set; }
        public ICommand? ShowCommand { get; set; }

        /// <summary>
        /// Rank ID for dismissal key (stable across renames)
        /// </summary>
        public string? RankId { get; set; }

        /// <summary>
        /// Command to dismiss this validation issue
        /// </summary>
        public ICommand? DismissCommand { get; set; }

        /// <summary>
        /// True if this validation has been dismissed by the user
        /// </summary>
        public bool IsDismissed { get; set; }

        /// <summary>
        /// True if this issue can be shown in the UI (has a specific location)
        /// </summary>
        public bool CanShow => ShowCommand != null;

        /// <summary>
        /// True if this issue can be automatically removed
        /// </summary>
        public bool CanRemove => RemoveCommand != null;

        /// <summary>
        /// True if this issue can be dismissed (warnings and advisories only, not errors)
        /// </summary>
        public bool CanDismiss => DismissCommand != null && !IsDismissed;

        /// <summary>
        /// True if no actions are available for this issue
        /// </summary>
        public bool NoActionsAvailable => !CanShow && !CanRemove && !CanDismiss;

        /// <summary>
        /// Text to display in the actions column
        /// </summary>
        public string ActionsText => NoActionsAvailable ? "No actions available" : string.Empty;
    }
}
