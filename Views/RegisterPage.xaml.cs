using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CivitasERP.Models;
using BiometriaDP.Services;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window
    {
        private readonly string _connectionString;
        private FingerprintService _fingerService;
        private byte[] _templateHuella;   // contendrá el template serializado

        public RegisterPage()
        {
            InitializeComponent();

            // 1) Obtener la cadena de conexión:
            _connectionString = new Conexion().ObtenerCadenaConexion();

            // 2) Crear e inicializar el servicio de huella:
            _fingerService = new FingerprintService(_connectionString);
            _fingerService.OnEnrollmentComplete += FingerService_OnEnrollmentComplete;
            _fingerService.OnError += FingerService_OnError;

            // 3) Inicializamos el template en null (aún no capturado):
            _templateHuella = null;
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

        // Se dispara cuando el wrapper completa el Enrollment y envía el template (byte[])
        private void FingerService_OnEnrollmentComplete(object sender, FingerprintCapturedEventArgs e)
        {
            _templateHuella = e.TemplateBytes;

            // Actualizar UI en el hilo de la interfaz:
            Dispatcher.Invoke(() =>
            {
                EstadoHuella.Text = "✔ Huella capturada correctamente.";
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

        // --- Evento para registrar el usuario y guardar la huella ---
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1) Asegurarnos de que ya se haya capturado la huella
            if (_templateHuella == null)
            {
                MessageBox.Show(
                    "Primero debes capturar la huella con el botón “Capturar Huella”.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Obtener datos de los campos de texto
            string nombre = txtUsuario.Text;
            string apellidop = txtApellidoPaterno.Text;
            string apellidom = txtApellidoMaterno.Text;
            string usuario = txtUsuario.Text;
            string correo = txtCorreo.Text;
            string contraseña = pwdPassword.Password;
            string categoria = "admin";
            decimal semanal = 1500.00m;

            // 3) Crear el objeto Insert_admin incluyendo la huella
            Insert_admin inser = new Insert_admin
            {
                Nombre = nombre,
                ApellidoP = apellidop,
                ApellidoM = apellidom,
                Correo = correo,
                Usuario = usuario,
                Contra = contraseña,
                Semanal = semanal,
                Categoria = categoria,
                Huella = _templateHuella
            };

            // 4) Llamar al método que inserta en la base de datos
            try
            {
                inser.InsertarAdmin();

                // Limpiar formularios y variable de huella
                _templateHuella = null;
                txtUsuario.Clear();
                txtApellidoPaterno.Clear();
                txtApellidoMaterno.Clear();
                txtCorreo.Clear();
                pwdPassword.Clear();
                txtSueldo.Clear();
                EstadoHuella.Text = "Click para capturar huella";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el usuario: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Botones de ventana ---
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
