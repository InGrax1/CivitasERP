using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
namespace CivitasERP.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ok = value is bool b && b;
            return ok
              ? (Brush)Application.Current.Resources["buttonColor1"]  // verde
              : (Brush)Application.Current.Resources["buttonColor2"]; // rojo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => throw new NotSupportedException();
    }
}
