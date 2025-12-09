namespace LSPDFREnhancedConfigurator.Services.Validation
{
    /// <summary>
    /// Unified severity level for all validation issues.
    /// Replaces both the old ValidationSeverity and RankValidationSeverity enums.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// No issues detected (used for tree items without validation problems)
        /// </summary>
        None = 0,

        /// <summary>
        /// Informational advisory (e.g., "previous rank had more stations").
        /// Non-blocking, UI hint only. User can proceed without fixing.
        /// </summary>
        Advisory = 1,

        /// <summary>
        /// Warning that should be reviewed (e.g., "salary decreased", "reference not found").
        /// Can proceed but user should be aware. May cause issues in-game.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Critical error that blocks generation (e.g., "negative XP", "duplicate names").
        /// Must be fixed before proceeding. Will cause application or game issues.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Successful validation (used for directory validation and success states).
        /// Indicates validation passed with no issues.
        /// </summary>
        Success = -1
    }
}
