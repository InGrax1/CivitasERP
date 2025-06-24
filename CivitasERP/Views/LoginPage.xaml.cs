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
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Password;

            try
            {
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
                {
                    throw new ArgumentException("Por favor ingresa usuario y contraseña.");
                }

                byte[] hashIngresado = h.ObtenerSHA256(contraseña);

                // Primero validamos como admin
                byte[] hashAdmin = c.ObtenerHashContraseña(usuario);
                if (hashAdmin != null && hashIngresado.SequenceEqual(hashAdmin))
                {
                    int? idAdmin = c.ObtenerIdPorUsuario(usuario);
                    if (!idAdmin.HasValue)
                    {
                        MessageBox.Show("No se encontró el ID del admin.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Variables.IdAdmin = idAdmin;
                    Variables.Usuario = usuario;
                    Variables.Jefe = true;

                    MainWindow ventanaPrincipal = new MainWindow();
                    ventanaPrincipal.Show();
                    ventanaPrincipal.CargarFotoPerfil(idAdmin.Value);
                    this.Close();
                    return;
                }

                // Si no es admin, intentamos validar como jefe
                byte[] hashJefe = c.ObtenerHashContraseñaJefe(usuario);
                if (hashJefe != null && hashIngresado.SequenceEqual(hashJefe))
                {
                    int? idJefe = c.ObtenerIdPorUsuarioJefe(usuario);
                    if (!idJefe.HasValue)
                    {
                        MessageBox.Show("No se encontró el ID del jefe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Variables.IdAdmin = idJefe;
                    Variables.Usuario = usuario;
                    Variables.Jefe = true;

                    MainWindow ventanaPrincipal = new MainWindow();
                    ventanaPrincipal.Show();
                    ventanaPrincipal.CargarFotoPerfilJefe(idJefe.Value);
                    this.Close();
                    return;
                }

                // Si no se valida como admin ni como jefe
                MessageBox.Show("Usuario o contraseña incorrectos", "Error de autenticación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al iniciar sesión:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    
        private void brnResetPassword(object sender, RoutedEventArgs e)
        {
            ForgotPasswordPage forgotPasswordPage = new ForgotPasswordPage();
            forgotPasswordPage.ShowDialog();
            this.Hide();
        }

    }
}
