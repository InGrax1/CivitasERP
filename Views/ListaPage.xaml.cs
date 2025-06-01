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

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para ListaPage.xaml
    /// </summary>
    public partial class ListaPage : Window
    {
        public ListaPage()
        {
            InitializeComponent();
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

      
        private void btnRegis_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            registerPage.Show();
        }


        private void btnSemanaOp_Click(object sender, RoutedEventArgs e)
        {
            //PRUEBA, LOGICA DEL BOTON SEMANA NO OFICIAL
            this.WindowState = WindowState.Minimized;

        }
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Show();
            this.Close();
        }
        private void btnNomina_Click(object sender, RoutedEventArgs e)
        {
            NominaPage nominaPage = new NominaPage();
            nominaPage.Show();
            this.Close();
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // 1) Instanciamos la ventana de Login (ajusta el nombre si tu clase se llama distinto)
            var loginWindow = new LoginPage();

            // 2) Mostramos Login en modo no modal (para que la app siga viva)
            loginWindow.Show();

            // 3) Cerramos la ventana actual (dejará viva únicamente la de Login)
            this.Close();
        }

        private void ObraTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de huella no implementada aún.");
        }
    }
}
