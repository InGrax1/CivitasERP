using System;
using System.Collections.Generic;
using System.IO;
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
using CivitasERP.Models;
using System.Windows.Threading;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window, DPFP.Capture.EventHandler
    {
        // --- Variables para captura de huella ---
        private Capture Capturador;
        private Enrollment _enroladorGlobal;   // Acumula múltiples lecturas
        private Template TemplateHuella;        // Contendrá el template final
        public RegisterPage()
        {
            InitializeComponent();
            InicializarCaptura();
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
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

        // --- Evento para registrar el usuario y guardar la huella ---
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
            string nombre, apellidop, apellidom, usuario, correo, contraseña, categoria;
            nombre = txtUsuario.Text;
            apellidop = txtApellidoPaterno.Text;
            apellidom = txtApellidoMaterno.Text;
            usuario = txtUsuario.Text;
            correo = txtCorreo.Text;
            contraseña = pwdPassword.Password;
            categoria = "admin";
            Decimal semanal = 1000.00m;

            // 3) Serializar el TemplateHuella a byte[]
            byte[] huellaBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                TemplateHuella.Serialize(ms);
                huellaBytes = ms.ToArray();
            }

            // 4) Crear el objeto Insert_admin incluyendo la huella
            Insert_admin inser = new Insert_admin
            {
                Nombre = nombre,
                ApellidoP = apellidop,
                ApellidoM = apellidom,
                Correo = correo,
                Usuario = usuario,
                Contra = contraseña,
                Semanal = 1500.00m,
                Categoria = categoria,
                Huella = huellaBytes
            };

            // 5) Llamar al método que inserta en la base de datos
            try
            {
                inser.InsertarAdmin();
                MessageBox.Show("Usuario registrado con éxito.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar formularios y variable de huella
                TemplateHuella = null;
                txtUsuario.Clear();
                txtApellidoPaterno.Clear();
                txtApellidoMaterno.Clear();
                txtCorreo.Clear();
                pwdPassword.Clear();
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


        //Botones de la ventana
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
