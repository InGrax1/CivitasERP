using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CivitasERP.AdminViews
{
    /// <summary>
    /// Lógica de interacción para EditAdminPage.xaml
    /// </summary>
    public partial class EditAdminPage : Page
    {
        public EditAdminPage()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the previous page
            NavigationService?.GoBack();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Empleado guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            // Navigate back to the previous page after saving
            NavigationService?.GoBack();
        }

    }
}
