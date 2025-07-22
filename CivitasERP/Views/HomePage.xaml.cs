using CivitasERP.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly GraficosHomePage _svc;
        private readonly DispatcherTimer _timer;
        public HomePage()
        {
            InitializeComponent();
            _svc = new GraficosHomePage();

            // 1) Resumen
            var sum = _svc.GetSummary();
            txtTotalEmpleados.Text = sum.TotalEmpleados.ToString();
            txtNominaSemanal.Text = sum.NominaSemanal.ToString("C2");
            txtHorasExtra.Text = sum.HorasExtra.ToString("N2");
            txtAusencias.Text = sum.Ausencias.ToString();

            // 2) Gráficas
            piePlot.Model = _svc.GetPieChartModel();
            linePlot.Model = _svc.GetLineChartModel();

            // 3) Actualizar fecha y hora
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateFechaHora();
            _timer.Start();
        }
        private void UpdateFechaHora()
        {
            var now = DateTime.Now;
            var culture = new CultureInfo("es-ES");

            if (string.IsNullOrEmpty(Variables.FechaHomePage))
            {
                // Generar la fecha solo una vez y guardarla
                string fecha = now.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);
                Variables.FechaHomePage = char.ToUpper(fecha[0]) + fecha.Substring(1);
            }

            string hora = now.ToString("hh:mm tt");
            txtFechaHora.Text = $"{Variables.FechaHomePage} - {hora}";
        }
    }
}
