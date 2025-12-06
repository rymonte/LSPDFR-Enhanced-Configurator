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
    }
}
