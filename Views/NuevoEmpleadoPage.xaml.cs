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
using CivitasERP.Views;
using CivitasERP.ViewModels;
using static CivitasERP.Views.LoginPage;
using System.Data.SqlClient;
using System.Reflection;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using System.IO;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevoEmpleadoPage.xaml
    /// </summary>
    public partial class NuevoEmpleadoPage : Window, DPFP.Capture.EventHandler
    {
        // --- Variables para captura de huella ---
        private Capture Capturador;
        private Enrollment _enroladorGlobal;   // Acumula múltiples lecturas
        private Template TemplateHuella;        // Contendrá el template final
        public NuevoEmpleadoPage()
        {
            InitializeComponent();
            InicializarCaptura();

        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Inicializa el capturador de huellas dactilares.
        /// </summary>
        private void InicializarCaptura()
        {
            try
            {
                Capturador = new Capture();                 // Crear instancia de Capture
                Capturador.EventHandler = this;             // Registrar los eventos en esta clase
                _enroladorGlobal = new Enrollment(); // Creamos el Enrollment vacío
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo inicializar el capturador de huella:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Evento para iniciar la lectura de huella ---
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Iniciamos la captura. Si no hay lector, StartCapture lanzará excepción.
                Capturador.StartCapture();
                EstadoHuella.Text = "⏳ Coloca tu dedo en el lector…";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo iniciar la captura. Verifica que el lector esté conectado.\n\nDetalle:\n{ex.Message}",
                    "Error al inicializar lector",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1) Asegurarnos de que ya se haya capturado la huella
            if (TemplateHuella == null)
            {
                MessageBox.Show(
                    "Primero debes capturar la huella con el botón “Capturar Huella”.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Obtener datos de los campos de texto
            string Nombre ="", apellidop = "", apellidom = "", categoria = "", usuario = "", obra = "";
            int? Id_admin=0 ,id_obra=0;
            decimal sueldo=0;
            bool exito;
            Nombre = txtNombre.Text;
            apellidop = txtApellidoPaterno.Text;
            apellidom = txtApellidoMaterno.Text;
            categoria = txtCategoria.Text;
            exito = decimal.TryParse(txtSueldo.Text, out sueldo);

            // 3) Serializar el TemplateHuella a byte[]
            byte[] huellaBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                TemplateHuella.Serialize(ms);
                huellaBytes = ms.ToArray();
            }


            //a) obtener ID ADMIN
            DB_admins id_ADMIN = new DB_admins();
            usuario = GlobalVariables1.usuario;
            Id_admin = id_ADMIN.ObtenerIdPorUsuario(usuario);
            obra = cmbObra.Text;
            id_obra = ObtenerID_obra(Id_admin, obra);


            // 4) Crear el objeto Insert_Empleado incluyendo la huella
            Insert_Empleado insert_Empleado = new Insert_Empleado()
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
                Huella = huellaBytes
            };

            // 5) Llamar al método que inserta en la base de datos
            try
            {
                insert_Empleado.InsertEmpleado();
                MessageBox.Show("Empleado Registrado Con Exito");

                // Limpiar formularios y variable de huella
                TemplateHuella = null;
                txtApellidoPaterno.Clear();
                txtApellidoMaterno.Clear();
                txtCategoria.Clear();
                txtNombre.Clear();
                txtSueldo.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el usuario: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === Implementación de DPFP.Capture.EventHandler ===
        // Se llama cuando se completa la lectura de la huella
        public void OnComplete(object capture, string ReaderSerialNumber, Sample sample)
        {
            try
            {
                // 1) Extraemos las características de la muestra en modo Enrollment
                FeatureSet features = ExtraerCaracteristicas(sample, DataPurpose.Enrollment);
                if (features == null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        EstadoHuella.Text = "❌ No se pudieron extraer características. Intenta de nuevo.";
                        MessageBox.Show(
                            "No se pudieron extraer características. Intenta de nuevo.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                    return;
                }

                // 2) Agregamos estas características al Enrollment (persistente)
                _enroladorGlobal.AddFeatures(features);

                // 3) Revisamos si ya “se completó” el template
                if (_enroladorGlobal.TemplateStatus == Enrollment.Status.Ready)
                {
                    // Ya acumuló suficientes lecturas: generamos el Template definitivo
                    TemplateHuella = _enroladorGlobal.Template;

                    // 4) Detenemos la captura AHORA que el template está listo
                    Capturador.StopCapture();

                    Dispatcher.Invoke(() =>
                    {
                        EstadoHuella.Text = "✔ Huella capturada correctamente.";

                    });
                }
                else
                {
                    // No aún no es suficiente: pedimos otra pasada
                    int faltantes = (int)_enroladorGlobal.FeaturesNeeded;
                    Dispatcher.Invoke(() =>
                    {
                        EstadoHuella.Text = $"⏳ Lectura recibida. Levanta y vuelve a colocar tu dedo {faltantes} vez{(faltantes > 1 ? "es" : "")} más.";
                    });

                    // 5) Volvemos a iniciar la captura para la siguiente pasada
                    Capturador.StartCapture();
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    EstadoHuella.Text = "❌ Error interno al procesar huella.";
                    MessageBox.Show(
                        $"Error interno en OnComplete:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }




        // Extrae las características de la muestra según el propósito
        private FeatureSet ExtraerCaracteristicas(Sample sample, DataPurpose purpose)
        {
            try
            {
                FeatureExtraction extractor = new FeatureExtraction();
                CaptureFeedback feedback = CaptureFeedback.None;
                FeatureSet features = new FeatureSet();
                extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);
                return (feedback == CaptureFeedback.Good) ? features : null;
            }
            catch
            {
                return null;
            }
        }

        // Métodos vacíos que requiere la interfaz
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback) { }
        
        //FIN DE LECTOR DE HUELLAS



        ////////////////////////////////

        private void cmbObra_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
        }
        private void CargarDatosComboBox()
        {
            string usuario;
            usuario =GlobalVariables1.usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdminObra;
            idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);

            try
            {
                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;


                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbObra.Items.Clear();

                    while (reader.Read())
                    {
                        cmbObra.Items.Add(reader["obra_nombre"].ToString());
                    }

                    reader.Close();
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

            Conexion Sconexion = new Conexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                cmd.Parameters.AddWithValue("@obraNombre", obraNombre);

                conn.Open();
                object resultado = cmd.ExecuteScalar();

                if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                {
                    return idObra;
                }

                return null; // No se encontró coincidencia
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
