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
            MessageBox.Show("diste click");
        }
    }
}
