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
    /// Lógica de interacción para ResetPasswordPage.xaml
    /// </summary>
    public partial class ResetPasswordPage : Window
    {
        public ResetPasswordPage()
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
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de restablecimiento de contraseña no implementada aún.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
