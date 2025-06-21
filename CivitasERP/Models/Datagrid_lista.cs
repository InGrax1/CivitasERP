using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using CivitasERP.Views;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Models
{
    public class Datagrid_lista
    {
        public class Empleado_Asistencia
        {
            public int ID { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }

            public TimeSpan? EntradaL { get; set; }
            public TimeSpan? SalidaL { get; set; }

            public TimeSpan? EntradaM { get; set; }
            public TimeSpan? SalidaM { get; set; }

            public TimeSpan? EntradaMI { get; set; }
            public TimeSpan? SalidaMI { get; set; }

            public TimeSpan? EntradaJ { get; set; }
            public TimeSpan? SalidaJ { get; set; }

            public TimeSpan? EntradaV { get; set; }
            public TimeSpan? SalidaV { get; set; }

            public TimeSpan? EntradaS { get; set; }
            public TimeSpan? SalidaS { get; set; }
        }

        private readonly string connectionString;

        public Datagrid_lista(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Empleado_Asistencia> ObtenerEmpleados()
        {
            var empleados = new List<Empleado_Asistencia>();

            string connStr = new Conexion().ObtenerCadenaConexion();
            string usuario = Variables.Usuario;
            int? idAdmin = new DB_admins().ObtenerIdPorUsuario(usuario);
            int? idObra = Variables.IdObra ?? 0;
            DateTime fechaInicio = DateTime.Parse(Variables.FechaInicio);

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();

                // Cargar Admins
                string queryAdmins = @"SELECT id_admins, CONCAT(admins_nombre, ' ', admins_apellidop, ' ', admins_apellidom) AS admin_nombre, admin_categoria 
                                       FROM admins WHERE id_admins = @idAdmin";

                using (SqlCommand cmd = new SqlCommand(queryAdmins, connection))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            empleados.Add(CrearEmpleadoAsistencia(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), fechaInicio, true));
                        }
                    }
                }

                // Cargar Empleados
                string queryEmpleados = @"
                    SELECT DISTINCT e.id_empleado, 
                        CONCAT(e.emp_nombre, ' ', e.emp_apellidop, ' ', e.emp_apellidom) AS emp_nombre, 
                        e.emp_puesto
                    FROM empleado e
                    LEFT JOIN asistencia a ON e.id_empleado = a.id_empleado
                    WHERE e.id_admins = @idAdmin AND e.id_obra = @idObra";

                using (SqlCommand cmd = new SqlCommand(queryEmpleados, connection))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@idObra", idObra);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            empleados.Add(CrearEmpleadoAsistencia(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), fechaInicio, false));
                        }
                    }
                }
            }

            return empleados;
        }

        private Empleado_Asistencia CrearEmpleadoAsistencia(int id, string nombre, string categoria, DateTime fecha, bool esAdmin)
        {
            return new Empleado_Asistencia
            {
                ID = id,
                Nombre = nombre,
                Categoria = categoria,
                EntradaL = ObtenerHorario(id, fecha, esAdmin, true),
                SalidaL = ObtenerHorario(id, fecha, esAdmin, false),
                EntradaM = ObtenerHorario(id, fecha.AddDays(1), esAdmin, true),
                SalidaM = ObtenerHorario(id, fecha.AddDays(1), esAdmin, false),
                EntradaMI = ObtenerHorario(id, fecha.AddDays(2), esAdmin, true),
                SalidaMI = ObtenerHorario(id, fecha.AddDays(2), esAdmin, false),
                EntradaJ = ObtenerHorario(id, fecha.AddDays(3), esAdmin, true),
                SalidaJ = ObtenerHorario(id, fecha.AddDays(3), esAdmin, false),
                EntradaV = ObtenerHorario(id, fecha.AddDays(4), esAdmin, true),
                SalidaV = ObtenerHorario(id, fecha.AddDays(4), esAdmin, false),
                EntradaS = ObtenerHorario(id, fecha.AddDays(5), esAdmin, true),
                SalidaS = ObtenerHorario(id, fecha.AddDays(5), esAdmin, false),
            };
        }

        private TimeSpan? ObtenerHorario(int id, DateTime fecha, bool esAdmin, bool esEntrada)
        {
            TimeSpan? resultado = null;
            string campo = esEntrada ? "asis_hora" : "asis_salida";
            string columnaID = esAdmin ? "admins_id_asistencia" : "id_empleado";

            string query = $"SELECT {campo} FROM asistencia WHERE {columnaID} = @id AND CAST(asis_dia AS DATE) = @fecha";

            using (SqlConnection conn = new SqlConnection(new Conexion().ObtenerCadenaConexion()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                        resultado = reader.GetTimeSpan(0);
                }
            }

            return resultado;
        }

        public void MarcarAsistencia(int idEmpleado, bool esAdmin = false)
        {
            string cs = new Conexion().ObtenerCadenaConexion();
            DateTime hoy = DateTime.Today;
            TimeSpan ahora = DateTime.Now.TimeOfDay;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                string columnaID = esAdmin ? "admins_id_asistencia" : "id_empleado";

                string select = $@"
                    SELECT id_asistencia, asis_hora, asis_salida
                    FROM asistencia
                    WHERE {columnaID} = @id AND CAST(asis_dia AS DATE) = @hoy";

                using (SqlCommand cmdSelect = new SqlCommand(select, conn))
                {
                    cmdSelect.Parameters.AddWithValue("@id", idEmpleado);
                    cmdSelect.Parameters.AddWithValue("@hoy", hoy);

                    using (SqlDataReader reader = cmdSelect.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int idAsistencia = reader.GetInt32(0);
                            bool tieneSalida = !reader.IsDBNull(2);
                            reader.Close();

                            if (!tieneSalida)
                            {
                                string update = @"UPDATE asistencia SET asis_salida = @ahora WHERE id_asistencia = @idAsistencia";
                                using (SqlCommand cmdUpdate = new SqlCommand(update, conn))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@ahora", ahora);
                                    cmdUpdate.Parameters.AddWithValue("@idAsistencia", idAsistencia);
                                    cmdUpdate.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Ya registraste tu salida para hoy.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            reader.Close();
                            string insert = $@"
                                INSERT INTO asistencia 
                                    ({columnaID}, asis_dia, asis_hora, asis_salida, asis_hora_extra, {(esAdmin ? "id_empleado" : "admins_id_asistencia")})
                                VALUES
                                    (@id, @hoy, @ahora, NULL, '00:00:00', NULL)";
                            using (SqlCommand cmdInsert = new SqlCommand(insert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@id", idEmpleado);
                                cmdInsert.Parameters.AddWithValue("@hoy", hoy);
                                cmdInsert.Parameters.AddWithValue("@ahora", ahora);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        public void MarcarAsistenciaAdmin(int idAdmin)
        {
            MarcarAsistencia(idAdmin, true);
        }
    }
}
