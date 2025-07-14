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
        public Summary GetSummary(int? idAdmin = null, bool esJefe = false, int? idObraSeleccionada = null)
        {
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
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryTotalEmpleados = @"
                        SELECT COUNT(*) 
                        FROM empleado e
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo
                    queryTotalEmpleados = @"
                        SELECT COUNT(*) 
                        FROM empleado e
                        INNER JOIN obra o ON e.id_obra = o.id_obra";
                }
            }
            else
            {
                // Admin específico
                queryTotalEmpleados = @"
                    SELECT COUNT(*) 
                    FROM empleado e
                    INNER JOIN obra o ON e.id_obra = o.id_obra
                    WHERE o.id_admin_obra = @idAdmin";
            }

            using (var cmd = new SqlCommand(queryTotalEmpleados, conn))
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }
                s.TotalEmpleados = (int)cmd.ExecuteScalar();
            }

            // Nómina semanal
            string queryNomina;
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryNomina = @"
                        SELECT 
                          ISNULL(SUM(
                            CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                            CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
                          ), 0) AS total_nomina
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra
                          AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo - SUMA TOTAL
                    queryNomina = @"
                        SELECT 
                          ISNULL(SUM(
                            CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                            CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
                          ), 0) AS total_nomina
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
            }
            else
            {
                // Admin específico
                queryNomina = @"
                    SELECT 
                      ISNULL(SUM(
                        CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                        CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
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

                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }

                var result = cmd.ExecuteScalar();
                s.NominaSemanal = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }

            // Horas extra
            string queryHorasExtra;
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryHorasExtra = @"
                        SELECT 
                          ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra
                          AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                          AND a.asis_hora_extra IS NOT NULL
                          AND a.asis_hora_extra != '00:00:00'";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo - SUMA TOTAL
                    queryHorasExtra = @"
                        SELECT 
                          ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                          AND a.asis_hora_extra IS NOT NULL
                          AND a.asis_hora_extra != '00:00:00'";
                }
            }
            else
            {
                // Admin específico
                queryHorasExtra = @"
                    SELECT 
                      ISNULL(SUM(DATEDIFF(MINUTE, '00:00:00', a.asis_hora_extra))/60.0, 0)
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

                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }

                var result = cmd.ExecuteScalar();
                s.HorasExtra = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }

            // Ausencias
            string queryAusencias;
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryAusencias = @"
                        SELECT COUNT(*) 
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra
                          AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                          AND (a.asis_hora IS NULL OR a.asis_hora = '') 
                          AND (a.asis_salida IS NULL OR a.asis_salida = '')";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo - SUMA TOTAL
                    queryAusencias = @"
                        SELECT COUNT(*) 
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                          AND (a.asis_hora IS NULL OR a.asis_hora = '') 
                          AND (a.asis_salida IS NULL OR a.asis_salida = '')";
                }
            }
            else
            {
                // Admin específico
                queryAusencias = @"
                    SELECT COUNT(*) 
                    FROM asistencia a
                    LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                    LEFT JOIN obra o ON e.id_obra = o.id_obra
                    WHERE (o.id_admin_obra = @idAdmin OR a.admins_id_asistencia = @idAdmin)
                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                      AND (a.asis_hora IS NULL OR a.asis_hora = '') 
                      AND (a.asis_salida IS NULL OR a.asis_salida = '')";
            }

            using (var cmd = new SqlCommand(queryAusencias, conn))
            {
                cmd.Parameters.AddWithValue("@fechaInicio", fi);
                cmd.Parameters.AddWithValue("@fechaFin", ff);

                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }

                s.Ausencias = (int)cmd.ExecuteScalar();
            }

            return s;
        }

        // Método modificado para el gráfico de dona
        public PlotModel GetPieChartModel(int? idAdmin = null, bool esJefe = false, int? idObraSeleccionada = null)
        {
            var data = new List<(string Name, int Count)>();
            using var conn = new SqlConnection(_connStr);
            conn.Open();

            bool mostrarTodo = (idAdmin == null || idAdmin == 0) && (idObraSeleccionada == null || idObraSeleccionada == 0);

            string queryPie;
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryPie = @"
                        SELECT o.obra_nombre, COUNT(e.id_empleado) as total_empleados
                        FROM obra o
                        LEFT JOIN empleado e ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra
                        GROUP BY o.obra_nombre, o.id_obra
                        ORDER BY total_empleados DESC";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo - TODAS LAS OBRAS
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
                // Admin específico
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
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    data.Add((rdr.GetString(0), rdr.GetInt32(1)));
            }

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
        public PlotModel GetLineChartModel(int? idAdmin = null, bool esJefe = false, int? idObraSeleccionada = null)
        {
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
            if (esJefe || mostrarTodo)
            {
                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    // Jefe con obra específica seleccionada
                    queryLineal = @"
                        SELECT 
                          ISNULL(SUM(
                            CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                            CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
                          ), 0) AS total_nomina
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE o.id_obra = @idObra
                          AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
                else
                {
                    // Jefe sin selección específica o mostrar todo - SUMA TOTAL
                    queryLineal = @"
                        SELECT 
                          ISNULL(SUM(
                            CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                            CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
                          ), 0) AS total_nomina
                        FROM asistencia a
                        INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                        INNER JOIN obra o ON e.id_obra = o.id_obra
                        WHERE a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
                }
            }
            else
            {
                // Admin específico
                queryLineal = @"
                    SELECT 
                      ISNULL(SUM(
                        CASE WHEN a.salariofecha IS NOT NULL THEN a.salariofecha ELSE 0 END + 
                        CASE WHEN a.paga_horaXT IS NOT NULL THEN a.paga_horaXT ELSE 0 END
                      ), 0) AS total_nomina
                    FROM asistencia a
                    LEFT JOIN empleado e ON a.id_empleado = e.id_empleado
                    LEFT JOIN obra o ON e.id_obra = o.id_obra
                    WHERE (o.id_admin_obra = @idAdmin OR a.admins_id_asistencia = @idAdmin)
                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin";
            }

            foreach (var rango in semanas)
            {
                var p = rango.Split(" - ");
                var ini = DateTime.ParseExact(p[0], "dd/MM/yyyy", null);
                var fin = DateTime.ParseExact(p[1], "dd/MM/yyyy", null);

                using var cmd = new SqlCommand(queryLineal, conn);
                cmd.Parameters.AddWithValue("@fechaInicio", ini.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fechaFin", fin.ToString("yyyy-MM-dd"));

                if (idObraSeleccionada != null && idObraSeleccionada > 0)
                {
                    cmd.Parameters.AddWithValue("@idObra", idObraSeleccionada);
                }
                else if (!esJefe && !mostrarTodo && idAdmin != null)
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                }

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