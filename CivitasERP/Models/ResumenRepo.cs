using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitasERP.Models
{
    public class ResumenRepo
    {
        private readonly string _connStr = ConfigurationManager
            .ConnectionStrings["TuCadena"].ConnectionString;

        public int GetTotalEmpleadosActivos()
        {
            const string sql = "SELECT COUNT(*) FROM Empleados WHERE Activo = 1";
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public decimal GetNominaSemanal(DateTime inicio, DateTime fin)
        {
            const string sql = @"
          SELECT ISNULL(SUM(Importe),0)
          FROM Nomina
          WHERE Fecha BETWEEN @ini AND @fin";
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ini", inicio);
            cmd.Parameters.AddWithValue("@fin", fin);
            conn.Open();
            return (decimal)cmd.ExecuteScalar();
        }

        public int GetHorasExtraAcum(DateTime inicio, DateTime fin)
        {
            const string sql = @"
          SELECT ISNULL(SUM(HorasExtra),0)
          FROM Asistencia
          WHERE Fecha BETWEEN @ini AND @fin";
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ini", inicio);
            cmd.Parameters.AddWithValue("@fin", fin);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public int GetAusenciasRecientes(DateTime inicio, DateTime fin)
        {
            const string sql = @"
          SELECT COUNT(*)
          FROM Asistencia
          WHERE Fecha BETWEEN @ini AND @fin AND Ausencia = 1";
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ini", inicio);
            cmd.Parameters.AddWithValue("@fin", fin);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

}
