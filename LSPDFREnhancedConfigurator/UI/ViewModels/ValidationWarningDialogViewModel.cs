using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class ValidationWarningDialogViewModel : ViewModelBase
    {
        private string _issuesText = string.Empty;
        private bool _viewInUI;

        public ValidationWarningDialogViewModel(ValidationResult result)
        {
            ContinueAnywayCommand = new RelayCommand(OnContinueAnyway);
            ViewAndFixCommand = new RelayCommand(OnViewAndFix);

            PopulateIssues(result);
        }

        #region Properties

        public string IssuesText
        {
            get => _issuesText;
            set => SetProperty(ref _issuesText, value);
        }

        public bool ViewInUI
        {
            get => _viewInUI;
            private set => SetProperty(ref _viewInUI, value);
        }

        #endregion

        #region Commands

        public ICommand ContinueAnywayCommand { get; }
        public ICommand ViewAndFixCommand { get; }

        #endregion

        #region Command Handlers

        private void OnContinueAnyway()
        {
            ViewInUI = false;
        }

        private void OnViewAndFix()
        {
            ViewInUI = true;
        }

        #endregion

        #region Helper Methods

        private void PopulateIssues(ValidationResult result)
        {
            var text = "";

            if (result.HasErrors)
            {
                text += $"ERRORS ({result.ErrorCount}):\n";
                text += new string('=', 60) + "\n\n";
                text += FormatIssuesByCategory(result.Errors.ToList(), "❌");
                text += "\n";
            }

            if (result.HasWarnings)
            {
                text += $"WARNINGS ({result.WarningCount}):\n";
                text += new string('=', 60) + "\n\n";
                text += FormatIssuesByCategory(result.Warnings.ToList(), "⚠️");
            }

            if (result.HasErrors)
            {
                text += "\n" + new string('=', 60) + "\n";
                text += "⚠️  ERRORS must be fixed before generating Ranks.xml\n";
                text += "⚠️  WARNINGS are optional but recommended to review\n";
            }

            IssuesText = text;
        }

        private string FormatIssuesByCategory(List<ValidationIssue> issues, string prefix)
        {
            // Group issues by category
            var categorized = issues.GroupBy(i => i.Category).OrderBy(g => g.Key == "Rank" ? "0" : g.Key);

            var result = "";
            foreach (var group in categorized)
            {
                result += $"[{group.Key.ToUpper()}]\n";
                foreach (var issue in group)
                {
                    result += $"{prefix} - {issue.Message}\n";
                }
                result += "\n";
            }

            return result;
        }

        #endregion
    }
}
