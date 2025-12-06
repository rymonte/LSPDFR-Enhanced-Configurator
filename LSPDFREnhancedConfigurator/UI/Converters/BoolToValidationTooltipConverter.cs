using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LSPDFREnhancedConfigurator.UI.Converters
{
    /// <summary>
    /// Converts boolean validation state to tooltip message
    /// True (valid) = null (no tooltip)
    /// False (invalid) = Warning message
    /// </summary>
    public class BoolToValidationTooltipConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If valid (true), no tooltip
                // If invalid (false), show warning message
                return boolValue
                    ? null
                    : "⚠️ Station not found in game data";
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
