using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LSPDFREnhancedConfigurator.UI.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Check parameter for color mode
                if (parameter is string mode && mode == "validation")
                {
                    // Validation mode: true = white, false = orange warning
                    return boolValue
                        ? new SolidColorBrush(Color.Parse("#E9EAEA")) // TextWhite
                        : new SolidColorBrush(Color.Parse("#FF9F40")); // WarningOrange
                }
                else
                {
                    // Default mode: true = white, false = gray
                    return boolValue
                        ? new SolidColorBrush(Color.Parse("#E9EAEA")) // TextWhite
                        : new SolidColorBrush(Color.Parse("#B4B4B4")); // TextGray
                }
            }

            return new SolidColorBrush(Color.Parse("#E9EAEA")); // Default to white
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
