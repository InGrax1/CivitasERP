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
using static CivitasERP.Models.Datagrid_lista;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Data;
using BiometriaDP.Services;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para ListaPage.xaml
    /// </summary>
    public partial class ListaPage : Window
    {
        private Datagrid_lista repo;

        private readonly string _connectionString;
        private FingerprintService _fingerService;
        public ListaPage()
        {
            InitializeComponent();

            CargarEmpleadosEnGrilla();

            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            repo = new Datagrid_lista(connectionString);

            var Empleado_Asistencia = repo.ObtenerEmpleados();
            dataGridAsistencia.ItemsSource = Empleado_Asistencia;

            // 1) Obtener cadena de conexión y crear el servicio
            _connectionString = new Conexion().ObtenerCadenaConexion();
            _fingerService = new FingerprintService(_connectionString);
            // 2) Suscribirse a eventos de verificación
            _fingerService.OnVerificationComplete += FingerService_OnVerificationComplete;
            _fingerService.OnError += FingerService_OnError;
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


        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _fingerService.StartVerification();
                lblEstadoHuella.Text = "⏳ Coloca tu dedo para validar...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar verificación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FingerService_OnVerificationComplete(object sender, FingerprintVerifiedEventArgs e)
        {
            int idEmpleadoMatch = e.IdEmpleado;
            if (idEmpleadoMatch > 0)
            {
                // Marcar hora de entrada/salida en la base de datos
                var repo = new Datagrid_lista(new Conexion().ObtenerCadenaConexion());
                repo.MarcarAsistencia(idEmpleadoMatch);

                Dispatcher.Invoke(() =>
                {
                    lblEstadoHuella.Text = $"✔ Huella reconocida. Empleado #{idEmpleadoMatch}.";
                    MessageBox.Show($"Asistencia registrada para empleado #{idEmpleadoMatch}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarEmpleadosEnGrilla();
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    lblEstadoHuella.Text = "❌ Huella no reconocida.";
                    MessageBox.Show("Huella no coincide con ningún empleado.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void FingerService_OnError(object sender, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                lblEstadoHuella.Text = $"❌ {mensaje}";
                MessageBox.Show(mensaje, "Error huella", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        private void CargarEmpleadosEnGrilla()
        {
            // Crea un repositorio con la cadena de conexión y obtiene la lista de empleados/asistencias
            var repo = new Datagrid_lista(new Conexion().ObtenerCadenaConexion());
            var listaEmpleados = repo.ObtenerEmpleados();

            // Asigna esa lista al DataGrid de la UI
            dataGridAsistencia.ItemsSource = listaEmpleados;
        }

        /// <summary>
        /// Devuelve una lista de tuplas (idEmpleado, byte[] template) leídas desde la tabla 'admins'.
        /// </summary>
        private List<Tuple<int, byte[]>> ObtenerTemplatesDesdeBD()
        {
            var lista = new List<Tuple<int, byte[]>>();
            string connectionString = new Conexion().ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT admins_id, admin_huella 
                               FROM admins 
                               WHERE admin_huella IS NOT NULL";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        byte[] huellaBytes = reader["admin_huella"] as byte[];
                        lista.Add(new Tuple<int, byte[]>(id, huellaBytes));
                    }
                }
                conn.Close();
            }
            return lista;
        }

        /// <summary>
        /// Dado un ID de usuario, busca su nombre en la tabla 'admins' para mostrarlo.
        /// </summary>
        private string ObtenerUsernamePorId(int userId)
        {
            string nombreUsuario = string.Empty;
            string connectionString = new Conexion().ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT admins_usuario 
                               FROM admins 
                               WHERE admins_id = @Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        nombreUsuario = result.ToString();
                }
                conn.Close();
            }
            return nombreUsuario;
        }
    }
}
