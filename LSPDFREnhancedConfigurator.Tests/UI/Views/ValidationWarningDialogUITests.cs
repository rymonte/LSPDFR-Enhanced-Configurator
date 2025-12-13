using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class ValidationWarningDialogUITests
{
    private ValidationResult CreateTestValidationResult()
    {
        var result = new ValidationResult();
        result.Issues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Error,
            Message = "Test error 1",
            Category = "Test"
        });
        result.Issues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Error,
            Message = "Test error 2",
            Category = "Test"
        });
        result.Issues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Warning,
            Message = "Test warning 1",
            Category = "Test"
        });
        return result;
    }

    [AvaloniaFact]
    public void ValidationWarningDialog_Opens_Successfully()
    {
        // Arrange
        var result = CreateTestValidationResult();
        var viewModel = new ValidationWarningDialogViewModel(result);

        // Act
        var dialog = new ValidationWarningDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
        dialog.DataContext.Should().Be(viewModel);
    }

    [AvaloniaFact]
    public void ValidationWarningDialog_ViewModel_PopulatesIssues()
    {
        // Arrange
        var result = CreateTestValidationResult();
        var viewModel = new ValidationWarningDialogViewModel(result);

        // Act
        var dialog = new ValidationWarningDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.IssuesText.Should().NotBeEmpty("validation result should populate issues text");
    }

    [AvaloniaFact]
    public void ValidationWarningDialog_Commands_ExistAndInitialize()
    {
        // Arrange
        var result = CreateTestValidationResult();
        var viewModel = new ValidationWarningDialogViewModel(result);

        // Act
        var dialog = new ValidationWarningDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.ContinueAnywayCommand.Should().NotBeNull();
        viewModel.ViewAndFixCommand.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void ValidationWarningDialog_WithNoErrors_CreatesDialog()
    {
        // Arrange
        var result = new ValidationResult();
        var viewModel = new ValidationWarningDialogViewModel(result);

        // Act
        var dialog = new ValidationWarningDialog
        {
            DataContext = viewModel
        };

        // Assert
        dialog.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void ValidationWarningDialog_ViewInUI_InitiallyFalse()
    {
        // Arrange
        var result = CreateTestValidationResult();
        var viewModel = new ValidationWarningDialogViewModel(result);

        // Act
        var dialog = new ValidationWarningDialog
        {
            DataContext = viewModel
        };

        // Assert
        viewModel.ViewInUI.Should().BeFalse("ViewInUI should be false initially");
    }
}
