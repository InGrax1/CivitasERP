using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CivitasERP.Models;
using System.Windows;
using System.IO.Packaging;
using CivitasERP.Views;
using static CivitasERP.Models.DataGridNominas;
using System.Data.SqlTypes;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Models
{
    class Datagrid_lista
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
            List<Empleado_Asistencia> empleados = new List<Empleado_Asistencia>();


            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string usuario = GlobalVariables1.usuario;
                DB_admins dB_Admins = new DB_admins();
                int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

                // Aquí estás verificando si se ha guardado un id_obra globalmente
                int? id_obra;

                if (GlobalVariables1.id_obra != null)
                {
                    id_obra = GlobalVariables1.id_obra;
                }
                else
                {
                    id_obra = 0; // cuidado: esto convierte el null a 0, podría ser ambiguo si 0 no es válido
                }

                string fechaInicio = "", fechaFin = "";

                fechaInicio = GlobalVariables1.fecha_inicio;
                fechaFin = GlobalVariables1.fecha_fin;

                MessageBox.Show(fechaInicio);
                MessageBox.Show(fechaFin);
                string query = @"SELECT id_admins, 
                            CONCAT(admins.admins_nombre, ' ', admins.admins_apellidop, ' ', admins.admins_apellidom) AS admin_nombre, 
                            admin_categoria, 
                     FROM admins 
                     WHERE id_admins = @idAdmin";



                SqlCommand cmd1 = new SqlCommand(query, connection);
                cmd1.Parameters.AddWithValue("@idAdmin", idAdmin);
                connection.Open();

                SqlDataReader reader1 = cmd1.ExecuteReader();

                Dictionary<int, Empleado_Asistencia> empleadosDict = new Dictionary<int, Empleado_Asistencia>();

                string fechas;
                fechas = GlobalVariables1.fecha_inicio;

                DateTime fecha = DateTime.Parse(fechas);



                while (reader1.Read())
                {

                    empleados.Add(new Empleado_Asistencia
                    {
                        ID = reader1.GetInt32(0),
                        Nombre = reader1.GetString(1),
                        Categoria = reader1.GetString(2),



                        EntradaL = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha),
                        SalidaL = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha),

                        EntradaM = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha.AddDays(1)),
                        SalidaM = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha.AddDays(1)),

                        EntradaMI = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha.AddDays(2)),
                        SalidaMI = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha.AddDays(2)),

                        EntradaJ = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha.AddDays(3)),
                        SalidaJ = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha.AddDays(3)),

                        EntradaV = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha.AddDays(4)),
                        SalidaV = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha.AddDays(4)),

                        EntradaS = ObtenerHorarioDeEntrada_Admin(reader1.GetInt32(0), fecha.AddDays(5)),
                        SalidaS = ObtenerHorarioDeSalida_Admin(reader1.GetInt32(0), fecha.AddDays(5)),
                    });
                }
                reader1.Close();

            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                
                string usuario = GlobalVariables1.usuario;
                DB_admins dB_Admins = new DB_admins();
                int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

                // Aquí estás verificando si se ha guardado un id_obra globalmente
                int? id_obra;

                if (GlobalVariables1.id_obra != null)
                {
                    id_obra = GlobalVariables1.id_obra;
                }
                else
                {
                    id_obra = 0; // cuidado: esto convierte el null a 0, podría ser ambiguo si 0 no es válido
                }

                string fechaInicio = "", fechaFin = "";

                fechaInicio = GlobalVariables1.fecha_inicio;
                fechaFin = GlobalVariables1.fecha_fin;

                MessageBox.Show(fechaInicio);
                MessageBox.Show(fechaFin);
                string query = @"
                                SELECT DISTINCT e.id_empleado, 
                                    CONCAT(e.emp_nombre, ' ', e.emp_apellidop, ' ', e.emp_apellidom) AS emp_nombre, 
                                    e.emp_puesto, 
                                    e.emp_dia, 
                                    e.emp_semanal, 
                                    e.emp_hora_extra  
                                    FROM empleado e
                                    INNER JOIN asistencia a ON e.id_empleado = a.id_empleado
                                    WHERE e.id_admins = @idAdmin 
                                      AND e.id_obra = @id_obra 
                                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";


                SqlCommand cmd1 = new SqlCommand(query, connection);
                cmd1.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd1.Parameters.AddWithValue("@id_obra", id_obra);
                cmd1.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd1.Parameters.AddWithValue("@fechaFin", fechaFin);
                connection.Open();

                SqlDataReader reader1 = cmd1.ExecuteReader();

                Dictionary<int, Empleado_Asistencia> empleadosDict = new Dictionary<int, Empleado_Asistencia>();

                string fechas;
                fechas =GlobalVariables1.fecha_inicio;

                DateTime fecha = DateTime.Parse(fechas);

                

                while (reader1.Read())
                {

                    empleados.Add(new Empleado_Asistencia
                    {
                        ID = reader1.GetInt32(0),
                        Nombre = reader1.GetString(1),
                        Categoria = reader1.GetString(2),



                        EntradaL = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha),
                        SalidaL = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha),

                        EntradaM = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha.AddDays(1)),
                        SalidaM = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha.AddDays(1)),

                        EntradaMI = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha.AddDays(2)),
                        SalidaMI = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha.AddDays(2)),

                        EntradaJ = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha.AddDays(3)),
                        SalidaJ = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha.AddDays(3)),

                        EntradaV = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha.AddDays(4)),
                        SalidaV = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha.AddDays(4)),

                        EntradaS = ObtenerHorarioDeEntrada(reader1.GetInt32(0), fecha.AddDays(5)),
                        SalidaS = ObtenerHorarioDeSalida(reader1.GetInt32(0), fecha.AddDays(5)),
                    });
                }
                reader1.Close();

            }

            return empleados;
        }

        public TimeSpan? ObtenerHorarioDeEntrada(int idEmpleado, DateTime fecha)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;


            TimeSpan? horaEntrada = null;


            string query = @"
            SELECT asis_hora, asis_salida
            FROM asistencia
           WHERE id_empleado = @id_empleado AND CAST(asis_dia AS DATE) = @asis_dia";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id_empleado", idEmpleado);
                cmd.Parameters.AddWithValue("@asis_dia", fecha.Date);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            horaEntrada = reader.GetTimeSpan(reader.GetOrdinal("asis_hora"));

                       
                    }
                }
            }
            return (horaEntrada);
        }

        public TimeSpan? ObtenerHorarioDeSalida(int idEmpleado, DateTime fecha)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;


            TimeSpan? horaSalida = null;

            string query = @"
            SELECT asis_hora, asis_salida
            FROM asistencia
           WHERE id_empleado = @id_empleado AND CAST(asis_dia AS DATE) = @asis_dia";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id_empleado", idEmpleado);
                cmd.Parameters.AddWithValue("@asis_dia", fecha.Date);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(1))
                            horaSalida = reader.GetTimeSpan(reader.GetOrdinal("asis_salida"));
                    }
                }
            }
            return (horaSalida);
        }





        public TimeSpan? ObtenerHorarioDeEntrada_Admin(int idEmpleado, DateTime fecha)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;


            TimeSpan? horaEntrada = null;


            string query = @"
            SELECT asis_hora, asis_salida
            FROM asistencia
           WHERE admins_id_asistencia = @admins_id_asistencia AND CAST(asis_dia AS DATE) = @asis_dia";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@admins_id_asistencia", idEmpleado);
                cmd.Parameters.AddWithValue("@asis_dia", fecha.Date);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            horaEntrada = reader.GetTimeSpan(reader.GetOrdinal("asis_hora"));


                    }
                }
            }
            return (horaEntrada);
        }

        public TimeSpan? ObtenerHorarioDeSalida_Admin(int idEmpleado, DateTime fecha)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;


            TimeSpan? horaSalida = null;

            string query = @"
            SELECT asis_hora, asis_salida
            FROM admins_id_asistencia
           WHERE admins_id_asistencia = @admins_id_asistencia AND CAST(asis_dia AS DATE) = @asis_dia";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@admins_id_asistencia", idEmpleado);
                cmd.Parameters.AddWithValue("@asis_dia", fecha.Date);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(1))
                            horaSalida = reader.GetTimeSpan(reader.GetOrdinal("asis_salida"));
                    }
                }
            }
            return (horaSalida);
        }
        /// <summary>
        /// Marca la asistencia de un empleado:
        /// - Si no existe registro para hoy, inserta asis_hora (hora de entrada).
        /// - Si existe pero asis_salida es NULL, actualiza asis_salida (hora de salida).
        /// </summary>
        /// 
        public void MarcarAsistencia(int idEmpleado)
        {
            // 1) Obtener la cadena de conexión (no uses “asis_id” aquí, solo id_asistencia cuando lo leas)
            string connectionString = new Conexion().ObtenerCadenaConexion();

            // 2) Fecha de hoy (solo la parte DATE)
            DateTime fechaHoy = DateTime.Today;
            TimeSpan horaAhora = DateTime.Now.TimeOfDay;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 3) Intentamos leer el registro de asistencia para este empleado Y esta fecha
                //    Nota que aquí CORREGIMOS “asis_id” por “id_asistencia”
                string sqlSelect = @"
            SELECT id_asistencia, asis_hora, asis_salida
              FROM asistencia
             WHERE id_empleado = @idEmpleado
               AND CAST(asis_dia AS DATE) = @fechaHoy";

                using (SqlCommand cmdSelect = new SqlCommand(sqlSelect, conn))
                {
                    cmdSelect.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                    cmdSelect.Parameters.AddWithValue("@fechaHoy", fechaHoy.Date);

                    using (SqlDataReader reader = cmdSelect.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Ya existe fila de asistencia para “hoy” y este empleado
                            int idAsistencia = reader.GetInt32(reader.GetOrdinal("id_asistencia"));
                            bool tieneSalida = !reader.IsDBNull(reader.GetOrdinal("asis_salida"));
                            reader.Close();

                            if (!tieneSalida)
                            {
                                // Aún no ha marcado la salida: hacemos UPDATE para poner “asis_salida”
                                string sqlUpdate = @"
                            UPDATE asistencia
                               SET asis_salida = @horaAhora
                             WHERE id_asistencia = @idAsistencia
                        ";
                                using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@horaAhora", horaAhora);
                                    cmdUpdate.Parameters.AddWithValue("@idAsistencia", idAsistencia);
                                    cmdUpdate.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Si detectas que ya tenía asis_salida (tercera pasada), 
                                // tal vez quieras mostrar un mensaje informando que no puede volver a marcar:
                                MessageBox.Show(
                                    "Ya registraste tu salida para hoy. No se puede volver a marcar.",
                                    "Atención",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            // reader.Read() = false -> no hay fila para hoy: INSERT para marcar la hora de entrada
                            reader.Close();

                            string sqlInsert = @"
                        INSERT INTO asistencia 
                            (id_empleado, asis_dia, asis_hora, asis_salida, asis_hora_extra, admins_id_asistencia) 
                        VALUES 
                            (@idEmpleado, @fechaHoy, @horaAhora, NULL, '00:00:00', NULL)
                    ";
                            using (SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                                cmdInsert.Parameters.AddWithValue("@fechaHoy", fechaHoy.Date);
                                cmdInsert.Parameters.AddWithValue("@horaAhora", horaAhora);
                                // En este ejemplo no asignamos admins_id_asistencia; si quieres grabar qué admin marcó,
                                // reemplaza el NULL por tu propio parámetro: e.g.  @idAdminQueMarca
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }

                conn.Close();
            }
        }

    }

}