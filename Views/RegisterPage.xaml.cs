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
using CivitasERP.Models;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string nombre, apellidop, apellidom, usuario, correo, contraseña, categoria;
            nombre = txtUsuario.Text;
            apellidop = txtApellidoPaterno.Text;
            apellidom = txtApellidoMaterno.Text;
            usuario = txtUsuario.Text;
            correo = txtCorreo.Text;
            contraseña = pwdPassword.Password;
            categoria = "admin";
            Decimal semanal = 1000.00m;

            Insert_admin inser = new Insert_admin
            {
                Nombre = nombre,
                ApellidoP = apellidop,
                ApellidoM = apellidom,
                Correo = correo,
                Usuario = usuario,
                Contra = contraseña,
                Semanal = 1500.00m,
                Categoria = categoria
            };

            inser.InsertarAdmin();
            MessageBox.Show("Usuario Registrado Con Exito");

        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de huella no implementada aún.");
        }
    }
}
