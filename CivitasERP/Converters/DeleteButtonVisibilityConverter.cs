using CivitasERP.Models;
using CivitasERP.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CivitasERP.Converters
{
    public class DeleteButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value vendrá siendo el ID del Empleado de la fila
            if (value is int idEmpleado && idEmpleado == Variables.IdAdmin)
                return Visibility.Collapsed;    // es el admin actual → ocultar
            return Visibility.Visible;          // resto de filas → mostrar
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
