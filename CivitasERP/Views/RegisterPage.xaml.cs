using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CivitasERP.Models;
using BiometriaDP.Services;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window
    {
        private readonly string _connectionString;
        private readonly FingerprintService _fingerService;
        private byte[] _templateHuella;   // contendrá el template serializado

        public RegisterPage()
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

            // Actualizar UI en el hilo de la interfaz:
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
            decimal semanal = 0.00m;
            bool exito = decimal.TryParse(txtSueldo.Text, out semanal);
            if (!exito)
            {
                MessageBox.Show(
                    "El campo sueldo no es un número válido.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
