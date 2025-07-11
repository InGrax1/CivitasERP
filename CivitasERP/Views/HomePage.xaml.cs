using CivitasERP.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private string connectionString;
        public HomePage()
        {
            InitializeComponent();
            connectionString = new Conexion().ObtenerCadenaConexion();
            // Cargar datos al inicializar
            CargarEstadisticasGenerales();

            CargarGraficoObras();
        }
        private void CargarEstadisticasGenerales()
        {
            try
            {
                // Obtener fechas de la semana actual si no están establecidas
                EstablecerFechasSemanaActual();

                // Cargar todas las estadísticas
                int totalEmpleados = ObtenerTotalEmpleados();
                decimal nominaSemanal = ObtenerNominaSemanal();
                decimal horasExtra = ObtenerHorasExtra();
                int ausencias = ObtenerAusencias();

                // Actualizar los TextBlocks
                txtTotalEmpleados.Text = totalEmpleados.ToString();
                txtNominaSemanal.Text = nominaSemanal.ToString("C2");
                txtHorasExtra.Text = horasExtra.ToString("N2");
                txtAusencias.Text = ausencias.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}");
            }
        }
        private void EstablecerFechasSemanaActual()
        {
            if (string.IsNullOrEmpty(Variables.FechaInicio) || string.IsNullOrEmpty(Variables.FechaFin))
            {
                DateTime hoy = DateTime.Now;
                int diferenciaConLunes = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
                if (diferenciaConLunes < 0)
                    diferenciaConLunes += 7;

                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);

                Variables.FechaInicio = lunes.ToString("yyyy-MM-dd");
                Variables.FechaFin = domingo.ToString("yyyy-MM-dd");
            }
        }
        private int ObtenerTotalEmpleados()
        {
            int totalEmpleados = 0;
            int idAdmin = Variables.IdAdmin ?? 0;

            using (var conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*) AS TotalEmpleados
                    FROM empleado e
                    INNER JOIN obra o ON e.id_obra = o.id_obra
                    WHERE o.id_admin_obra = @idAdmin";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out totalEmpleados))
                    {
                        return totalEmpleados;
                    }
                }
            }
            return totalEmpleados;
        }

        private decimal ObtenerNominaSemanal()
        {
            decimal nominaTotal = 0;
            int idAdmin = Variables.IdAdmin ?? 0;
            string fechaInicio = Variables.FechaInicio;
            string fechaFin = Variables.FechaFin;

            using (var conn = new SqlConnection(connectionString))
            {
                // Nómina de empleados
                string queryEmpleados = @"
                    WITH DiasUnicos AS (
                        SELECT DISTINCT id_empleado, CAST(asis_dia AS DATE) AS Dia
                        FROM asistencia
                        WHERE id_empleado IN (
                            SELECT id_empleado
                            FROM empleado
                            WHERE id_admins = @idAdmin
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
                            SUM(paga_horaXT) AS paga_horaXT_total
                        FROM AsistenciaFiltrada
                        GROUP BY id_empleado
                    )
                    SELECT 
                        SUM(ISNULL(salario.salario_diario_fijo * extra.dias_laborados, 0) + ISNULL(extra.paga_horaXT_total, 0)) AS NominaTotal
                    FROM empleado e
                    LEFT JOIN SalarioBaseEmpleado salario ON e.id_empleado = salario.id_empleado
                    LEFT JOIN HorasYExtras extra ON e.id_empleado = extra.id_empleado
                    WHERE e.id_admins = @idAdmin";

                using (var cmd = new SqlCommand(queryEmpleados, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null && decimal.TryParse(result.ToString(), out decimal nominaEmpleados))
                    {
                        nominaTotal += nominaEmpleados;
                    }
                }

                // Nómina del admin
                string queryAdmin = @"
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
                            SUM(paga_horaXT) AS paga_horaXT_total
                        FROM AsistenciaFiltrada
                        GROUP BY admins_id_asistencia
                    )
                    SELECT 
                        SUM(ISNULL(salario.salario_diario_fijo * extra.dias_laborados, 0) + ISNULL(extra.paga_horaXT_total, 0)) AS NominaAdmin
                    FROM admins a
                    LEFT JOIN SalarioBaseAdmin salario ON a.id_admins = salario.id_admins
                    LEFT JOIN HorasYExtras extra ON a.id_admins = extra.id_admins
                    WHERE a.id_admins = @idAdmin";

                using (var cmd = new SqlCommand(queryAdmin, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                    object result = cmd.ExecuteScalar();
                    if (result != null && decimal.TryParse(result.ToString(), out decimal nominaAdmin))
                    {
                        nominaTotal += nominaAdmin;
                    }
                }
            }

            return nominaTotal;
        }

        private decimal ObtenerHorasExtra()
        {
            decimal horasExtraTotal = 0;
            int idAdmin = Variables.IdAdmin ?? 0;
            string fechaInicio = Variables.FechaInicio;
            string fechaFin = Variables.FechaFin;

            using (var conn = new SqlConnection(connectionString))
            {
                // Horas extra de empleados
                string queryEmpleados = @"
                    SELECT 
                        SUM(DATEDIFF(MINUTE, 0, asis_hora_extra)) / 60.0 AS HorasExtraTotal
                    FROM asistencia a
                    INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                    WHERE e.id_admins = @idAdmin
                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                      AND a.asis_hora_extra IS NOT NULL";

                using (var cmd = new SqlCommand(queryEmpleados, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null && decimal.TryParse(result.ToString(), out decimal horasEmpleados))
                    {
                        horasExtraTotal += horasEmpleados;
                    }
                }

                // Horas extra del admin
                string queryAdmin = @"
                    SELECT 
                        SUM(DATEDIFF(MINUTE, 0, asis_hora_extra)) / 60.0 AS HorasExtraAdmin
                    FROM asistencia a
                    WHERE a.admins_id_asistencia = @idAdmin
                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                      AND a.asis_hora_extra IS NOT NULL";

                using (var cmd = new SqlCommand(queryAdmin, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                    object result = cmd.ExecuteScalar();
                    if (result != null && decimal.TryParse(result.ToString(), out decimal horasAdmin))
                    {
                        horasExtraTotal += horasAdmin;
                    }
                }
            }

            return horasExtraTotal;
        }
        private int ObtenerAusencias()
        {
            int ausenciasTotal = 0;
            int idAdmin = Variables.IdAdmin ?? 0;
            string fechaInicio = Variables.FechaInicio;
            string fechaFin = Variables.FechaFin;

            using (var conn = new SqlConnection(connectionString))
            {
                // Calcular días laborables en el rango
                DateTime desde = DateTime.Parse(fechaInicio);
                DateTime hasta = DateTime.Parse(fechaFin);
                int diasLaborables = 0;

                for (DateTime fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
                {
                    if (fecha.DayOfWeek != DayOfWeek.Sunday) // Asumiendo que domingo no es laborable
                    {
                        diasLaborables++;
                    }
                }

                // Contar empleados activos
                string queryTotalEmpleados = @"
                    SELECT COUNT(*) AS TotalEmpleados
                    FROM empleado e
                    INNER JOIN obra o ON e.id_obra = o.id_obra
                    WHERE o.id_admin_obra = @idAdmin";

                int totalEmpleados = 0;
                using (var cmd = new SqlCommand(queryTotalEmpleados, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out totalEmpleados))
                    {
                        // Incluir al admin en el conteo
                        totalEmpleados += 1;
                    }
                }

                // Contar asistencias reales (empleados)
                string queryAsistenciasEmpleados = @"
                    SELECT COUNT(DISTINCT CONCAT(a.id_empleado, '-', CAST(a.asis_dia AS DATE))) AS AsistenciasEmpleados
                    FROM asistencia a
                    INNER JOIN empleado e ON a.id_empleado = e.id_empleado
                    WHERE e.id_admins = @idAdmin
                      AND a.asis_dia BETWEEN @fechaInicio AND @fechaFin
                      AND (a.asis_hora IS NOT NULL OR a.asis_salida IS NOT NULL)";

                int asistenciasEmpleados = 0;
                using (var cmd = new SqlCommand(queryAsistenciasEmpleados, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                    object result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out asistenciasEmpleados))
                    {
                        // Calcular asistencias del admin
                        string queryAsistenciasAdmin = @"
                            SELECT COUNT(DISTINCT CAST(asis_dia AS DATE)) AS AsistenciasAdmin
                            FROM asistencia
                            WHERE admins_id_asistencia = @idAdmin
                              AND asis_dia BETWEEN @fechaInicio AND @fechaFin
                              AND (asis_hora IS NOT NULL OR asis_salida IS NOT NULL)";

                        using (var cmdAdmin = new SqlCommand(queryAsistenciasAdmin, conn))
                        {
                            cmdAdmin.Parameters.AddWithValue("@idAdmin", idAdmin);
                            cmdAdmin.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                            cmdAdmin.Parameters.AddWithValue("@fechaFin", fechaFin);

                            object resultAdmin = cmdAdmin.ExecuteScalar();
                            if (resultAdmin != null && int.TryParse(resultAdmin.ToString(), out int asistenciasAdmin))
                            {
                                int totalAsistencias = asistenciasEmpleados + asistenciasAdmin;
                                int asistenciasEsperadas = totalEmpleados * diasLaborables;
                                ausenciasTotal = Math.Max(0, asistenciasEsperadas - totalAsistencias);
                            }
                        }
                    }
                }
            }

            return ausenciasTotal;
        }
        // Método público para actualizar las estadísticas (puede ser llamado desde otras partes)
        public void ActualizarEstadisticas()
        {
            CargarEstadisticasGenerales();
        }


        //OXYPLOT WPF graficos
        public class ObraCount
        {
            public string ObraName { get; set; }
            public int Count { get; set; }
        }
        private void CargarGraficoObras()
        {
            // 1) Recupera datos de BD: nombre de obra y cantidad de empleados
            var lista = new List<ObraCount>();
            using var conn = new SqlConnection(new Conexion().ObtenerCadenaConexion());
            conn.Open();
            using (var cmd = new SqlCommand(@"
        SELECT o.obra_nombre, COUNT(e.id_empleado) 
        FROM obra o
        LEFT JOIN empleado e ON e.id_obra = o.id_obra 
          AND e.id_admins = @idAdmin
        GROUP BY o.obra_nombre
        ORDER BY o.obra_nombre", conn))
            {
                cmd.Parameters.AddWithValue("@idAdmin", Variables.IdAdmin);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    lista.Add(new ObraCount
                    {
                        ObraName = rdr.GetString(0),
                        Count = rdr.GetInt32(1)
                    });
            }

            // 2) Crea el PlotModel
            var model = new PlotModel
            {
                Title = "Empleados por Obra",
                IsLegendVisible = true
            };

            // 3) Crea y añade la leyenda (dona abajo, horizontal)
            model.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0
            });

            // 4) Crea una PieSeries estilo dona
            var pie = new PieSeries
            {
                StrokeThickness = 1,
                InnerDiameter = 0.6,                   // convierte pie en dona
                OutsideLabelFormat = "",               // oculta etiquetas externas completamente
                InsideLabelFormat = "",                // oculta etiquetas internas
                TickHorizontalLength = 0,              // quita líneas hacia etiquetas
                TickRadialLength = 0,
                LegendFormat = "{1} ({2:0}%)"          // {1}=label, {2}=porcentaje
            };

            // 5) Calcula el total para los porcentajes
            var total = lista.Sum(x => x.Count);

            // 6) Agrega los slices (volvemos al formato original)
            foreach (var item in lista)
            {
                pie.Slices.Add(new PieSlice(item.ObraName, item.Count));
            }

            // Si no hay datos, agregar datos de prueba para verificar
            if (pie.Slices.Count == 0)
            {
                pie.Slices.Add(new PieSlice("Obra A", 40));
                pie.Slices.Add(new PieSlice("Obra B", 20));
                pie.Slices.Add(new PieSlice("Obra C", 15));
            }

            model.Series.Add(pie);

            // 7) Asigna al PlotView
            piePlot.Model = model;
        }
    }
}
