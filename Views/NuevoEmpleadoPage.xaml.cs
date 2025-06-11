using CivitasERP.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CivitasERP.Views.LoginPage;
using BiometriaDP.Services;
namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevoEmpleadoPage.xaml
    /// </summary>
    public partial class NuevoEmpleadoPage : Window
    {
        private readonly string _connectionString;
        private FingerprintService _fingerService;
        private byte[] _templateHuella;

        public NuevoEmpleadoPage()
        {
            InitializeComponent();

            // 1) Obtener la cadena de conexión
            _connectionString = new Conexion().ObtenerCadenaConexion();

            // 2) Inicializar el servicio de huella (sólo Enrollment)
            int loggedAdmin = GlobalVariables1.id_admin
                ?? throw new InvalidOperationException("No hay admin logueado");
            _fingerService = new FingerprintService(_connectionString, loggedAdmin);

            _fingerService.OnEnrollmentComplete += FingerService_OnEnrollmentComplete;
            _fingerService.OnError += FingerService_OnError;

            // 3) Estado inicial
            _templateHuella = null;
            EstadoHuella.Text = "Click para capturar huella";
        }


        // --- Evento para iniciar la lectura de huella ---
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Iniciar Enrollment (varias pasadas) con el wrapper
                _fingerService.StartEnrollment();
                EstadoHuella.Text = "⏳ Coloca tu dedo para el escaneo…";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo iniciar el registro de huella.\n\nDetalle:\n{ex.Message}",
                    "Error al capturar huella",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Se dispara cuando el Enrollment genera el template final
        private void FingerService_OnEnrollmentComplete(object sender, FingerprintCapturedEventArgs e)
        {
            _templateHuella = e.TemplateBytes;

            Dispatcher.Invoke(() =>
            {
                EstadoHuella.Text = "✔ Huella capturada correctamente.";
                MessageBox.Show("La huella fue capturada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // Se dispara si ocurre algún error durante el Enrollment (calidad baja, falta lector, etc.)
        private void FingerService_OnError(object sender, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                EstadoHuella.Text = $"{mensaje}";
            });
        }

        // --- Guarda el admin en BD junto con la huella ---
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_templateHuella == null)
            {
                MessageBox.Show(
                    "Primero debes capturar la huella con el botón “Capturar Huella”.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) Obtener datos de los campos de texto
            string Nombre = txtNombre.Text;
            string apellidop = txtApellidoPaterno.Text;
            string apellidom = txtApellidoMaterno.Text;
            string categoria = txtCategoria.Text;
            decimal sueldo = 0.00m;
            bool exito = decimal.TryParse(txtSueldo.Text, out sueldo);

            if (!exito)
            {
                MessageBox.Show(
                    "El campo sueldo no es un número válido.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Obtener ID de admin y ID de obra
            string usuario = GlobalVariables1.usuario;
            var idDB = new DB_admins();
            int? Id_admin = idDB.ObtenerIdPorUsuario(usuario);
            string obra = cmbObra.Text;
            int? id_obra = ObtenerID_obra(Id_admin, obra);

            // 3) Crear el objeto Insert_Empleado incluyendo la huella
            var insert_Empleado = new Insert_Empleado
            {
                Nombre = Nombre,
                ApellidoP = apellidop,
                ApellidoM = apellidom,
                Paga_semanal = sueldo,
                Paga_diaria = sueldo / 6,
                Paga_Hora_Extra = (sueldo / 6) / 8,
                Obra = obra,
                id_admin = Id_admin,
                id_obra = id_obra,
                categoria = categoria,
                Huella = _templateHuella
            };

            try
            {
                insert_Empleado.InsertEmpleado();

                // Limpiar campos y variable de huella
                _templateHuella = null;
                txtApellidoPaterno.Clear();
                txtApellidoMaterno.Clear();
                txtCategoria.Clear();
                txtNombre.Clear();
                txtSueldo.Clear();
                EstadoHuella.Text = "Click para capturar huella";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el empleado: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }






        // Al abrir el ComboBox, cargamos las obras asociadas al admin
        private void cmbObra_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
        }

        private void CargarDatosComboBox()
        {
            string usuario = GlobalVariables1.usuario;
            var dB_Admins = new DB_admins();
            int? idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);

            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            cmbObra.Items.Clear();
                            while (reader.Read())
                            {
                                cmbObra.Items.Add(reader["obra_nombre"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }

        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre))
                return null;

            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                    cmd.Parameters.AddWithValue("@obraNombre", obraNombre);
                    conn.Open();

                    object resultado = cmd.ExecuteScalar();
                    if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                        return idObra;

                    return null;
                }
            }
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

    }
}
