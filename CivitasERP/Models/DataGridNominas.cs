using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using static CivitasERP.Models.DataGridNominas;
using CivitasERP.Converters;

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

            public List<DayAttendance> DiasTrabajadosDetalle { get; set; }

        }
        public class DayAttendance
        {
            public string DayName { get; set; }   // “Lun”, “Mar”, …
            public bool HasCheckIn { get; set; }
            public bool HasCheckOut { get; set; }
        }


        private readonly string connectionString;

        public DataGridNominas(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Empleado> ObtenerEmpleados(int? idAdminm, string usuariom)
        {
            

            var empleados = new List<Empleado>();
            var fechaInicio = Variables.FechaInicio;
            var fechaFin = Variables.FechaFin;
            var usuario = usuariom;
            var idAdmin = idAdminm.GetValueOrDefault();
            var idObra = Variables.IdObra ?? 0;
           
            empleados.AddRange(ObtenerDatosNomina(idAdmin, fechaInicio, fechaFin, true));
            empleados.AddRange(ObtenerDatosNomina(idAdmin, fechaInicio, fechaFin, false, idObra));

            return empleados;
        }


       
        private List<Empleado> ObtenerDatosNomina(int idAdmin, string fechaInicio, string fechaFin, bool esAdmin, int idObra = 0)
        {
            const decimal divisorDias = 6m;
            var empleados = new List<Empleado>();



            // 1) Traemos la lista base de empleados + sus totales
            string baseSql = esAdmin
              ? @"
WITH DiasUnicos AS (
    SELECT DISTINCT admins_id_asistencia AS id_admins, CAST(asis_dia AS DATE) AS Dia
    FROM asistencia
    WHERE admins_id_asistencia = @idAdmin
      AND asis_dia BETWEEN @fechaInicio AND @fechaFin
),
DiasLimitados AS (
    SELECT id_admins, Dia,
           ROW_NUMBER() OVER (PARTITION BY id_admins ORDER BY Dia ASC) AS rn
    FROM DiasUnicos
),
AsistenciaFiltrada AS (
    SELECT a.*
    FROM asistencia a
    INNER JOIN DiasLimitados d
        ON a.admins_id_asistencia = d.id_admins
       AND CAST(a.asis_dia AS DATE) = d.Dia
    WHERE d.rn <= 6
),
SalarioBaseAdmin AS (
    SELECT 
        admins_id_asistencia AS id_admins,
        MAX(salariofecha) AS salario_diario_fijo
    FROM AsistenciaFiltrada
    GROUP BY admins_id_asistencia
),
HorasYExtras AS (
    SELECT 
        admins_id_asistencia AS id_admins,
        COUNT(DISTINCT CAST(asis_dia AS DATE)) AS dias_laborados,
        SUM(paga_horaXT) AS paga_horaXT_total,  -- 🔁 suma total de paga_horaXT
        SUM(DATEDIFF(MINUTE, 0, asis_hora_extra)) / 60.0 AS horas_extra_total
    FROM AsistenciaFiltrada
    GROUP BY admins_id_asistencia
)
SELECT 
    a.id_admins AS OwnerId,
    CONCAT(ad.admins_nombre, ' ', ad.admins_apellidop, ' ', ad.admins_apellidom) AS Nombre,
    ad.admin_categoria AS Categoria,
    ISNULL(salario.salario_diario_fijo * 6, 0) AS SueldoSemanal,  -- fijo para 6 días
    ISNULL(extra.paga_horaXT_total, 0) AS PagaHoraExtra,  -- suma directa
    ISNULL(extra.dias_laborados, 0) AS Dias,
    ISNULL(extra.horas_extra_total, 0) AS HorasExtra
FROM admins a
INNER JOIN admins ad ON a.id_admins = ad.id_admins
LEFT JOIN SalarioBaseAdmin salario ON a.id_admins = salario.id_admins
LEFT JOIN HorasYExtras extra ON a.id_admins = extra.id_admins
WHERE a.id_admins = @idAdmin
GROUP BY 
    a.id_admins, ad.admins_nombre, ad.admins_apellidop, ad.admins_apellidom, ad.admin_categoria,
    salario.salario_diario_fijo, extra.paga_horaXT_total, extra.dias_laborados, extra.horas_extra_total;


                "





                      : @"
WITH DiasUnicos AS (
    SELECT DISTINCT id_empleado, CAST(asis_dia AS DATE) AS Dia
    FROM asistencia
    WHERE id_empleado IN (
        SELECT id_empleado
        FROM empleado
        WHERE id_admins = @idAdmin AND id_obra = @idObra
    )
    AND asis_dia BETWEEN @fechaInicio AND @fechaFin
),
DiasLimitados AS (
    SELECT id_empleado, Dia,
           ROW_NUMBER() OVER (PARTITION BY id_empleado ORDER BY Dia ASC) AS rn
    FROM DiasUnicos
),
AsistenciaFiltrada AS (
    SELECT a.*
    FROM asistencia a
    INNER JOIN DiasLimitados d
        ON a.id_empleado = d.id_empleado
       AND CAST(a.asis_dia AS DATE) = d.Dia
    WHERE d.rn <= 6
),
SalarioBaseEmpleado AS (
    SELECT 
        id_empleado,
        MAX(salariofecha) AS salario_diario_fijo
    FROM AsistenciaFiltrada
    GROUP BY id_empleado
),
HorasYExtras AS (
    SELECT 
        id_empleado,
        COUNT(DISTINCT CAST(asis_dia AS DATE)) AS dias_laborados,
        SUM(paga_horaXT) AS paga_horaXT_total,  -- 🔁 suma total de paga_horaXT
        SUM(DATEDIFF(MINUTE, 0, asis_hora_extra)) / 60.0 AS horas_extra_total
    FROM AsistenciaFiltrada
    GROUP BY id_empleado
)
SELECT 
    e.id_empleado AS OwnerId,
    CONCAT(e.emp_nombre, ' ', e.emp_apellidop, ' ', e.emp_apellidom) AS Nombre,
    e.emp_puesto AS Categoria,
    ISNULL(salario.salario_diario_fijo * 6, 0) AS SueldoSemanal,
    ISNULL(extra.paga_horaXT_total, 0) AS PagaHoraExtra,  -- suma directa
    ISNULL(extra.dias_laborados, 0) AS Dias,
    ISNULL(extra.horas_extra_total, 0) AS HorasExtra
FROM empleado e
LEFT JOIN SalarioBaseEmpleado salario ON e.id_empleado = salario.id_empleado
LEFT JOIN HorasYExtras extra ON e.id_empleado = extra.id_empleado
WHERE e.id_admins = @idAdmin AND e.id_obra = @idObra
GROUP BY 
    e.id_empleado, e.emp_nombre, e.emp_apellidop, e.emp_apellidom, e.emp_puesto,
    salario.salario_diario_fijo, extra.paga_horaXT_total, extra.dias_laborados, extra.horas_extra_total;



                ";

            //IMPLEMENTACION GRAFICA DE ASISTENCIA EN NOMINAS EN LA COLUMNA DE ASISTENCIA DEL DATA GRID
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(baseSql, conn))
            {
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                if (!esAdmin) cmd.Parameters.AddWithValue("@idObra", idObra);

                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var sueldoSemanal = rdr.GetDecimal(3);
                        var dias = rdr.GetInt32(5);
                        var sueldoJornada = sueldoSemanal / divisorDias;
                        var horasExtra = rdr.GetDecimal(6);
                        var pagaHoraExtra = rdr.GetDecimal(4) ;
                        var sueldoTrabajado = sueldoJornada * dias;
                        var sueldoTotal = sueldoTrabajado + pagaHoraExtra;

                        empleados.Add(new Empleado
                        {
                            ID = rdr.GetInt32(0),
                            Nombre = rdr.GetString(1),
                            Categoria = rdr.GetString(2),
                            Dias = dias,
                            SueldoJornada = sueldoJornada,
                            SueldoSemanal = sueldoSemanal,
                            HorasExtra = horasExtra,
                            PHoraExtra = pagaHoraExtra,
                            SuelExtra = pagaHoraExtra *horasExtra,
                            SuelTrabajado = sueldoTrabajado,
                            SuelTotal = sueldoTotal,
                            DiasTrabajadosDetalle = new List<DayAttendance>()
                        });
                    }
                }
            }

            // 2) Traemos **una sola** vez** todas las asistencias del rango y todos los OwnerId’s**:
            //    (OwnerId = id_admins para admins, o id_empleado para trabajadores)
            var attendance = new Dictionary<(int OwnerId, DateTime Dia), (bool HasIn, bool HasOut)>();

            string attSql = esAdmin
                ? @"
                  SELECT 
                    a.admins_id_asistencia    AS OwnerId,
                    CAST(a.asis_dia AS date)  AS Dia,
                    MAX(CASE WHEN a.asis_hora      IS NOT NULL THEN 1 ELSE 0 END) AS HasIn,
                    MAX(CASE WHEN a.asis_salida    IS NOT NULL THEN 1 ELSE 0 END) AS HasOut
                  FROM asistencia a
                  WHERE a.admins_id_asistencia = @idAdmin
                    AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                  GROUP BY a.admins_id_asistencia, CAST(a.asis_dia AS date);
                  "
                            : @"
                  SELECT 
                    a.id_empleado            AS OwnerId,
                    CAST(a.asis_dia AS date) AS Dia,
                    MAX(CASE WHEN a.asis_hora      IS NOT NULL THEN 1 ELSE 0 END) AS HasIn,
                    MAX(CASE WHEN a.asis_salida    IS NOT NULL THEN 1 ELSE 0 END) AS HasOut
                  FROM asistencia a
                  INNER JOIN empleado e 
                    ON e.id_empleado = a.id_empleado
                  WHERE e.id_admins = @idAdmin
                    AND e.id_obra   = @idObra
                    AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                  GROUP BY a.id_empleado, CAST(a.asis_dia AS date);
                  ";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(attSql, conn))
            {
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                if (!esAdmin) cmd.Parameters.AddWithValue("@idObra", idObra);

                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int owner = rdr.GetInt32(0);
                        DateTime dia = rdr.GetDateTime(1);
                        bool hasIn = rdr.GetInt32(2) == 1;
                        bool hasOut = rdr.GetInt32(3) == 1;
                        attendance[(owner, dia)] = (hasIn, hasOut);
                    }
                }
            }

            // 3) Ahora, para cada empleado, rellenamos su lista de DayAttendance
            var desde = DateTime.Parse(fechaInicio);
            var hasta = DateTime.Parse(fechaFin);
            foreach (var emp in empleados)
            {
                var detalle = new List<DayAttendance>();
                for (var d = desde; d <= hasta; d = d.AddDays(1))
                {
                    var key = (emp.ID, d.Date);
                    if (attendance.TryGetValue(key, out var inf))
                    {
                        detalle.Add(new DayAttendance
                        {
                            DayName = d.ToString("ddd"),
                            HasCheckIn = inf.HasIn,
                            HasCheckOut = inf.HasOut
                        });
                    }
                    else
                    {
                        detalle.Add(new DayAttendance
                        {
                            DayName = d.ToString("ddd"),
                            HasCheckIn = false,
                            HasCheckOut = false
                        });
                    }
                }
                emp.DiasTrabajadosDetalle = detalle;
            }

            return empleados;
        }


        /// <summary>
        /// Elimina el empleado con el ID dado de la base de datos.
        /// </summary>
        public async Task EliminarEmpleadoAsync(int idEmpleado)
        {
            using (var cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync();
                using (var tx = cn.BeginTransaction())
                {
                    // 1) Borra las asistencias asociadas
                    using (var cmdAsis = new SqlCommand(
                               "DELETE FROM asistencia WHERE id_empleado = @idEmpleado",
                               cn, tx))
                    {
                        cmdAsis.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                        await cmdAsis.ExecuteNonQueryAsync();
                    }

                    // 2) Borra el empleado
                    using (var cmdEmp = new SqlCommand(
                               "DELETE FROM empleado WHERE id_empleado = @idEmpleado",
                               cn, tx))
                    {
                        cmdEmp.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                        int filas = await cmdEmp.ExecuteNonQueryAsync();
                        if (filas == 0)
                            throw new InvalidOperationException($"No existe empleado con ID={idEmpleado}");
                    }

                    tx.Commit();
                }
            }
        }

    }
}