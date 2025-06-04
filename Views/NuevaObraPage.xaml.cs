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
using System.Data.SqlClient;
using CivitasERP.ViewModels;
using CivitasERP.Models;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevaObraPage.xaml
    /// </summary>
    public partial class NuevaObraPage : Window
    {
        public NuevaObraPage()
        {
            InitializeComponent();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


        private void btnAgregarObra_Click(object sender, RoutedEventArgs e)
        {
            string usuario, nombreObra, ubicacion;
            usuario = GlobalVariables1.usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdminObra;
            idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);

            nombreObra = txtObraNombre.Text;
            ubicacion = txtObraUbicacion.Text;

            Insert_Obra insert_Obra = new Insert_Obra();
            insert_Obra.AgregarObra(nombreObra, ubicacion, idAdminObra);  
            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
