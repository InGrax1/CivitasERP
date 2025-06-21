using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CivitasERP.Models;

namespace CivitasERP.Models
{
    class DataGridNominas
    {
        public class Empleado
        {
            public int ID { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
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
            var empleados = new List<Empleado>();
            var fechaInicio = Variables.FechaInicio;
            var fechaFin = Variables.FechaFin;
            var usuario = Variables.Usuario;
            var idAdmin = new DB_admins().ObtenerIdPorUsuario(usuario) ?? 0;
            var idObra = Variables.IdObra ?? 0;

            empleados.AddRange(ObtenerDatosNomina(idAdmin, fechaInicio, fechaFin, true));
            empleados.AddRange(ObtenerDatosNomina(idAdmin, fechaInicio, fechaFin, false, idObra));

            return empleados;
        }

        private List<Empleado> ObtenerDatosNomina(int idAdmin, string fechaInicio, string fechaFin, bool esAdmin, int idObra = 0)
        {
            var empleados = new List<Empleado>();
            const decimal divisorDias = 6m;

            string query = esAdmin
                ? @"
                    SELECT 
                        ad.id_admins,
                        CONCAT(ad.admins_nombre, ' ', ad.admins_apellidop, ' ', ad.admins_apellidom) AS Nombre,
                        ad.admin_categoria,
                        ISNULL(SUM(a.salariofecha), 0) AS SueldoSemanal,
                        ISNULL(SUM(a.paga_horaXT), 0) AS PagaHoraExtra,
                        COUNT(DISTINCT a.asis_dia) AS Dias,
                        ISNULL(SUM(DATEDIFF(MINUTE, 0, a.asis_hora_extra)) / 60.0, 0) AS HorasExtra
                    FROM admins ad
                    LEFT JOIN asistencia a ON ad.id_admins = a.admins_id_asistencia 
                        AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                    WHERE ad.id_admins = @idAdmin
                    GROUP BY ad.id_admins, ad.admins_nombre, ad.admins_apellidop, ad.admins_apellidom, ad.admin_categoria"
                : @"
                    SELECT 
                        e.id_empleado,
                        CONCAT(e.emp_nombre, ' ', e.emp_apellidop, ' ', e.emp_apellidom) AS Nombre,
                        e.emp_puesto,
                        ISNULL(SUM(a.salariofecha), 0) AS SueldoSemanal,
                        ISNULL(SUM(a.paga_horaXT), 0) AS PagaHoraExtra,
                        COUNT(DISTINCT a.asis_dia) AS Dias,
                        ISNULL(SUM(DATEDIFF(MINUTE, 0, a.asis_hora_extra)) / 60.0, 0) AS HorasExtra
                    FROM empleado e
                    LEFT JOIN asistencia a ON e.id_empleado = a.id_empleado 
                        AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                    WHERE e.id_admins = @idAdmin AND e.id_obra = @idObra
                    GROUP BY e.id_empleado, e.emp_nombre, e.emp_apellidop, e.emp_apellidom, e.emp_puesto";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                if (!esAdmin)
                    cmd.Parameters.AddWithValue("@idObra", idObra);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal sueldoSemanal = reader.GetDecimal(3);
                        int dias = reader.GetInt32(5);
                        decimal sueldoJornada = sueldoSemanal / divisorDias;
                        decimal sueldoTrabajado = sueldoJornada * dias;
                        decimal horasExtra = reader.GetDecimal(6);
                        decimal pagaHoraExtra = reader.GetDecimal(4);
                        decimal sueldoTotal = sueldoTrabajado + pagaHoraExtra;

                        empleados.Add(new Empleado
                        {
                            ID = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.GetString(2),
                            Dias = dias,
                            SueldoJornada = sueldoJornada,
                            SueldoSemanal = sueldoSemanal,
                            HorasExtra = horasExtra,
                            PHoraExtra = pagaHoraExtra,
                            SuelExtra = pagaHoraExtra,
                            SuelTrabajado = sueldoTrabajado,
                            SuelTotal = sueldoTotal
                        });
                    }
                }
            }

            return empleados;
        }

        public (bool exito, DateTime fechaInicio, DateTime fechaFin) ObtenerFechasDesdeGlobal()
        {
            string texto = Variables.Fecha;
            if (string.IsNullOrWhiteSpace(texto)) return (false, DateTime.MinValue, DateTime.MinValue);

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
