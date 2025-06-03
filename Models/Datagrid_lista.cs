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
                string query = @"SELECT id_empleado, CONCAT(empleado.emp_nombre, ' ', empleado.emp_apellidop, ' ', empleado.emp_apellidom) AS emp_nombre, emp_puesto,emp_dia, emp_semanal,emp_hora_extra  FROM empleado where id_admins=107;";


                SqlCommand cmd1 = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader1 = cmd1.ExecuteReader();

                Dictionary<int, Empleado_Asistencia> empleadosDict = new Dictionary<int, Empleado_Asistencia>();




                DateTime hoy = DateTime.Today;
                
                while (reader1.Read())
                {

                    empleados.Add(new Empleado_Asistencia
                    {
                        ID = reader1.GetInt32(0),
                        Nombre = reader1.GetString(1),
                        Categoria = reader1.GetString(2),



                        EntradaL = ObtenerHorarioDeEntrada(reader1.GetInt32(0),hoy),
                        SalidaL = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy),

                        EntradaM = ObtenerHorarioDeEntrada(reader1.GetInt32(0), hoy.AddDays(1)),
                        SalidaM = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy.AddDays(1)),

                        EntradaMI = ObtenerHorarioDeEntrada(reader1.GetInt32(0), hoy.AddDays(2)),
                        SalidaMI = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy.AddDays(2)),

                        EntradaJ = ObtenerHorarioDeEntrada(reader1.GetInt32(0), hoy.AddDays(3)),
                        SalidaJ = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy.AddDays(3)),

                        EntradaV = ObtenerHorarioDeEntrada(reader1.GetInt32(0), hoy.AddDays(4)),
                        SalidaV = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy.AddDays(4)),

                        EntradaS = ObtenerHorarioDeEntrada(reader1.GetInt32(0), hoy.AddDays(5)),
                        SalidaS = ObtenerHorarioDeSalida(reader1.GetInt32(0), hoy.AddDays(5)),
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
    }

}



