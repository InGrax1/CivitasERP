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
        private void DragWindow(object sender, MouseButtonEventArgs e)
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


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            DB_admins c = new DB_admins();
            hash h = new hash();
            string usuario,contraseña;

            usuario = txtUsuario.Text;
            contraseña = txtPassword.Password;


            try
            {
                //validacion de campos vacios
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
                {
                    throw new ArgumentException("Por favor ingresa usuario y contraseña.");
                }
                
                //validacion de usuario y contraseña
                if (h.ObtenerSHA256(contraseña).SequenceEqual(c.ObtenerHashContraseña(usuario)))
                {
                    // 1) Crear instancia de MainWindow
                    MainWindow ventanaPrincipal = new MainWindow();

                    // 2) Mostrar MainWindow
                    ventanaPrincipal.Show();

                    // 3) Cerrar (o Hide) la ventana de login actual:
                    this.Close();

                }
                else
                {
                    MessageBox.Show("usuario o contraseña incorrectos", "Error de autenticación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    
                }
            }
            catch (ArgumentException ex)
            {
                // ocurre cuando alguno de los campos está vacío
                MessageBox.Show(ex.Message, "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex)
            {
                // cualquier otro error inesperado
                MessageBox.Show($"Ocurrió un error al iniciar sesión:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            GlobalVariables1.usuario = "";
            GlobalVariables1.usuario = usuario;

        }
        public static class GlobalVariables1
        {

            public static string usuario;
            public static int? id_admin;
            public static int? id_obra;
        }
        private void brnResetPassword(object sender, RoutedEventArgs e)
        {
            ForgotPasswordPage forgotPasswordPage = new ForgotPasswordPage();
            forgotPasswordPage.Show();
            this.Hide();
        }

    }
}
