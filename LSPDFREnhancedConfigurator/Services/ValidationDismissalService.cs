using System.Security.Cryptography;
using System.Text;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Service for managing dismissal of validation warnings and advisories
    /// </summary>
    public class ValidationDismissalService
    {
        private readonly SettingsManager _settingsManager;
        private HashSet<string> _dismissedKeys;

        public ValidationDismissalService(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _dismissedKeys = _settingsManager.GetDismissedValidations().ToHashSet();
        }

        /// <summary>
        /// Dismiss a validation issue (warnings and advisories only)
        /// </summary>
        public void Dismiss(ValidationIssue issue)
        {
            if (issue.RankId == null) return;

            // Create unique key using message hash for stability
            var messageHash = ComputeHash(issue.Message);
            var key = $"{issue.RankId}|{issue.Category}|{issue.ItemName ?? ""}|{messageHash}";

            if (!_dismissedKeys.Contains(key))
            {
                _dismissedKeys.Add(key);
                var dismissals = _dismissedKeys.ToList();
                _settingsManager.SetDismissedValidations(dismissals);
            }
        }

        /// <summary>
        /// Dismiss a validation issue by components
        /// </summary>
        public void Dismiss(string rankId, string category, string itemName, string message)
        {
            var messageHash = ComputeHash(message);
            var key = $"{rankId}|{category}|{itemName}|{messageHash}";

            if (!_dismissedKeys.Contains(key))
            {
                _dismissedKeys.Add(key);
                var dismissals = _dismissedKeys.ToList();
                _settingsManager.SetDismissedValidations(dismissals);
            }
        }

        /// <summary>
        /// Check if a validation issue has been dismissed
        /// </summary>
        public bool IsDismissed(ValidationIssue issue)
        {
            if (issue.RankId == null) return false;

            var messageHash = ComputeHash(issue.Message);
            var key = $"{issue.RankId}|{issue.Category}|{issue.ItemName ?? ""}|{messageHash}";
            return _dismissedKeys.Contains(key);
        }

        /// <summary>
        /// Clear all dismissed validations
        /// </summary>
        public void ClearAll()
        {
            _dismissedKeys.Clear();
            _settingsManager.SetDismissedValidations(new List<string>());
        }

        /// <summary>
        /// Compute SHA256 hash of a message for stable identification
        /// </summary>
        private static string ComputeHash(string message)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash)[..16]; // Use first 16 chars for brevity
            }
        }
    }
}
