using CivitasERP.Models;
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
using System.Data;



namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NominaPage.xaml
    /// </summary>
    public partial class NominaPage : Window
    {
        private DataGridNominas repo;
        public NominaPage()
        {
            InitializeComponent();
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            repo = new DataGridNominas(connectionString);

            var empleados = repo.ObtenerEmpleados();
            dataGridNomina.ItemsSource = empleados;

            this.Loaded += HomePage_Loaded;
        }
        //Poder mover la ventana con libertad
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
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
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnSemanaOp_Click(object sender, RoutedEventArgs e)
        {
            //PRUEBA, LOGICA DEL BOTON SEMANA NO OFICIAL
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                       ? WindowState.Normal
                       : WindowState.Maximized;
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Show();
            this.Close();
        }
        private void btnLista_Click(object sender, RoutedEventArgs e)
        {
            ListaPage listaPage = new ListaPage();
            listaPage.Show();
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
    }
    
}
