using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CivitasERP.Models;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.IO.Packaging;
using CivitasERP.Views;
using static CivitasERP.Models.DataGridNominas;
using System.Data.SqlTypes;
using static CivitasERP.Views.LoginPage;
using static CivitasERP.Views.HomePage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CivitasERP.Models
{
    class DataGridNominas
    {
        public class Empleado
        {
            public int ID { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            /*public int DiasTrabajados { get; set; }*/
            public int Dias { get; set; }
            public decimal SueldoJornada { get; set; }
            public decimal SueldoSemanal { get; set; }
            public decimal HorasExtra { get; set; }
            public decimal PHoraExtra { get; set; }
            public decimal SuelExtra { get; set; }
            public decimal SuelTrabajado { get; set; }
            public decimal SuelTotal { get; set; }


        }

        private readonly string connectionString;

        public DataGridNominas(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Empleado> ObtenerEmpleados()
        {
            List<Empleado> empleados = new List<Empleado>();


            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;
            
            
            
            
            //admins
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string usuario;
                usuario = Variables.Usuario;
                DB_admins dB_Admins = new DB_admins();
                int? idAdmin;
                idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

                string query = @"SELECT id_admins, 
                            CONCAT(admins.admins_nombre, ' ', admins.admins_apellidop, ' ', admins.admins_apellidom) AS admin_nombre, 
                            admin_categoria, 
                            admins_semanal
                     FROM admins 
                     WHERE id_admins = @idAdmin";

                SqlCommand cmd1 = new SqlCommand(query, connection);
                cmd1.Parameters.AddWithValue("@idAdmin", idAdmin);

                connection.Open();

                SqlDataReader reader1 = cmd1.ExecuteReader();

                Dictionary<int, Empleado> empleadosDict = new Dictionary<int, Empleado>();

                while (reader1.Read())
                {
                    empleados.Add(new Empleado
                    {
                        ID = reader1.GetInt32(0),
                        Nombre = reader1.GetString(1),
                        Categoria = reader1.GetString(2),
                        SueldoJornada = Convert.ToDecimal(reader1.GetValue(3))/6,
                        SueldoSemanal = Convert.ToDecimal(reader1.GetValue(3)),
                        PHoraExtra = (Convert.ToDecimal(reader1.GetValue(3)) / 6) /8,
                        Dias = CalcularDiasTrabajados_Admin(reader1.GetInt32(0)),
                        HorasExtra = ObtenerTotalHorasExtra_Admin(reader1.GetInt32(0)),
                        SuelExtra = ((Convert.ToDecimal(reader1.GetValue(3)) / 6) / 8) * ObtenerTotalHorasExtra_Admin(reader1.GetInt32(0)),
                        SuelTrabajado = (Convert.ToDecimal(reader1.GetValue(3))/6) * CalcularDiasTrabajados_Admin(reader1.GetInt32(0)),
                        SuelTotal = ( ((Convert.ToDecimal(reader1.GetValue(3)) / 6) / 8) * ObtenerTotalHorasExtra_Admin(reader1.GetInt32(0))) +
                                    (Convert.ToDecimal(reader1.GetValue(3)) / 6 * CalcularDiasTrabajados_Admin(reader1.GetInt32(0)))
                    });
                }

                reader1.Close();
            }


            //empleados
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string usuario = Variables.Usuario;
                DB_admins dB_Admins = new DB_admins();
                int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

                // Aquí estás verificando si se ha guardado un id_obra globalmente
                int? id_obra;

                if (Variables.IdObra != null)
                {
                    id_obra = Variables.IdObra;
                }
                else
                {
                    id_obra = 0; // cuidado: esto convierte el null a 0, podría ser ambiguo si 0 no es válido
                }

                string fechaInicio="",fechaFin="";

                fechaInicio = Variables.FechaInicio;
                fechaFin = Variables.FechaFin;


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
                                   ";

                SqlCommand cmd1 = new SqlCommand(query, connection);
                cmd1.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd1.Parameters.AddWithValue("@id_obra", id_obra);
                cmd1.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd1.Parameters.AddWithValue("@fechaFin", fechaFin);
                connection.Open();

                SqlDataReader reader1 = cmd1.ExecuteReader();

                Dictionary<int, Empleado> empleadosDict = new Dictionary<int, Empleado>();

                while (reader1.Read())
                {

                    
                    empleados.Add(new Empleado
                    {
                        ID = reader1.GetInt32(0),
                        Nombre = reader1.GetString(1),
                        Categoria = reader1.GetString(2),
                        SueldoJornada = Convert.ToDecimal(reader1.GetValue(3)),
                        SueldoSemanal = Convert.ToDecimal(reader1.GetValue(4)),
                        PHoraExtra = Convert.ToDecimal(reader1.GetValue(5)),
                        Dias = CalcularDiasTrabajados(reader1.GetInt32(0)),
                        HorasExtra = ObtenerTotalHorasExtra(reader1.GetInt32(0)),
                        SuelExtra = Convert.ToDecimal(reader1.GetValue(5)) * ObtenerTotalHorasExtra(reader1.GetInt32(0)),
                        SuelTrabajado = Convert.ToDecimal(reader1.GetValue(3)) * CalcularDiasTrabajados(reader1.GetInt32(0)),
                        SuelTotal = (Convert.ToDecimal(reader1.GetValue(5)) * ObtenerTotalHorasExtra(reader1.GetInt32(0))) +
                                    (Convert.ToDecimal(reader1.GetValue(3)) * CalcularDiasTrabajados(reader1.GetInt32(0)))
                    });

                }

                reader1.Close();
            }

            return empleados;
        }




        // EMPLEADOS
        public int CalcularDiasTrabajados(int idEmpleado)
        {
            var fechasUnicas = new HashSet<DateTime>();

            Conexion Sconexion = new Conexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            string fechaInicio="", fechaFin="";

            fechaInicio = Variables.FechaInicio;
            fechaFin = Variables.FechaFin;


            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
            SELECT CAST(asis_dia AS DATE) AS DiaAsistido
            FROM asistencia
            WHERE id_empleado = @id_empleado
              AND asis_dia BETWEEN @fechaInicio AND @fechaFin
        ";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {

                    comando.Parameters.AddWithValue("@id_empleado", idEmpleado);
                    comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@fechaFin", fechaFin);


                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            DateTime dia = lector.GetDateTime(0);
                            fechasUnicas.Add(dia); // Asegura contar solo un día único
                        }
                    }
                }
            }
            return fechasUnicas.Count;
        }


            public decimal ObtenerTotalHorasExtra(int idEmpleado)
        {
            decimal totalHoras = 0;

            Conexion Sconexion = new Conexion();
            string connectionString = new Conexion().ObtenerCadenaConexion();

            string fechaInicio = "", fechaFin = "";

            fechaInicio = Variables.FechaInicio;
            fechaFin = Variables.FechaFin;

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
                                SELECT asis_hora_extra
                                FROM asistencia
                                WHERE id_empleado = @id_empleado AND asis_hora_extra IS NOT NULL
              AND asis_dia BETWEEN @fechaInicio AND @fechaFin
        ";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@id_empleado", idEmpleado);
                    comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@fechaFin", fechaFin);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            TimeSpan horaExtra = lector.GetTimeSpan(0);
                            totalHoras += (decimal)horaExtra.TotalHours; // convierte a decimal
                        }
                    }
                }
            }

            return totalHoras;
        }



        //admins

        public int CalcularDiasTrabajados_Admin(int idAdmin)
        {
            var fechasUnicas = new HashSet<DateTime>();

            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            string fechaInicio = "", fechaFin = "";


            fechaInicio = Variables.FechaInicio;
            fechaFin = Variables.FechaFin;

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
                SELECT CAST(asis_dia AS DATE) AS DiaAsistido
                FROM asistencia
                WHERE admins_id_asistencia = @id_admin
              AND asis_dia BETWEEN @fechaInicio AND @fechaFin
";


                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.Add("@id_admin", SqlDbType.Int).Value = idAdmin;
                    comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@fechaFin", fechaFin);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            DateTime dia = lector.GetDateTime(0);
                            fechasUnicas.Add(dia); // Asegura contar solo un día único
                        }
                    }
                }
            }

            return fechasUnicas.Count;

        }

        public decimal ObtenerTotalHorasExtra_Admin(int idAdmin)
        {
            decimal totalHoras = 0;

            Conexion Sconexion = new Conexion();
            string connectionString = new Conexion().ObtenerCadenaConexion();

            string fechaInicio = "", fechaFin = "";

            fechaInicio = Variables.FechaInicio;
            fechaFin = Variables.FechaFin;

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
                                SELECT asis_hora_extra
                                FROM asistencia
                                WHERE admins_id_asistencia = @id_admin AND asis_hora_extra IS NOT NULL
              AND asis_dia BETWEEN @fechaInicio AND @fechaFin
        ";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.Add("@id_admin", SqlDbType.Int).Value = idAdmin;
                    comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@fechaFin", fechaFin);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            TimeSpan horaExtra = lector.GetTimeSpan(0);
                            totalHoras += (decimal)horaExtra.TotalHours; // convierte a decimal
                        }
                    }
                }
            }

            return totalHoras;
        }
        public (bool exito, DateTime fechaInicio, DateTime fechaFin) ObtenerFechasDesdeGlobal()
        {
            string texto = Variables.Fecha;

            if (string.IsNullOrWhiteSpace(texto))
                return (false, DateTime.MinValue, DateTime.MinValue);

            string[] partes = texto.Split(" - ");

            if (partes.Length == 2 &&
                DateTime.TryParseExact(partes[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime inicio) &&
                DateTime.TryParseExact(partes[1], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fin))
            {
                return (true, inicio, fin);
            }

            return (false, DateTime.MinValue, DateTime.MinValue);
        }
    }
}
    
