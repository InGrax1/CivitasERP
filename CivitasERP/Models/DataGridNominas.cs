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
            public int DiasTrabajados { get; set; }
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
            string connectionString = Sconexion.ObtenerCadenaConexion();

            string fechaInicio = Variables.FechaInicio;
            string fechaFin = Variables.FechaFin;

            string usuario = Variables.Usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
            int? id_obra = Variables.IdObra ?? 0;

            // ================== ADMIN ===================
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string queryAdmin = @"
SELECT 
    ad.id_admins AS admins_id_asistencia,
    CONCAT(ad.admins_nombre, ' ', ad.admins_apellidop, ' ', ad.admins_apellidom) AS admin_nombre,
    ad.admin_categoria,
    ISNULL(SUM(a.salariofecha), 0) AS total_salario,
    ISNULL(SUM(a.paga_horaXT), 0) AS total_horas_extra,
    COUNT(DISTINCT a.asis_dia) AS dias_trabajados
FROM admins ad
LEFT JOIN asistencia a 
    ON ad.id_admins = a.admins_id_asistencia
    AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
WHERE ad.id_admins = @idAdmin
GROUP BY 
    ad.id_admins, 
    ad.admins_nombre, 
    ad.admins_apellidop, 
    ad.admins_apellidom, 
    ad.admin_categoria";

                SqlCommand cmd = new SqlCommand(queryAdmin, connection);
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empleados.Add(new Empleado
                        {
                            ID = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.GetString(2),
                            SueldoJornada = reader.GetDecimal(3) / 6, // No aplica
                            Dias = reader.GetInt32(5),
                            HorasExtra = ObtenerTotalHorasExtra_Admin(reader.GetInt32(0)), // No se desglosa como tiempo
                            PHoraExtra = reader.GetDecimal(4), // No se desglosa como tiempo
                            SueldoSemanal = reader.GetDecimal(3),
                            SuelExtra = reader.GetDecimal(4),
                            SuelTrabajado = (reader.GetDecimal(3) / 6) * reader.GetInt32(5),
                            SuelTotal = (reader.GetDecimal(3) / 6) * reader.GetInt32(5) + reader.GetDecimal(4)
                        });
                    }
                }
            }

            // ================== EMPLEADOS ===================
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string queryEmp = @"
        SELECT 
            e.id_empleado,
            CONCAT(e.emp_nombre, ' ', e.emp_apellidop, ' ', e.emp_apellidom) AS emp_nombre,
            e.emp_puesto,
            ISNULL(SUM(a.salariofecha), 0) AS total_salario,
            ISNULL(SUM(a.paga_horaXT), 0) AS total_horas_extra,
            COUNT(DISTINCT a.asis_dia) AS dias_trabajados
        FROM empleado e
        LEFT JOIN asistencia a 
            ON e.id_empleado = a.id_empleado 
            AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
        WHERE e.id_admins = @idAdmin
          AND e.id_obra = @id_obra
        GROUP BY e.id_empleado, e.emp_nombre, e.emp_apellidop, e.emp_apellidom, e.emp_puesto";

                SqlCommand cmd = new SqlCommand(queryEmp, connection);
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@id_obra", id_obra);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empleados.Add(new Empleado
                        {
                            ID = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.GetString(2),
                            SueldoJornada = reader.GetDecimal(3) / 6, // No aplica
                            Dias = reader.GetInt32(5),
                            HorasExtra = ObtenerTotalHorasExtra(reader.GetInt32(0)), // No se desglosa como tiempo
                            PHoraExtra =reader.GetDecimal(4), // No se desglosa como tiempo
                            SueldoSemanal = reader.GetDecimal(3),
                            SuelExtra = reader.GetDecimal(4),
                            SuelTrabajado = (reader.GetDecimal(3) /6)*reader.GetInt32(5),
                            SuelTotal = (reader.GetDecimal(3)/6) * reader.GetInt32(5) + reader.GetDecimal(4)
                        });
                    }
                }
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
    
