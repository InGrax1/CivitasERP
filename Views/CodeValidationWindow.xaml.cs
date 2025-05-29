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

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para CodeValidationWindow.xaml
    /// </summary>
    public partial class CodeValidationWindow : Window
    {
        public CodeValidationWindow()
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
            this.Hide();
            LoginPage loginPage = new LoginPage();
            loginPage.Show();

        }
        private void btnValidar_Click(object sender, RoutedEventArgs e)
        {
            // 1) ¿Expiró?
            if (DateTime.Now > RecoverySession.Expires)
            {
                MessageBox.Show(
                    "El código ha expirado. Solicita uno nuevo.",
                    "Expirado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                this.Close();
                ForgotPasswordPage forgotPasswordPage = new ForgotPasswordPage();
                forgotPasswordPage.ShowDialog();
                return;
            }

            // 2) ¿Coincide?
            if (txtCodigo.Text.Trim() != RecoverySession.Code)
            {
                MessageBox.Show(
                    "Código incorrecto. Intenta de nuevo.",
                    "Inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 3) Correcto → abrir ResetPasswordPage
            this.Close();
            var reset = new ResetPasswordPage();
            reset.ShowDialog();
            
        }
    }
}

