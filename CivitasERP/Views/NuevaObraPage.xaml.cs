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

            // 0) Validar que no hayan campos vacíos
            string nombreObra = txtObraNombre.Text.Trim();
            string ubicacion = txtObraUbicacion.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombreObra) || string.IsNullOrWhiteSpace(ubicacion))
            {
                MessageBox.Show(
                    "Por favor completa ambos campos (Nombre de obra y Ubicación).",
                    "Campos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 1) Obtener el id del admin
            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            int? idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);
            if (idAdminObra == null)
            {
                MessageBox.Show(
                    "No se pudo determinar tu usuario de administrador.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // 2) Verificar que NO exista ya una obra con ese nombre para este admin
            bool existe;
            const string sqlCheck = @"
        SELECT COUNT(*) 
          FROM obra 
         WHERE id_admin_obra = @idAdmin 
           AND obra_nombre = @nombreObra;
    ";
            using (var conn = new SqlConnection(new Conexion().ObtenerCadenaConexion()))
            using (var cmd = new SqlCommand(sqlCheck, conn))
            {
                cmd.Parameters.AddWithValue("@idAdmin", idAdminObra);
                cmd.Parameters.AddWithValue("@nombreObra", nombreObra);
                conn.Open();
                existe = (int)cmd.ExecuteScalar() > 0;
            }

            if (existe)
            {
                MessageBox.Show(
                    $"Ya tienes registrada una obra llamada “{nombreObra}”.\nPor favor elige otro nombre.",
                    "Nombre duplicado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 3) Insertar la obra
            try
            {
                var insert_Obra = new Insert_Obra();
                insert_Obra.AgregarObra(nombreObra, ubicacion, idAdminObra);

                MessageBox.Show(
                    "Obra agregada correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 4) Limpiar el formulario
                txtObraNombre.Clear();
                txtObraUbicacion.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al agregar la obra:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

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
