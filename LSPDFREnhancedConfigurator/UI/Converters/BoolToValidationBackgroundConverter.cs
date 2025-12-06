using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LSPDFREnhancedConfigurator.UI.Converters
{
    /// <summary>
    /// Converts boolean validation state to background color
    /// True (valid) = Transparent
    /// False (invalid) = Orange warning background (matches Ranks view)
    /// </summary>
    public class BoolToValidationBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If valid (true), transparent background
                // If invalid (false), solid orange warning background (matches RanksView warning color)
                return boolValue
                    ? Brushes.Transparent
                    : new SolidColorBrush(Color.FromArgb(255, 255, 159, 64)); // Solid orange - matches Ranks view
            }

            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
