    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
using System.Windows.Interop;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        public HomePage()
        {
            InitializeComponent();
            this.Loaded += HomePage_Loaded;

        }
        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Cambio de tamaño de la ventana
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.MinWidth = 800;
                this.MinHeight = 600;
                // Máximo al área útil de la pantalla:
                this.MaxWidth = SystemParameters.WorkArea.Width;
                this.MaxHeight = SystemParameters.WorkArea.Height;
            }

            // Verificamos si quedó fuera del área útil y la centramos:
            var wa = SystemParameters.WorkArea;
            if (this.Left < wa.Left ||
                this.Top < wa.Top ||
                this.Left + this.ActualWidth > wa.Right ||
                this.Top + this.ActualHeight > wa.Bottom)
            {
                this.Left = wa.Left + (wa.Width - this.ActualWidth) / 2;
                this.Top = wa.Top + (wa.Height - this.ActualHeight) / 2;
            }
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


        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                       ? WindowState.Normal
                       : WindowState.Maximized;
        }

        private void btnNomina_Click(object sender, RoutedEventArgs e)
        {
            NominaPage nominaPage = new NominaPage();
            nominaPage.Show();
            this.Close();
        }

        private void btnRegis_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            registerPage.Show();
        }

        private void btnLista_Click(object sender, RoutedEventArgs e)
        {
            ListaPage listaPage = new ListaPage();
            listaPage.Show();
            this.Close();
        }
    }
}
