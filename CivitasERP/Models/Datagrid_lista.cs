using CivitasERP.Views; // para Variables
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

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

        private readonly string _connectionString;

        public Datagrid_lista(string connectionString)
            => _connectionString = connectionString;

        /// <summary>
        /// Tu consulta PIVOT asíncrona
        /// </summary>
        public async Task<List<Empleado_Asistencia>> ObtenerEmpleadosAsync(
            int idAdmin, int idObra, DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<Empleado_Asistencia>();
            var sql = @"
SET DATEFIRST 1;
WITH Usuarios AS (
    SELECT id_admins  AS ID,
           CONCAT(admins_nombre,' ',admins_apellidop,' ',admins_apellidom) AS Nombre,
           admin_categoria AS Categoria
      FROM admins
     WHERE id_admins = @idAdmin
    UNION ALL
    SELECT id_empleado AS ID,
           CONCAT(emp_nombre,' ',emp_apellidop,' ',emp_apellidom) AS Nombre,
           emp_puesto     AS Categoria
      FROM empleado
     WHERE id_admins = @idAdmin
       AND id_obra   = @idObra
),
Asis AS (
    SELECT 
      COALESCE(a.admins_id_asistencia, a.id_empleado) AS ID,
      a.asis_dia,
      a.asis_hora,
      a.asis_salida
    FROM asistencia a
    WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin

)
SELECT
  u.ID,
  u.Nombre,
  u.Categoria,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=1 THEN A.asis_hora   END) AS EntradaL,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=1 THEN A.asis_salida END) AS SalidaL,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=2 THEN A.asis_hora   END) AS EntradaM,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=2 THEN A.asis_salida END) AS SalidaM,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=3 THEN A.asis_hora   END) AS EntradaMI,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=3 THEN A.asis_salida END) AS SalidaMI,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=4 THEN A.asis_hora   END) AS EntradaJ,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=4 THEN A.asis_salida END) AS SalidaJ,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=5 THEN A.asis_hora   END) AS EntradaV,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=5 THEN A.asis_salida END) AS SalidaV,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=6 THEN A.asis_hora   END) AS EntradaS,
  MAX(CASE WHEN DATEPART(WEEKDAY, A.asis_dia)=6 THEN A.asis_salida END) AS SalidaS
FROM Usuarios u
LEFT JOIN Asis A ON A.ID = u.ID
GROUP BY u.ID, u.Nombre, u.Categoria;
";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
            cmd.Parameters.AddWithValue("@idObra", idObra);
            cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

            await conn.OpenAsync().ConfigureAwait(false);
            using var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await rdr.ReadAsync().ConfigureAwait(false))
            {
                lista.Add(new Empleado_Asistencia
                {
                    ID = rdr.GetInt32(0),
                    Nombre = rdr.GetString(1),
                    Categoria = rdr.GetString(2),
                    EntradaL = rdr.IsDBNull(3) ? null : rdr.GetTimeSpan(3),
                    SalidaL = rdr.IsDBNull(4) ? null : rdr.GetTimeSpan(4),
                    EntradaM = rdr.IsDBNull(5) ? null : rdr.GetTimeSpan(5),
                    SalidaM = rdr.IsDBNull(6) ? null : rdr.GetTimeSpan(6),
                    EntradaMI = rdr.IsDBNull(7) ? null : rdr.GetTimeSpan(7),
                    SalidaMI = rdr.IsDBNull(8) ? null : rdr.GetTimeSpan(8),
                    EntradaJ = rdr.IsDBNull(9) ? null : rdr.GetTimeSpan(9),
                    SalidaJ = rdr.IsDBNull(10) ? null : rdr.GetTimeSpan(10),
                    EntradaV = rdr.IsDBNull(11) ? null : rdr.GetTimeSpan(11),
                    SalidaV = rdr.IsDBNull(12) ? null : rdr.GetTimeSpan(12),
                    EntradaS = rdr.IsDBNull(13) ? null : rdr.GetTimeSpan(13),
                    SalidaS = rdr.IsDBNull(14) ? null : rdr.GetTimeSpan(14),
                });
            }

            return lista;
        }

        /// <summary>
        /// Síncrono, para tu ViewModel
        /// </summary>
        public List<Empleado_Asistencia> ObtenerEmpleados()
        {
            var idAdmin = new DB_admins().ObtenerIdPorUsuario(Variables.Usuario) ?? 0;
            var idObra = Variables.IdObra ?? 0;
            var inicio = DateTime.Parse(Variables.FechaInicio);
            var fin = DateTime.Parse(Variables.FechaFin);

            return ObtenerEmpleadosAsync(idAdmin, idObra, inicio, fin)
                   .GetAwaiter().GetResult();
        }

        /// <summary>
        /// Síncrono para marcar entrada/salida
        /// </summary>
        public void MarcarAsistencia(int id, bool esAdmin = false)
        {
            var hoy = DateTime.Today;
            var ahora = DateTime.Now.TimeOfDay;
            var col = esAdmin ? "admins_id_asistencia" : "id_empleado";
            var other = esAdmin ? "id_empleado" : "admins_id_asistencia";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // 1) ¿Ya hay registro hoy?
            var sel = $@"
        SELECT id_asistencia, asis_salida
          FROM asistencia
         WHERE {col} = @id
           AND CAST(asis_dia AS DATE)=@hoy";
            using (var cmd = new SqlCommand(sel, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@hoy", hoy);
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    var idAsis = rdr.GetInt32(0);
                    var tieneSalida = !rdr.IsDBNull(1);
                    rdr.Close();
                    if (tieneSalida)
                    {
                        MessageBox.Show(
                          "Ya registraste tu salida para hoy.",
                          "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    // sólo actualiza salida
                    using var upd = new SqlCommand(
                      "UPDATE asistencia SET asis_salida=@ahora WHERE id_asistencia=@idAsis", conn);
                    upd.Parameters.AddWithValue("@ahora", ahora);
                    upd.Parameters.AddWithValue("@idAsis", idAsis);
                    upd.ExecuteNonQuery();
                    return;
                }
            }

            // 2) no había registro → insert completo
            var ins = $@"
INSERT INTO asistencia 
  ({col}, asis_dia, asis_hora, asis_salida, asis_hora_extra, {other})
VALUES
  (@id, @hoy, @ahora, NULL, '00:00:00', NULL)";
            using var cmdIns = new SqlCommand(ins, conn);
            cmdIns.Parameters.AddWithValue("@id", id);
            cmdIns.Parameters.AddWithValue("@hoy", hoy);
            cmdIns.Parameters.AddWithValue("@ahora", ahora);
            cmdIns.ExecuteNonQuery();
        }

        /// <summary>
        /// Para tu ViewModel
        /// </summary>
        public void MarcarAsistenciaAdmin(int idAdmin)
            => MarcarAsistencia(idAdmin, true);
    }
}
