using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class ValidationWarningDialogViewModel : ViewModelBase
    {
        private string _issuesText = string.Empty;
        private bool _viewInUI;

        public ValidationWarningDialogViewModel(ValidationReport report)
        {
            ContinueAnywayCommand = new RelayCommand(OnContinueAnyway);
            ViewAndFixCommand = new RelayCommand(OnViewAndFix);

            PopulateIssues(report);
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

        private void PopulateIssues(ValidationReport report)
        {
            var text = "";

            if (report.HasErrors)
            {
                text += $"ERRORS ({report.Errors.Count}):\n";
                text += new string('=', 60) + "\n\n";
                text += FormatIssuesByCategory(report.Errors, "❌");
                text += "\n";
            }

            if (report.HasWarnings)
            {
                text += $"WARNINGS ({report.Warnings.Count}):\n";
                text += new string('=', 60) + "\n\n";
                text += FormatIssuesByCategory(report.Warnings, "⚠️");
            }

            if (report.HasErrors)
            {
                text += "\n" + new string('=', 60) + "\n";
                text += "⚠️  ERRORS must be fixed before generating Ranks.xml\n";
                text += "⚠️  WARNINGS are optional but recommended to review\n";
            }

            IssuesText = text;
        }

        private string FormatIssuesByCategory(List<string> issues, string prefix)
        {
            // Categorize issues
            var categorized = new Dictionary<string, List<string>>();

            foreach (var issue in issues)
            {
                string category = "GENERAL";

                // Extract category from issue text
                if (issue.Contains("Outfit") || issue.Contains("outfit"))
                {
                    category = "OUTFITS";
                }
                else if (issue.Contains("Vehicle") || issue.Contains("vehicle"))
                {
                    category = "VEHICLES";
                }
                else if (issue.Contains("Station") || issue.Contains("station"))
                {
                    category = "STATIONS";
                }
                else if (issue.Contains("Rank") || issue.Contains("XP") || issue.Contains("salary"))
                {
                    category = "RANKS";
                }

                if (!categorized.ContainsKey(category))
                {
                    categorized[category] = new List<string>();
                }

                categorized[category].Add(issue);
            }

            // Sort categories: RANKS first, then alphabetically
            var sortedCategories = categorized.Keys.OrderBy(k => k == "RANKS" ? "0" : k).ToList();

            var result = "";
            foreach (var category in sortedCategories)
            {
                result += $"[{category}]\n";
                foreach (var issue in categorized[category])
                {
                    result += $"{prefix} - {issue}\n";
                }
                result += "\n";
            }

            return result;
        }

        #endregion
    }
}
