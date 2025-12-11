using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI;

/// <summary>
/// Base class for Avalonia Headless UI tests.
/// Provides common setup and utilities for testing UI components.
/// </summary>
public abstract class HeadlessTestBase
{
    /// <summary>
    /// Runs an action on the UI thread and waits for completion.
    /// </summary>
    protected async Task RunOnUIThread(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }

    /// <summary>
    /// Runs a func on the UI thread and returns the result.
    /// </summary>
    protected async Task<T> RunOnUIThread<T>(Func<T> func)
    {
        return await Dispatcher.UIThread.InvokeAsync(func);
    }

    /// <summary>
    /// Creates a mock DataLoadingService with default test data.
    /// </summary>
    protected Mock<DataLoadingService> CreateMockDataService()
    {
        return new MockServiceBuilder().BuildMock();
    }

    /// <summary>
    /// Creates a test SettingsManager with a temporary file path.
    /// </summary>
    protected SettingsManager CreateTestSettingsManager()
    {
        var tempPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"test_settings_{Guid.NewGuid()}.ini"
        );
        return new SettingsManager(tempPath);
    }

    /// <summary>
    /// Waits for a window to be shown and fully rendered.
    /// </summary>
    protected async Task WaitForWindow(Window window, int timeoutMs = 1000)
    {
        var startTime = DateTime.UtcNow;
        while (!window.IsVisible && (DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// Finds a control by name within a parent control.
    /// </summary>
    protected T? FindControl<T>(Control parent, string name) where T : Control
    {
        return parent.FindControl<T>(name);
    }

    /// <summary>
    /// Simulates clicking a button.
    /// </summary>
    protected async Task ClickButton(Button button)
    {
        await RunOnUIThread(() =>
        {
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        });
    }

    /// <summary>
    /// Sets text in a TextBox.
    /// </summary>
    protected async Task SetTextBoxText(TextBox textBox, string text)
    {
        await RunOnUIThread(() =>
        {
            textBox.Text = text;
        });
    }

    /// <summary>
    /// Waits for a condition to be true with timeout.
    /// </summary>
    protected async Task<bool> WaitForCondition(Func<bool> condition, int timeoutMs = 2000)
    {
        var startTime = DateTime.UtcNow;
        while (!condition() && (DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            await Task.Delay(50);
        }
        return condition();
    }
}

/// <summary>
/// Application builder for headless tests.
/// Configures Avalonia for testing without a real window system.
/// </summary>
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false
            });
    }
}

/// <summary>
/// Test application class for Avalonia Headless.
/// Minimal implementation without the complex startup logic.
/// </summary>
public class TestApplication : Application
{
    public override void Initialize()
    {
        // Minimal initialization - don't load XAML
        // This avoids the complex startup logic in the real App
    }
}
