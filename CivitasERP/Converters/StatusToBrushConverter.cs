using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
namespace CivitasERP.Converters
{
    /// <summary>
    /// Clase donde se implementa la conversión de un estado booleano a un color de pincel (Brush).
    /// para indicar en nomina de manera grafica si un empleado tomo asistencia o no. 
    /// 
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ok = value is bool b && b;
            return ok
              ? (Brush)Application.Current.Resources["Color2"]  // verde
              : (Brush)Application.Current.Resources["Color3"]; // rojo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => throw new NotSupportedException();
    }
}
