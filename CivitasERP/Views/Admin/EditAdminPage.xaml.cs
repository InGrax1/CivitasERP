using CivitasERP.Models;
using CivitasERP.Conexiones;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CivitasERP.AdminViews
{
    /// <summary>
    /// Lógica de interacción para EditAdminPage.xaml
    /// </summary>
    public partial class EditAdminPage : Page
    {
        public EditAdminPage()
        {
            InitializeComponent();
            cargar_admin();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {

            // 1) Validar que hayan seleccionado un empleado
            if (AdminComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Por favor selecciona un empleado.",
                    "Campos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string admin;
            admin = AdminComboBox.SelectedItem.ToString();
            DB_admins admins = new DB_admins();

            GuardarDatosAdmindb(admins.ObtenerIdPorUsuario(admin));
        }

        private void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdminComboBox.SelectedItem != null)
            {
                string admin = AdminComboBox.SelectedItem.ToString();
                DB_admins admins = new DB_admins();

                CargarDatosAdmindb(admins.ObtenerIdPorUsuario(admin));
                CargarDatosComboBox(admins.ObtenerIdPorUsuario(admin));
            }
            else
            {
                // Opcional: limpia los datos o muestra un mensaje
                Console.WriteLine("No se ha seleccionado ningún administrador.");
            }
        }

        private void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {
            cargar_admin();
        }



        private void cargar_admin()
        {

            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            Variables.IdAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
            int? idAdminObra = Variables.IdAdmin;

            try
            {
                var conexion = new DBConexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    // Suponiendo que quieres obtener todos los admins, o filtrar de alguna forma
                    string query = @"SELECT
                            admins_usuario AS admin_nombre
                     FROM admins;";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            AdminComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                AdminComboBox.Items.Add(reader["admin_nombre"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar administradores: " + ex.Message);
            }


        }

        private void CargarDatosAdmindb(int? idAdmin)
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT admins_nombre, admins_apellidop, admins_apellidom, admins_correo, admins_semanal, admin_categoria FROM admins WHERE id_admins = @idAdmin";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@idAdmin", idAdmin);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        TxtNombre.Text = reader["admins_nombre"].ToString();
                        TxtApellidoP.Text = reader["admins_apellidop"].ToString();
                        TxtApellidoM.Text = reader["admins_apellidom"].ToString();
                        TxtCorreo.Text = reader["admins_correo"].ToString();
                        TxtCategoria.Text = reader["admin_categoria"].ToString();
                        TxtSueldoSemanal.Text = Convert.ToDecimal(reader["admins_semanal"]).ToString();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró un administrador con ese ID.");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los datos: " + ex.Message);
                }



            }

        }
        private void CargarDatosComboBox(int? id)
        {

            try
            {
                var conexion = new DBConexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idAdminObra", id);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            CbxObra.Items.Clear();
                            while (reader.Read())
                            {
                                CbxObra.Items.Add(reader["obra_nombre"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }
        private void GuardarDatosAdmindb(int? idAdmin)
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"UPDATE admins SET 
                            admins_nombre = @nombre,
                            admins_apellidop = @apellidop,
                            admins_apellidom = @apellidom,
                            admins_correo = @correo,
 
                            admins_semanal = @semanal,
                            admin_categoria = @categoria

                         WHERE id_admins = @idAdmin";

                SqlCommand command = new SqlCommand(query, connection);

                // Parámetros
                command.Parameters.AddWithValue("@nombre", TxtNombre.Text.Trim());
                command.Parameters.AddWithValue("@apellidop", TxtApellidoP.Text.Trim());
                command.Parameters.AddWithValue("@apellidom", TxtApellidoM.Text.Trim());
                command.Parameters.AddWithValue("@correo", TxtCorreo.Text.Trim());


                if (decimal.TryParse(TxtSueldoSemanal.Text, out decimal semanal))
                {
                    command.Parameters.AddWithValue("@semanal", semanal);
                }
                else
                {
                    MessageBox.Show("Por favor, ingresa un valor numérico válido para el sueldo semanal.");
                    return;
                }


                command.Parameters.AddWithValue("@categoria", TxtCategoria.Text.Trim());
                command.Parameters.AddWithValue("@idAdmin", idAdmin);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Datos del Administrador actualizados correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el Administrador para actualizar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar los datos: " + ex.Message);
                }
            }
        }

    }
}
