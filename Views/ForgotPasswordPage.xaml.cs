using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        }
        private void btnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string correo = txtCorreo.Text.Trim();
            
            GlobalVariables.usuario = usuario;
            // 1) Comprueba existencia en dbo.admins
            int? adminId = null;
            var conexion = new Conexion();                          
            string cs = conexion.ObtenerCadenaConexion();      

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

                //obtienes el calor del id
                GlobalVariables.id_admin = adminId;
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
            RecoverySession.Expires = DateTime.Now.AddMinutes(15);

            // 2.1) Guarda TODO en la sesión
            RecoverySession.AdminId = adminId.Value;      // <-- el ID 
            RecoverySession.Code = codigo;
            RecoverySession.Expires = DateTime.Now.AddMinutes(15);


            // 3) Envía el código
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("lolgratis8@gmail.com"),
                    Subject = "Código de recuperación CivitasERP",
                    Body = $"Tu código de recuperación es: {codigo}"
                };
                mail.To.Add(correo);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;               // TLS
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;              // NO usar credenciales de Windows
                    smtp.Credentials = new NetworkCredential("lolgratis8@gmail.com", "dzhl tfhi osgr njzo\r\n"); // la App Password de 16 caracteres
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
