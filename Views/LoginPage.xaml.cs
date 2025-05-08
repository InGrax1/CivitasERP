using CivitasERP.Clases;
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
using CivitasERP.Clases;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        //Poder mover la ventana con libertad
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Bd_admin c = new Bd_admin();
            hash h = new hash();
            string usuario,contraseña;
            usuario = txtUsuario.Text;
            contraseña = txtPassword.Password;
            

            if (h.ObtenerSHA256(contraseña).SequenceEqual(c.ObtenerHashContraseña(usuario))) {
                MessageBox.Show("usuario y contrasña correctos ");
                this.Hide();
                HomePage HomePage = new HomePage();
                HomePage.Show();
            }
            else {
                MessageBox.Show("usuario o contrasña incorrectos la contraseña correcta es " );

                string base64 = Convert.ToBase64String(h.ObtenerSHA256(contraseña));
                string a = Convert.ToBase64String(c.ObtenerHashContraseña(usuario));
                MessageBox.Show(base64);
                MessageBox.Show(a);
            }
                
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            registerPage.Show();
        }
    }
}
