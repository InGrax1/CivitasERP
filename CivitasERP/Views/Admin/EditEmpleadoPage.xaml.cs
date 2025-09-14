using CivitasERP.Models;
using CivitasERP.Conexiones;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace CivitasERP.AdminViews
{
    /// <summary>
    /// Lógica de interacción para EditEmpleadoPage.xaml
    /// </summary>
    public partial class EditEmpleadoPage : Page
    {
        public EditEmpleadoPage()
        {
            InitializeComponent();
            cargar_admin();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validar que hayan seleccionado un empleado
            if (EmpleadoComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Por favor selecciona un empleado.",
                    "Campos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            GuardarDatosEmpleado(ObtenerIdEmpleado(EmpleadoComboBox.SelectedItem.ToString()));
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
                            Admin2ComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                Admin2ComboBox.Items.Add(reader["admin_nombre"].ToString());
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


        private void cargar_empleados(int? id_admin,int? id_obra)
        {
            try
            {
                var conexion = new DBConexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    // Suponiendo que quieres obtener todos los admins, o filtrar de alguna forma
                    string query = @"SELECT
                            CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) AS emple_nombre
                     FROM empleado 
                     WHERE 
                        id_admins =@Id_admin
                        AND id_obra=@Id_obra;
                        ";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id_admin", id_admin);
                        cmd.Parameters.AddWithValue("@Id_obra", id_obra);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            EmpleadoComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                EmpleadoComboBox.Items.Add(reader["emple_nombre"].ToString());
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
        private void CargarDatosEmpleado(int? idEmpleado)
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT emp_nombre, emp_apellidop, emp_apellidom,  emp_semanal, 
                                 emp_puesto 
                         FROM empleado 
                         WHERE id_empleado = @idEmpleado";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@idEmpleado", idEmpleado);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        TxtNombre.Text = reader["emp_nombre"].ToString();
                        TxtApellidoP.Text = reader["emp_apellidop"].ToString();
                        TxtApellidoM.Text = reader["emp_apellidom"].ToString();
                        
                        TxtSueldoSemanal.Text = Convert.ToDecimal(reader["emp_semanal"]).ToString("F2");

                        TxtCategoria.Text = reader["emp_puesto"].ToString();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró un empleado con ese ID.");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los datos del empleado: " + ex.Message);
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
                            ObraComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                ObraComboBox.Items.Add(reader["obra_nombre"].ToString());
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


        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre))
                return null;

            var conexion = new DBConexion();
            string connectionString = conexion.ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                    cmd.Parameters.AddWithValue("@obraNombre", obraNombre);
                    conn.Open();

                    object resultado = cmd.ExecuteScalar();
                    if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                        return idObra;

                    return null;
                }
            }
        }


        private void Admin2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Admin2ComboBox.SelectedItem != null)
            {
                string admin = Admin2ComboBox.SelectedItem.ToString();
                DB_admins admins = new DB_admins();

                // Limpia selección actual
                EmpleadoComboBox.SelectedItem = null;
                EmpleadoComboBox.Items.Clear();
                ObraComboBox.SelectedItem = null;
                ObraComboBox.Items.Clear();

                CargarDatosComboBox(admins.ObtenerIdPorUsuario(admin));
            }

        }
        private void Admin2ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            cargar_admin();
        }



        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (EmpleadoComboBox.SelectedItem != null)
            {
                EmpleadoComboBox.SelectedItem = null;  // Deselecciona para evitar errores
                EmpleadoComboBox.Items.Clear();
            }

            if (Admin2ComboBox.SelectedItem != null && ObraComboBox.SelectedItem != null)
            {
                string admin = Admin2ComboBox.SelectedItem.ToString();
                DB_admins admins = new DB_admins();
                int? id_obra = ObtenerID_obra(admins.ObtenerIdPorUsuario(admin), ObraComboBox.SelectedItem.ToString());

                cargar_empleados(admins.ObtenerIdPorUsuario(admin), id_obra);
            }


        }
        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {

        }



        private void EmpleadoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmpleadoComboBox.SelectedItem == null)
                return;

            string nombreEmpleado = EmpleadoComboBox.SelectedItem.ToString();

            // Validar que el item seleccionado aún exista en la lista
            if (!EmpleadoComboBox.Items.Contains(nombreEmpleado))
                return;

            if (Admin2ComboBox.SelectedItem != null && ObraComboBox.SelectedItem != null)
            {
                string admin = Admin2ComboBox.SelectedItem.ToString();
                DB_admins admins = new DB_admins();
                int? id_obra = ObtenerID_obra(admins.ObtenerIdPorUsuario(admin), ObraComboBox.SelectedItem.ToString());

                int? idEmpleado = ObtenerIdEmpleado(nombreEmpleado);
                if (idEmpleado != null)
                {
                    CargarDatosEmpleado(idEmpleado);
                }
            }
        }
        private void EmpleadoComboBox_DropDownOpened(object sender, EventArgs e)
        {

        }

        public int? ObtenerIdEmpleado(string nombre)
        {
            int? idEmpleado = null;

            string query = @"SELECT id_empleado ,CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) AS emple_nombre
                         FROM empleado 
                         WHERE CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) = @nombre ;";
            DBConexion conexion = new DBConexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nombre", nombre);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        idEmpleado = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    // Manejo del error: podrías loguear o lanzar la excepción
                    Console.WriteLine("Error al obtener el ID del empleado: " + ex.Message);
                }
            }

            return idEmpleado;
        }

        public void GuardarDatosEmpleado(int? idEmpleado)
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"UPDATE empleado
                         SET emp_nombre = @nombre,
                             emp_apellidop = @apellidop,
                             emp_apellidom = @apellidom,
                             emp_semanal = @semanal,
                             emp_puesto = @puesto
                         WHERE id_empleado = @idEmpleado";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Asignar los valores desde los TextBox
                    command.Parameters.AddWithValue("@nombre", TxtNombre.Text.Trim());
                    command.Parameters.AddWithValue("@apellidop", TxtApellidoP.Text.Trim());
                    command.Parameters.AddWithValue("@apellidom", TxtApellidoM.Text.Trim());

                    // Asegúrate de que el valor puede convertirse a decimal
                    if (decimal.TryParse(TxtSueldoSemanal.Text, out decimal sueldoSemanal))
                    {
                        command.Parameters.AddWithValue("@semanal", sueldoSemanal);
                    }
                    else
                    {
                        MessageBox.Show("El sueldo semanal no es válido.");
                        return;
                    }

                    command.Parameters.AddWithValue("@puesto", TxtCategoria.Text.Trim());
                    command.Parameters.AddWithValue("@idEmpleado", idEmpleado);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Datos del Empleado actualizados correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se Encontró el empleado para actualizar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar los datos del empleado: " + ex.Message);
                    }
                }
            }
        }
    }
}
