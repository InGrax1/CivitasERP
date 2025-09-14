using BiometriaDP.Services;
using CivitasERP.Conexiones;
using CivitasERP.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static CivitasERP.Views.Usuario.LoginPage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CivitasERP.Views.Usuario
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
            _connectionString = new DBConexion().ObtenerCadenaConexion();

            // 2) Inicializar el servicio de huella (sólo Enrollment)
            int loggedAdmin = 1;
            // ?? throw new InvalidOperationException("No hay admin logueado");
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
            if(CmboCategoria.SelectedIndex == 0)
            {
                if (_templateHuella == null)
                {
                    MessageBox.Show(
                        "Primero debes capturar la huella con el botón “Capturar Huella”.",
                        "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

            }
            


            // 2) Obtener datos de los campos
            decimal semanal = 0;
            bool exito = decimal.TryParse(txtSueldo.Text, out semanal);
            string nombre = txtUsuario.Text;
            string apellidop = txtApellidoPaterno.Text;
            string apellidom = txtApellidoMaterno.Text;
            string usuario = txtUsuario.Text;
            string correo = txtCorreo.Text;
            string contraseña = pwdPassword.Password;

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellidop) ||
                string.IsNullOrWhiteSpace(apellidom) ||
                string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contraseña)) 
            {
                MessageBox.Show(
                    "Por favor completa todos los campos antes de registrar.",
                    "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!exito)
            {
                MessageBox.Show("El campo sueldo no es un número válido.",
                    "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) Verificar selección en ComboBox
            if (CmboCategoria.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una categoría (Administrador o Director).",
                    "Categoría requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string categoria = ((ComboBoxItem)CmboCategoria.SelectedItem).Content.ToString();

            try
            {
                if (categoria == "Superintendente")
                {
                    Insert_admin inser = new Insert_admin
                    {
                        Nombre = nombre,
                        ApellidoP = apellidop,
                        ApellidoM = apellidom,
                        Correo = correo,
                        Usuario = usuario,
                        Contra = contraseña,
                        Semanal = semanal,
                        Categoria = "Superintendente",
                        Huella = _templateHuella
                    };

                    inser.InsertarAdmin();
                }
                else if (categoria == "Director")
                {
                    // Suponiendo que tienes o crearás una clase Insert_jefe
                    Insert_admin insert_Jefe = new Insert_admin


                    {
                        Usuario = usuario,
                        Contra = contraseña // asumiendo que Jefe_contra = huella
                                            // Aquí puedes agregar campos adicionales si necesitas
                    };

                    insert_Jefe.InsertarJefe();
                }

                // Limpiar formularios y variable de huella
                _templateHuella = null;
                txtUsuario.Clear();
                txtApellidoPaterno.Clear();
                txtApellidoMaterno.Clear();
                txtCorreo.Clear();
                pwdPassword.Clear();
                txtSueldo.Clear();
                CmboCategoria.SelectedIndex = -1;
                EstadoHuella.Text = "Click para capturar huella";

                MessageBox.Show("Registro completado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
