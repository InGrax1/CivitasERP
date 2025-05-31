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
            
        }

    }
}

