namespace LSPDFREnhancedConfigurator.Services.Validation
{
    /// <summary>
    /// Defines the context in which validation is performed.
    /// Different contexts may trigger different subsets of validation rules for performance.
    /// </summary>
    public enum ValidationContext
    {
        /// <summary>
        /// Full validation including all rules.
        /// Used at application startup and before saving configurations.
        /// All error, warning, and advisory rules are executed.
        /// </summary>
        Full,

        /// <summary>
        /// Real-time validation during editing for immediate user feedback.
        /// Only fast, focused rules are executed (e.g., field constraints, basic checks).
        /// Expensive reference checks and progression analysis may be skipped.
        /// </summary>
        RealTime,

        /// <summary>
        /// Validation before XML generation and file save.
        /// Only critical rules that could cause XML parsing errors or game crashes.
        /// Warnings and advisories may be skipped to allow generation with known issues.
        /// </summary>
        PreGenerate,

        /// <summary>
        /// Startup validation after loading from disk.
        /// Comprehensive validation to detect corruption or manual file edits.
        /// Includes all error and warning rules, plus reference validation.
        /// </summary>
        Startup,

        /// <summary>
        /// Advisory checks only - non-blocking suggestions.
        /// Used for optional feedback like "previous rank had more stations".
        /// No errors or warnings, only helpful hints.
        /// </summary>
        AdvisoryOnly
    }
}
