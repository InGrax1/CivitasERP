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
using System.Windows.Shapes;
using CivitasERP.Models;
using System.Collections;

namespace CivitasERP.AdminViews
{
    /// <summary>
    /// Lógica de interacción para prueba.xaml
    /// </summary>
    public partial class prueba : Window
    {
        private SqlDataAdapter adaptador;
        private DataTable tablaAdmins;

        private string connectionString = "Server=INGRAX;Database=CivitasERP;Integrated Security=True;";
        public prueba()
        {
            InitializeComponent();
        }


        private void CargarAdmins_Click(object sender, RoutedEventArgs e)
        {
            int? idAdmin;
            idAdmin = Variables.IdAdmin;

            // 1. Calcular lunes y domingo de la semana actual
            DateTime hoy = DateTime.Now.Date;
            int diferencia = (int)hoy.DayOfWeek == 0 ? 6 : ((int)hoy.DayOfWeek - 1); // Domingo=0
            DateTime fechaInicio = hoy.AddDays(-diferencia); // Lunes
            DateTime fechaFin = fechaInicio.AddDays(6);      // Domingo

            // 2. Consulta de datos del admin con asistencia más reciente de la semana
            string query = @"
                            SELECT 
                                adm.id_admins,
                                adm.admins_nombre,
                                adm.admins_apellidop,
                                adm.admins_apellidom,
                                adm.admin_categoria,
                                asi.asis_hora_extra AS HorasExtra
                            FROM admins AS adm
                            LEFT JOIN (
                                SELECT TOP 1 *
                                FROM asistencia
                                WHERE admins_id_asistencia = @idEmpleado
                                  AND asis_hora_extra IS NOT NULL
                                  AND asis_dia BETWEEN @fechaInicio AND @fechaFin
                                ORDER BY asis_dia DESC
                            ) AS asi
                                ON asi.admins_id_asistencia = adm.id_admins
                            WHERE adm.id_admins = @idEmpleado
                        ";

            tablaAdmins = new DataTable();
            using (var cn = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter(query, cn))
            {
                da.SelectCommand.CommandType = CommandType.Text;
                da.SelectCommand.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                da.SelectCommand.Parameters.AddWithValue("@fechaFin", fechaFin);
                da.SelectCommand.Parameters.AddWithValue("@idEmpleado", idAdmin);

                da.Fill(tablaAdmins);
                adaptador = da;
            }

            // 3. Agregar columnas para días de la semana
            string[] diasSemana = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            foreach (var dia in diasSemana)
            {
                if (!tablaAdmins.Columns.Contains(dia))
                    tablaAdmins.Columns.Add(dia, typeof(int));
            }

            // 4. Obtener días trabajados dentro de esta semana
            var diasTrabajados = new HashSet<DayOfWeek>();

            using (var conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string consultaDias = @"
                                    SELECT CAST(asis_dia AS DATE) AS DiaAsistido
                                    FROM asistencia
                                    WHERE admins_id_asistencia = @id_empleado
                                      AND asis_dia BETWEEN @fechaInicio AND @fechaFin";

                using (SqlCommand cmd = new SqlCommand(consultaDias, conexion))
                {
                    cmd.Parameters.AddWithValue("@id_empleado", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                    using (SqlDataReader lector = cmd.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            diasTrabajados.Add(lector.GetDateTime(0).DayOfWeek);
                        }
                    }
                }
            }

            // 5. Rellenar el DataTable
            foreach (DataRow row in tablaAdmins.Rows)
            {
                foreach (var dia in diasSemana)
                {
                    row[dia] = 0;
                }

                foreach (var diaTrabajado in diasTrabajados)
                {
                    string columna = diaTrabajado switch
                    {
                        DayOfWeek.Monday => "Lunes",
                        DayOfWeek.Tuesday => "Martes",
                        DayOfWeek.Wednesday => "Miércoles",
                        DayOfWeek.Thursday => "Jueves",
                        DayOfWeek.Friday => "Viernes",
                        DayOfWeek.Saturday => "Sábado",
                        DayOfWeek.Sunday => "Domingo",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(columna))
                        row[columna] = 1;
                }
            }

            // 6. Mostrar resultados en el DataGrid
            AdminDataGrid.ItemsSource = tablaAdmins.DefaultView;
        }


        private void CargarEmpleados_Click(object sender, RoutedEventArgs e)
        {
            int? idAdmin;
            idAdmin = Variables.IdAdmin;

            // 1. Calcular semana actual (lunes a domingo)
            DateTime hoy = DateTime.Now.Date;
            int diferencia = (int)hoy.DayOfWeek == 0 ? 6 : ((int)hoy.DayOfWeek - 1);
            DateTime fechaInicio = hoy.AddDays(-diferencia); // Lunes
            DateTime fechaFin = fechaInicio.AddDays(6);      // Domingo

            // 2. Consulta de todos los empleados del admin con hora extra más reciente
            string query = @"
                        SELECT 
                            emp.id_empleado,
                            emp.emp_nombre,
                            emp.emp_apellidop,
                            emp.emp_apellidom,
                            emp.emp_dia,
                            emp.emp_semanal,
                            emp.emp_hora_extra,
                            asi.asis_hora_extra AS HorasExtra
                        FROM empleado AS emp
                        LEFT JOIN (
                            SELECT a.*
                            FROM asistencia a
                            INNER JOIN (
                                SELECT id_empleado, MAX(asis_dia) AS UltimaFecha
                                FROM asistencia
                                WHERE asis_dia BETWEEN @fechaInicio AND @fechaFin
                                GROUP BY id_empleado
                            ) ult ON a.id_empleado = ult.id_empleado AND a.asis_dia = ult.UltimaFecha
                            WHERE a.asis_hora_extra IS NOT NULL
                        ) AS asi
                            ON asi.id_empleado = emp.id_empleado
                        WHERE emp.id_admins = @idAdmin
                    ";

            tablaAdmins = new DataTable();
            using (var cn = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter(query, cn))
            {
                da.SelectCommand.CommandType = CommandType.Text;
                da.SelectCommand.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                da.SelectCommand.Parameters.AddWithValue("@fechaFin", fechaFin);
                da.SelectCommand.Parameters.AddWithValue("@idAdmin", idAdmin);

                da.Fill(tablaAdmins);
                adaptador = da;
            }

            // 3. Agregar columnas de lunes a domingo
            string[] diasSemana = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            foreach (var dia in diasSemana)
            {
                if (!tablaAdmins.Columns.Contains(dia))
                    tablaAdmins.Columns.Add(dia, typeof(int));
            }

            // 4. Obtener asistencias por empleado en la semana
            using (var conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                foreach (DataRow row in tablaAdmins.Rows)
                {
                    int idEmpleado = Convert.ToInt32(row["id_empleado"]);

                    // Inicializar días en 0
                    foreach (var dia in diasSemana)
                        row[dia] = 0;

                    string consultaDias = @"
                                        SELECT CAST(asis_dia AS DATE) AS DiaAsistido
                                        FROM asistencia
                                        WHERE id_empleado = @id_empleado
                                          AND asis_dia BETWEEN @fechaInicio AND @fechaFin";

                    using (SqlCommand cmd = new SqlCommand(consultaDias, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id_empleado", idEmpleado);
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                        using (SqlDataReader lector = cmd.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                DayOfWeek diaTrabajado = lector.GetDateTime(0).DayOfWeek;

                                string columna = diaTrabajado switch
                                {
                                    DayOfWeek.Monday => "Lunes",
                                    DayOfWeek.Tuesday => "Martes",
                                    DayOfWeek.Wednesday => "Miércoles",
                                    DayOfWeek.Thursday => "Jueves",
                                    DayOfWeek.Friday => "Viernes",
                                    DayOfWeek.Saturday => "Sábado",
                                    DayOfWeek.Sunday => "Domingo",
                                    _ => ""
                                };

                                if (!string.IsNullOrEmpty(columna))
                                    row[columna] = 1;
                            }
                        }
                    }
                }
            }

            // 5. Mostrar resultado en DataGrid
            AdminDataGrid.ItemsSource = tablaAdmins.DefaultView;
        }

        private void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {
            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            Variables.IdAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
            int? idAdminObra = Variables.IdAdmin;

            try
            {
                var conexion = new Conexion();
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

        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var dB_Admins = new DB_admins();
            Variables.IdAdmin = dB_Admins.ObtenerIdPorUsuario(Variables.Usuario);
            int? idAdminObra = Variables.IdAdmin;
            MessageBox.Show("ID Admin Obra: " + idAdminObra);
            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
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

        private void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string usuario = AdminComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(usuario))
            {
                var dB_Admins = new DB_admins();
                int? id = dB_Admins.ObtenerIdPorUsuario(usuario);
                Variables.IdAdmin = id;
                MessageBox.Show("ID del admin seleccionado: " + id);
            }
        }

        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void GuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            if (tablaAdmins == null || adaptador == null)
            {
                MessageBox.Show("No hay datos cargados para guardar.");
                return;
            }

            string connectionString = "Server=INGRAX;Database=CivitasERP;Integrated Security=True;";

            using (var conexion = new SqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // Asignar la conexión al adaptador
                    adaptador.SelectCommand.Connection = conexion;

                    // Generar comandos para INSERT, UPDATE y DELETE si es necesario
                    var builder = new SqlCommandBuilder(adaptador);
                    adaptador.UpdateCommand = builder.GetUpdateCommand();
                    adaptador.InsertCommand = builder.GetInsertCommand();
                    adaptador.DeleteCommand = builder.GetDeleteCommand();

                    // Guardar los cambios del DataTable en la base de datos
                    adaptador.Update(tablaAdmins);

                    MessageBox.Show("Cambios guardados en la base de datos.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar cambios: " + ex.Message);
                }
            }
        }


    }
}
