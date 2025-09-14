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
using static CivitasERP.Views.Usuario.ForgotPasswordPage;



namespace CivitasERP.Views.Usuario
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
            loginPage.ShowDialog();
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string usuario = GlobalVariables.usuario;
            int? id_admin = GlobalVariables.id_admin;
            id_admin = GlobalVariables.id_admin;
            string nueva = txtNewPassword.Password;
            string confirmar = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(confirmar))
            {
                MessageBox.Show(
                    "Ambos campos de contraseña deben estar llenos.",
                    "Campos vacíos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (nueva.Length < 3)
            {
                MessageBox.Show(
                    "La nueva contraseña debe tener al menos 3 caracteres.",
                    "Contraseña demasiado corta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (nueva != confirmar)
            {
                MessageBox.Show(
                    "Las contraseñas no coinciden. Por favor ingrésalas de nuevo.",
                    "Contraseñas diferentes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            try
            {
                Reset_Password rp = new Reset_Password();
                rp.CambiarContraseña(usuario, nueva, id_admin);
      
                MessageBox.Show("Tu contraseña ha sido actualizada correctamente. Ahora inicia sesión con tu nueva contraseña.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Regresa a la página de inicio de sesión:
                var loginWindow = new LoginPage();
                loginWindow.Show();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al cambiar tu contraseña:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }



        }

    }
}

