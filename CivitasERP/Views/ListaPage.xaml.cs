using CivitasERP.Models;
using CivitasERP.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CivitasERP.Views
{
    public partial class ListaPage : Page
    {
        private readonly string _connectionString;
        private readonly Datagrid_lista _repo;
        private readonly int _idAdmin;

        public ListaPage()
        {
            InitializeComponent();
            this.DataContext = new ListaViewModel();
            _connectionString = new Conexion().ObtenerCadenaConexion();
            _repo = new Datagrid_lista(_connectionString);
            _idAdmin = new DB_admins()
                                   .ObtenerIdPorUsuario(Variables.Usuario) ?? 0;

            Loaded += ListaPage_Loaded;
            Loaded += async (_, __) => await CargarAsistenciaAsync();
        }

        private async void ListaPage_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosComboBox();
            ConfigurarCombos();
            await CargarAsistenciaAsync();
        }

        private async Task CargarAsistenciaAsync()
        {
            // 1) Asegura que haya rango de fechas
            if (string.IsNullOrEmpty(Variables.FechaInicio) ||
                string.IsNullOrEmpty(Variables.FechaFin))
            {
                var hoy = DateTime.Today;
                int diff = ((int)hoy.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                var lunes = hoy.AddDays(-diff);
                var domingo = lunes.AddDays(6);
                Variables.FechaInicio = lunes.ToString("yyyy-MM-dd");
                Variables.FechaFin = domingo.ToString("yyyy-MM-dd");
            }

            // 2) Actualiza obra seleccionada
            if (!string.IsNullOrWhiteSpace(Variables.ObraNom))
                Variables.IdObra = ObtenerID_obra(_idAdmin, Variables.ObraNom);

            // 3) Llama al repo async
            var inicio = DateTime.Parse(Variables.FechaInicio, CultureInfo.InvariantCulture);
            var fin = DateTime.Parse(Variables.FechaFin, CultureInfo.InvariantCulture);
            var empleados = await _repo
                .ObtenerEmpleadosAsync(_idAdmin, Variables.IdObra ?? 0, inicio, fin);

            dataGridAsistencia.ItemsSource = empleados;
            TotalPersonal.Text = empleados.Count.ToString();
        }

        private int? ObtenerID_obra(int idAdmin, string obraNombre)
        {
            const string sql = @"
                SELECT id_obra 
                  FROM obra 
                 WHERE id_admin_obra = @idAdmin 
                   AND obra_nombre   = @obra";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
            cmd.Parameters.AddWithValue("@obra", obraNombre);
            conn.Open();
            var r = cmd.ExecuteScalar();
            return r != null && int.TryParse(r.ToString(), out var i) ? i : (int?)null;
        }

        private void CargarDatosComboBox()
        {
            const string sql = @"
                SELECT obra_nombre
                  FROM obra
                 WHERE id_admin_obra = @idAdmin";
            var lista = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idAdmin", _idAdmin);
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                lista.Add(rdr.GetString(0));
            ObraComboBox.ItemsSource = lista;
        }

        private void ConfigurarCombos()
        {
            var tiempo = new Agregar_tiempo();
            ComBoxAnio.ItemsSource = tiempo.GetAnios();
            ComBoxMes.ItemsSource = tiempo.GetMeses();
            ComBoxAnio.SelectedItem = DateTime.Now.Year;
            ComBoxMes.SelectedIndex = DateTime.Now.Month - 1;
            ActualizarSemanas();

            if (int.TryParse(Variables.indexComboboxAño, out var a))
                ComBoxAnio.SelectedItem = a;
            if (int.TryParse(Variables.indexComboboxMes, out var m))
                ComBoxMes.SelectedIndex = m - 1;
            if (!string.IsNullOrEmpty(Variables.indexComboboxSemana))
                ComBoxSemana.SelectedItem = Variables.indexComboboxSemana;
        }

        private void ActualizarSemanas()
        {
            var tiempo = new Agregar_tiempo();
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
                ComBoxSemana.ItemsSource =
                  tiempo.GetSemanasDelMes(anio, ComBoxMes.SelectedIndex + 1);
            else
                ComBoxSemana.ItemsSource = null;
        }

        // ** Eventos de UI **
        private async void ObraComboBox_SelectionChanged(object s, SelectionChangedEventArgs e)
            => await CargarAsistenciaAsync();

        private async void ComBoxAnio_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            Variables.indexComboboxAño = ComBoxAnio.SelectedItem?.ToString();
            ActualizarSemanas();
            await CargarAsistenciaAsync();
        }

        private async void ComBoxMes_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            Variables.indexComboboxMes = ComBoxMes.SelectedItem?.ToString();
            ActualizarSemanas();
            await CargarAsistenciaAsync();
        }

        private async void ComBoxSemana_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            Variables.indexComboboxSemana = ComBoxSemana.SelectedItem?.ToString();
            var (ok, ini, fin) = new Agregar_tiempo().ObtenerFechasDesdeGlobal();
            if (ok)
            {
                Variables.FechaInicio = ini.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");
            }
            await CargarAsistenciaAsync();
        }

        private void ObraComboBox_DropDownOpened(object s, EventArgs e)
            => CargarDatosComboBox();

        private void ComBoxAnio_DropDownOpened(object s, EventArgs e)
            => ConfigurarCombos();

        private void ComBoxMes_DropDownOpened(object s, EventArgs e)
        {
            ConfigurarCombos();
            ActualizarSemanas();
        }

        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PRUEBA 2");
            if (DataContext is ListaViewModel vm)
                vm.ScanCommand.Execute(null);
        }

        public string ObtenerUbicacionObra(int? idObra)
        {
            if (idObra == null) return string.Empty;
            const string sql = "SELECT obra_ubicacion FROM obra WHERE id_obra = @idObra";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idObra", idObra);
            conn.Open();
            return cmd.ExecuteScalar()?.ToString() ?? string.Empty;
        }
    }
}
