using CivitasERP.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevoEmpleadoPage.xaml
    /// </summary>
    public partial class NuevoEmpleadoPage : Window
    {
        private byte[] _templateHuella; // aquí almacenaremos el template serializado desde el wrapper

        public NuevoEmpleadoPage()
        {
            InitializeComponent();
            // Inicializar el wrapper de huella más adelante:
            // _fingerService = new FingerprintService();
            // _fingerService.OnEnrollmentComplete += FingerService_OnEnrollmentComplete;
            // _fingerService.OnError             += FingerService_OnError;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // --- Evento para iniciar la lectura de huella ---
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // En lugar de DPFP, llamamos al wrapper:
                // _fingerService.StartEnrollment();
                EstadoHuella.Text = "⏳ Enroll: coloca tu dedo varias veces…";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo iniciar la captura de huella.\n\nDetalle:\n{ex.Message}",
                    "Error al inicializar huella",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Este método lo llamará el wrapper cuando el template esté listo
        // private void FingerService_OnEnrollmentComplete(object s, FingerprintCapturedEventArgs e)
        // {
        //     _templateHuella = e.TemplateBytes;
        //     Dispatcher.Invoke(() =>
        //     {
        //         EstadoHuella.Text = "✔ Huella capturada correctamente.";
        //     });
        // }

        // Este método lo llamará el wrapper en caso de error
        // private void FingerService_OnError(object s, string mensaje)
        // {
        //     Dispatcher.Invoke(() =>
        //     {
        //         EstadoHuella.Text = $"❌ {mensaje}";
        //         MessageBox.Show(mensaje, "Error huella", MessageBoxButton.OK, MessageBoxImage.Error);
        //     });
        // }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_templateHuella == null)
            {
                MessageBox.Show(
                    "Primero debes capturar la huella con el botón “Capturar Huella”.",
                    "Atención",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 1) Obtener datos de los campos de texto
            string Nombre = txtNombre.Text;
            string apellidop = txtApellidoPaterno.Text;
            string apellidom = txtApellidoMaterno.Text;
            string categoria = txtCategoria.Text;
            decimal sueldo = 0;
            bool exito = decimal.TryParse(txtSueldo.Text, out sueldo);

            if (!exito)
            {
                MessageBox.Show(
                    "El campo sueldo no es un número válido.",
                    "Error de validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
                MessageBox.Show(
                    "Empleado registrado con éxito.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

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
    }
}
