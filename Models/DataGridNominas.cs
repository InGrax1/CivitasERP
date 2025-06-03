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

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string usuario;
                usuario = GlobalVariables1.usuario;
                DB_admins dB_Admins = new DB_admins();
                int? idAdmin;
                idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

                string query = @"SELECT id_empleado, 
                            CONCAT(empleado.emp_nombre, ' ', empleado.emp_apellidop, ' ', empleado.emp_apellidom) AS emp_nombre, 
                            emp_puesto, 
                            emp_dia, 
                            emp_semanal, 
                            emp_hora_extra  
                     FROM empleado 
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

        public int CalcularDiasTrabajados(int idEmpleado)
        {
            var fechasUnicas = new HashSet<DateTime>();

            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
                SELECT CAST(asis_dia AS DATE) AS DiaAsistido
                FROM asistencia
                WHERE id_empleado = @id_empleado
            ";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;

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

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                string query = @"
                                SELECT asis_hora_extra
                                FROM asistencia
                                WHERE id_empleado = @id_empleado AND asis_hora_extra IS NOT NULL";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;

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
    }
}
    
