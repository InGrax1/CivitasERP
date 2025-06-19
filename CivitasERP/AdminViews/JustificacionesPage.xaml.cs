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
    /// Lógica de interacción para JustificacionesPage.xaml
    /// </summary>
    public partial class JustificacionesPage : Page
    {
        public JustificacionesPage()
        {
            InitializeComponent();
        }
        private void BtnGuardarJustificacion_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Justificación guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
    }
}
