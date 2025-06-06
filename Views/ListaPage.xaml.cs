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
    public partial class ListaPage : Page
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

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
        {

        }
    }
}
