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
using System.Windows.Shapes;
using System.Data.SqlClient;
using CivitasERP.ViewModels;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevaObraPage.xaml
    /// </summary>
    public partial class NuevaObraPage : Window
    {
        public NuevaObraPage()
        {
            InitializeComponent();
        }
        private void btnAgregarObra_Click(object sender, EventArgs e)
        {
            
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
