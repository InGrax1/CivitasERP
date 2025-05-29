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
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;



namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para ResetPasswordPage.xaml
    /// </summary>
    public partial class ResetPasswordPage : Window
    {
        private readonly int _adminId;

        public ResetPasswordPage()
        {
            InitializeComponent();
            _adminId = RecoverySession.AdminId;

        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            LoginPage loginPage = new LoginPage();
            loginPage.Show();


        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validar que las dos contraseñas coincidan
            string p1 = txtNewPassword.Password;
            string p2 = txtConfirmPassword.Password;
            if (p1 != p2)
            {
                MessageBox.Show(
                    "Las contraseñas no coinciden.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2) Generar salt + hash (PBKDF2)
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(p1, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // 3) Combinar salt + hash
            byte[] hashBytes = new byte[36];   //<------ CANTIDAD DE BYTES ENVIADOS
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // 4) Actualizar en la BD pasando directamente el byte[]
            string cs = new Conexion().ObtenerCadenaConexion();
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                  "UPDATE dbo.admins SET admins_contra = @h WHERE id_admins = @id", conn))
            {
                // Parámetro varbinary
                cmd.Parameters.Add("@h", SqlDbType.VarBinary, hashBytes.Length)
                              .Value = hashBytes;

                // Parámetro int
                cmd.Parameters.Add("@id", SqlDbType.Int)
                              .Value = _adminId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show(
                "Contraseña reiniciada con éxito.",
                "Éxito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            this.Close();
        }

    }
}

