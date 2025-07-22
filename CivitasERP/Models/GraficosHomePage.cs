using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CivitasERP.Models
{
    internal class GraficosHomePage
    {
        private readonly string _connStr;
        private readonly Agregar_tiempo _tiempo;

        public GraficosHomePage()
        {
            _connStr = new Conexion().ObtenerCadenaConexion();
            _tiempo = new Agregar_tiempo();
        }

        public class Summary
        {
            public int TotalEmpleados { get; set; }
            public decimal NominaSemanal { get; set; }
            public decimal HorasExtra { get; set; }
            public int Ausencias { get; set; }
        }

        // Método modificado para manejar casos donde no hay selección específica
        public Summary GetSummary()
        {
            bool esJefe = Variables.Jefe;
            int? idObraSeleccionada = Variables.IdObra;
            int? idAdmin = Variables.IdAdmin;
            var (ini, fin) = _tiempo.GetSemanaActual();
            string fi = ini.ToString("yyyy-MM-dd");
            string ff = fin.ToString("yyyy-MM-dd");

            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var s = new Summary();

            // Determinar el alcance de la consulta
            bool mostrarTodo = (idAdmin == null || idAdmin == 0) && (idObraSeleccionada == null || idObraSeleccionada == 0);

            // Total empleados activos
            string queryTotalEmpleados;
            bool usaIdObra = false;
            bool usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    //Jefe, muestra empleados con obra
                    queryTotalEmpleados = @"
                                            SELECT COUNT(*) 
                                            FROM empleado e
                                            INNER JOIN obra o ON e.id_obra = o.id_obra";
                    usaIdObra = true;
                }
                else
                {
                    //Jefe, muestra todods los empleados
                    queryTotalEmpleados = @"
                                            SELECT COUNT(*) 
                                            FROM empleado e
                                            INNER JOIN obra o ON e.id_obra = o.id_obra";

                }
            }
            else
            {
                //admin muestra empleados de su obra
                queryTotalEmpleados = @"
                                        SELECT COUNT(*) 
                                        FROM empleado e
                                        INNER JOIN obra o ON e.id_obra = o.id_obra
                                        WHERE o.id_admin_obra = @idAdmin";
                usaIdAdmin = true;
            }

            using (var cmd = new SqlCommand(queryTotalEmpleados, conn))
            {
                if (usaIdObra)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                }
                if (usaIdAdmin)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);
                }

                var result = cmd.ExecuteScalar();
                s.TotalEmpleados = result != null ? Convert.ToInt32(result) : 0;
            }


            // Nómina semanal
            string queryNomina;
            // 1. Nómina Semanal
            usaIdObra = false;
            usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {

                    queryNomina = @"
                                    SELECT ISNULL(SUM(
                                        ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
                                    ), 0) AS total_nomina
                                    FROM asistencia a
                                    LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                    LEFT JOIN obra o ON e.id_obra = o.id_obra
                                    WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
                else
                {
                    queryNomina = @"
                                    SELECT ISNULL(SUM(
                                        ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
                                    ), 0) AS total_nomina
                                    FROM asistencia a
                                    LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                    LEFT JOIN obra o ON e.id_obra = o.id_obra
                                    WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
            }
            else
            {
                usaIdAdmin = true;
                queryNomina = @"
                                SELECT ISNULL(SUM(
                                    ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
                                ), 0) AS total_nomina
                                FROM asistencia a
                                LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                LEFT JOIN obra o ON e.id_obra = o.id_obra
                                WHERE (o.id_admin_obra = @idAdmin OR a.admins_id_asistencia = @idAdmin)
                                  AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
            }

            using (var cmd = new SqlCommand(queryNomina, conn))
            {
                cmd.Parameters.AddWithValue("@fechaInicio", fi);
                cmd.Parameters.AddWithValue("@fechaFin", ff);

                if (usaIdObra)
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                if (usaIdAdmin)
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);

                var result = cmd.ExecuteScalar();
                s.NominaSemanal = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }


            // Horas extra
            string queryHorasExtra;
            usaIdObra = false;
            usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    usaIdObra = true;
                    queryHorasExtra = @"
                                        SELECT ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
                                        FROM asistencia a
                                        LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                        LEFT JOIN obra o ON e.id_obra = o.id_obra
                                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                          AND a.asis_hora_extra IS NOT NULL
                                          AND a.asis_hora_extra != '00:00:00'";
                }
                else
                {
                    queryHorasExtra = @"
                                        SELECT ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
                                        FROM asistencia a
                                        LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                        LEFT JOIN obra o ON e.id_obra = o.id_obra
                                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                          AND a.asis_hora_extra IS NOT NULL
                                          AND a.asis_hora_extra != '00:00:00'";
                }
            }
            else
            {
                usaIdAdmin = true;
                queryHorasExtra = @"
                                    SELECT ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
                                    FROM asistencia a
                                    LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                                    LEFT JOIN obra o ON e.id_obra = o.id_obra
                                    WHERE (o.id_admin_obra = @idAdmin OR a.admins_id_asistencia = @idAdmin)
                                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                      AND a.asis_hora_extra IS NOT NULL
                                      AND a.asis_hora_extra != '00:00:00'";
            }

            using (var cmd = new SqlCommand(queryHorasExtra, conn))
            {
                cmd.Parameters.AddWithValue("@fechaInicio", fi);
                cmd.Parameters.AddWithValue("@fechaFin", ff);

                if (usaIdObra)
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                if (usaIdAdmin)
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);

                var result = cmd.ExecuteScalar();
                s.HorasExtra = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }


            // Ausencias
            string queryAusencias;
            usaIdObra = false;
            usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    usaIdObra = true;
                    queryAusencias = @"
                                        SELECT COUNT(*) 
                                        FROM empleado e
                                        inner JOIN obra o ON e.id_obra = o.id_obra
                                        WHERE NOT EXISTS (
                                            SELECT 1 FROM asistencia a 
                                            WHERE a.id_empleado = e.id_empleado 
                                              AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                              AND a.asis_hora IS NOT NULL 
                                              AND a.asis_hora != ''
                                          )";
                }
                else
                {
                    queryAusencias = @"
                                        SELECT COUNT(*) 
                                        FROM empleado e
                                        inner JOIN obra o ON e.id_obra = o.id_obra
                                        WHERE NOT EXISTS (
                                            SELECT 1 FROM asistencia a 
                                            WHERE a.id_empleado = e.id_empleado 
                                              AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                              AND a.asis_hora IS NOT NULL 
                                              AND a.asis_hora != ''
                                          )";
                }
            }
            else
            {
                usaIdAdmin = true;
                queryAusencias = @"
                                    SELECT COUNT(*) 
                                    FROM empleado e
                                    inner JOIN obra o ON e.id_obra = o.id_obra
                                    WHERE o.id_admin_obra = @idAdmin
                                      AND NOT EXISTS (
                                          SELECT 1 FROM asistencia a 
                                          WHERE a.id_empleado = e.id_empleado 
                                            AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                                            AND a.asis_hora IS NOT NULL 
                                            AND a.asis_hora != ''
                                      )";
            }

            using (var cmd = new SqlCommand(queryAusencias, conn))
            {
                cmd.Parameters.AddWithValue("@fechaInicio", fi);
                cmd.Parameters.AddWithValue("@fechaFin", ff);

                if (usaIdObra)
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                if (usaIdAdmin)
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);

                var result = cmd.ExecuteScalar();
                s.Ausencias = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }


            return s;
        }

        // Método modificado para el gráfico de dona
        public PlotModel GetPieChartModel()
        {
            bool esJefe = Variables.Jefe;
            int? idObraSeleccionada = Variables.IdObra;
            int? idAdmin = Variables.IdAdmin;
            var data = new List<(string Name, int Count)>();

            using var conn = new SqlConnection(_connStr);
            conn.Open();

            bool mostrarTodo = (idAdmin == null || idAdmin == 0) && (idObraSeleccionada == null || idObraSeleccionada == 0);

            string queryPie;
            bool usaIdObra = false;
            bool usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    usaIdObra = true;
                    queryPie = @"
                        SELECT o.obra_nombre, COUNT(e.id_empleado) as total_empleados
                        FROM obra o
                        LEFT JOIN empleado e ON e.id_obra = o.id_obra
                        GROUP BY o.obra_nombre, o.id_obra
                        ORDER BY total_empleados DESC";
                }
                else
                {
                    queryPie = @"
                        SELECT o.obra_nombre, COUNT(e.id_empleado) as total_empleados
                        FROM obra o
                        LEFT JOIN empleado e ON e.id_obra = o.id_obra
                        GROUP BY o.obra_nombre, o.id_obra
                        ORDER BY total_empleados DESC";
                }
            }
            else
            {
                usaIdAdmin = true;
                queryPie = @"
                    SELECT o.obra_nombre, COUNT(e.id_empleado) as total_empleados
                    FROM obra o
                    LEFT JOIN empleado e ON e.id_obra = o.id_obra
                    WHERE o.id_admin_obra = @idAdmin
                    GROUP BY o.obra_nombre, o.id_obra
                    ORDER BY total_empleados DESC";
            }

            using (var cmd = new SqlCommand(queryPie, conn))
            {
                if (usaIdObra)
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                if (usaIdAdmin)
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    data.Add((rdr.GetString(0), rdr.GetInt32(1)));
            }

            // Crear gráfico
            var model = new PlotModel { Title = "Empleados por Obra" };
            model.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0
            });

            var pie = new PieSeries
            {
                InnerDiameter = 0.6,
                OutsideLabelFormat = null,
                InsideLabelFormat = null,
                AngleSpan = 360,
                StartAngle = 0
            };

            foreach (var (n, c) in data)
                pie.Slices.Add(new PieSlice(n, c) { IsExploded = false });

            model.Series.Add(pie);
            return model;

        }

        // Método modificado para el gráfico lineal
        public PlotModel GetLineChartModel()
        {
            int? idObraSeleccionada = Variables.IdObra;
            bool esJefe = Variables.Jefe;
            int? idAdmin = Variables.IdAdmin;

            var modelo = new PlotModel { Title = "Nómina semanal (mes actual)" };

            modelo.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Semana",
                IntervalType = DateTimeIntervalType.Days,
                MajorStep = 7
            });
            modelo.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Total Nómina",
                Minimum = 0
            });

            var points = new List<DataPoint>();
            var hoy = DateTime.Today;
            var semanas = _tiempo.GetSemanasDelMes(hoy.Year, hoy.Month);

            using var conn = new SqlConnection(_connStr);
            conn.Open();

            bool mostrarTodo = (idAdmin == null || idAdmin == 0) && (idObraSeleccionada == null || idObraSeleccionada == 0);

            string queryLineal;
            bool usaIdObra = false;
            bool usaIdAdmin = false;

            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    usaIdObra = true;
                    queryLineal = @"
            SELECT 
              ISNULL(SUM(
                ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
              ), 0) AS total_nomina
            FROM asistencia a
            left JOIN empleado e ON a.id_empleado = e.id_empleado
            left JOIN obra o ON e.id_obra = o.id_obra
            WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
                else
                {
                    queryLineal = @"
            SELECT 
              ISNULL(SUM(
                ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
              ), 0) AS total_nomina
            FROM asistencia a
            left JOIN empleado e ON a.id_empleado = e.id_empleado
            left JOIN obra o ON e.id_obra = o.id_obra
            WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
            }
            else
            {
                usaIdAdmin = true;
                queryLineal = @"
        SELECT 
          ISNULL(SUM(
            ISNULL(a.salariofecha, 0) + ISNULL(a.paga_horaXT, 0)
          ), 0) AS total_nomina
        FROM asistencia a
        LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
        LEFT JOIN obra o ON e.id_obra = o.id_obra
        WHERE (o.id_admin_obra = @idAdmin OR a.admins_id_asistencia = @idAdmin)
          AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
            }

            // Ejecutar consulta por semana
            foreach (var rango in semanas)
            {
                var p = rango.Split(" - ");
                var ini = DateTime.ParseExact(p[0], "dd/MM/yyyy", null);
                var fin = DateTime.ParseExact(p[1], "dd/MM/yyyy", null);

                using var cmd = new SqlCommand(queryLineal, conn);
                cmd.Parameters.AddWithValue("@fechaInicio", ini.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fechaFin", fin.ToString("yyyy-MM-dd"));

                if (usaIdObra)
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada.Value);
                if (usaIdAdmin)
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin.Value);

                var result = cmd.ExecuteScalar();
                var total = result != DBNull.Value ? Convert.ToDouble(result) : 0;

                points.Add(new DataPoint(DateTimeAxis.ToDouble(ini), total));
            }

            var line = new LineSeries { MarkerType = MarkerType.Circle, MarkerSize = 4 };
            line.Points.AddRange(points);
            modelo.Series.Add(line);

            return modelo;

        }
    }
}