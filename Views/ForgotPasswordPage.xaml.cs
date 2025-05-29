using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

namespace CivitasERP.Views
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

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de actualización de contraseña no implementada aún.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void brnBackLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            LoginPage loginPage = new LoginPage();
            loginPage.Show();
        }

        private void btnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string correo = txtCorreo.Text.Trim();

            // 1) Comprueba existencia en dbo.admins
            int? adminId = null;
            var conexion = new Conexion();                              // <-- tu clase
            string cs = conexion.ObtenerCadenaConexion();           // <-- aquí obtiene "Server=INGRAX;Database=CivitasERP;…"

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
            }

            if (adminId == null)
            {
                MessageBox.Show("Usuario o correo no registrados.",
                                "Error",MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // 2) Genera un código de 6 dígitos
            var random = new Random();
            string codigo = random.Next(0, 999999).ToString("D6");

            // 3) Envía el código
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("no-reply@tusitio.com"),
                    Subject = "Código de recuperación CivitasERP",
                    Body = $"Tu código de recuperación es: {codigo}"
                };
                mail.To.Add(correo);

                using (var smtp = new SmtpClient("smtp.tuservidor.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("smtp_user", "smtp_pass");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                MessageBox.Show("Código enviado. Revisa tu correo.",
                                "Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar correo: {ex.Message}",
                                "Error SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Opcional: aquí podrías guardar 'codigo' + adminId en una tabla para validar luego
        }
    }
}
