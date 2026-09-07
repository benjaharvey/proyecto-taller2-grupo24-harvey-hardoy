using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Presentacion;

public class SectionVisibilityConverter : IValueConverter
{
    public static readonly SectionVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentSection && parameter is string targetSection)
        {
            return currentSection.Equals(targetSection, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
