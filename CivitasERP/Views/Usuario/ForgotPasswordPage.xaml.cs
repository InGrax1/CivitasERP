using CivitasERP.Models;
using CivitasERP.Conexiones;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace CivitasERP.Views.Usuario
{
    /// <summary>
    /// Lógica de interacción para ForgotPasswordPage.xaml
    /// </summary>
    public partial class ForgotPasswordPage : Window
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            LoginPage loginPage = new LoginPage();
            loginPage.ShowDialog();
        }

        private void brnBackLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            LoginPage loginPage = new LoginPage();
            loginPage.ShowDialog();
        }

        public static class GlobalVariables
        {
            public static string usuario;
            public static int? id_admin;
            public static string correoUsuario;
        }

        // 2. método público para enviar el correo
        public bool EnviarCodigoRecuperacion(string usuario, string correo, string codigo)
        {
            try
            {
                // Cargar y preparar la plantilla
                string plantillaHTML = CargarPlantillaHTML();
                string contenidoFinal = ReemplazarPlaceholders(plantillaHTML, codigo, usuario);

                // Configurar el mensaje
                var mail = new MailMessage
                {
                    From = new MailAddress("lolgratis8@gmail.com", "CivitasERP - Soporte"),
                    Subject = "🔐 Código de Recuperación - CivitasERP",
                    Body = contenidoFinal,
                    IsBodyHtml = true
                };
                mail.To.Add(correo);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("lolgratis8@gmail.com", "dzhl tfhi osgr njzo");
                    smtp.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar correo: {ex.Message}",
                                "Error SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        /// <summary>
        /// Carga la plantilla HTML desde el archivo externo
        /// </summary>
        /// <returns>Contenido HTML de la plantilla</returns>
        private string CargarPlantillaHTML()
        {
            try
            {
                // 1) Obtén la carpeta raíz donde está el ejecutable
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // 2) Construye la ruta incluyendo Views
                string templatePath = System.IO.Path.Combine(baseDir, "Views", "EmailTemplate.html");

                if (!File.Exists(templatePath))
                    throw new FileNotFoundException($"No se encontró la plantilla en: {templatePath}");

                return File.ReadAllText(templatePath, Encoding.UTF8);
            }
            catch
            {
                // Si algo falla, regresa la plantilla básica
                return CrearPlantillaBasica();
            }
        }


        /// <summary>
        /// Plantilla básica de respaldo en caso de error
        /// </summary>
        /// <returns>HTML básico</returns>
        private string CrearPlantillaBasica()
        {
            return @"
            <html>
            <body style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                <h2>CivitasERP - Código de Recuperación</h2>
                <p>Hola {NOMBRE_USUARIO},</p>
                <p>Tu código de recuperación es: <strong style='font-size: 24px; color: #007bff;'>{CODIGO_VERIFICACION}</strong></p>
                <p>Este código expira el: {FECHA_EXPIRACION}</p>
                <p>Si no solicitaste este código, ignora este mensaje.</p>
            </body>
            </html>";
        }

        /// <summary>
        /// Reemplaza los placeholders en la plantilla con los valores reales
        /// </summary>
        /// <param name="plantilla">Plantilla HTML</param>
        /// <param name="codigo">Código de verificación</param>
        /// <param name="nombreUsuario">Nombre del usuario</param>
        /// <returns>HTML con valores reemplazados</returns>
        private string ReemplazarPlaceholders(string plantilla, string codigo, string nombreUsuario)
        {
            var fechaExpiracion = DateTime.Now.AddMinutes(15).ToString("dd/MM/yyyy HH:mm");

            return plantilla
                .Replace("{CODIGO_VERIFICACION}", codigo)
                .Replace("{NOMBRE_USUARIO}", nombreUsuario)
                .Replace("{FECHA_EXPIRACION}", fechaExpiracion);
        }

        private void btnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string correo = txtCorreo.Text.Trim();

            GlobalVariables.usuario = usuario;
            GlobalVariables.correoUsuario = correo;

            // 1) Comprueba existencia en dbo.admins
            int? adminId = null;
            var conexion = new DBConexion();
            string cs = conexion.ObtenerCadenaConexion();

            // 1) Comprueba existencia en dbo.admins
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
              SELECT id_admins
                FROM dbo.admins
               WHERE admins_usuario = @u
                 AND admins_correo  = @c", conn))
            {
                cmd.Parameters.AddWithValue("@u", usuario);
                cmd.Parameters.AddWithValue("@c", correo);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                    adminId = Convert.ToInt32(result);

                // obtienes el valor del id
                GlobalVariables.id_admin = adminId;
            }

            if (adminId == null)
            {
                MessageBox.Show("Usuario o correo no registrados.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // 2) Genera un código de 6 dígitos
            var random = new Random();
            string codigo = random.Next(0, 999999).ToString("D6");

            // 2.1) Guarda TODO en la sesión
            RecoverySession.AdminId = adminId.Value;
            RecoverySession.Code = codigo;
            RecoverySession.Expires = DateTime.Now.AddMinutes(15);

            // 3) Envía el código con plantilla HTML
            try
            {
                // Cargar y preparar la plantilla
                string plantillaHTML = CargarPlantillaHTML();
                string contenidoFinal = ReemplazarPlaceholders(plantillaHTML, codigo, usuario);

                // Configurar el mensaje
                var mail = new MailMessage
                {
                    From = new MailAddress("lolgratis8@gmail.com", "CivitasERP - Soporte"),
                    Subject = "🔐 Código de Recuperación - CivitasERP",
                    Body = contenidoFinal,
                    IsBodyHtml = true // ¡MUY IMPORTANTE! Esto permite HTML
                };
                mail.To.Add(correo);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("lolgratis8@gmail.com", "dzhl tfhi osgr njzo");
                    smtp.Send(mail);
                }

                MessageBox.Show("Código enviado. Revisa tu correo.",
                                "Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Hide();
                CodeValidationWindow codeValidationWindow = new CodeValidationWindow();
                codeValidationWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar correo: {ex.Message}",
                                "Error SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}