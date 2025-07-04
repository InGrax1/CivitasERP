using CivitasERP.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CivitasERP.Models.Datagrid_lista;
using System.Data.SqlClient;
using System;
using System.Linq;
using CivitasERP.ViewModels;

namespace CivitasERP.Views
{
    public partial class ListaPage : Page
    {
        private Datagrid_lista repo;
        private int? idAdminActual;

        public ListaPage()
        {
            InitializeComponent();

            idAdminActual = new DB_admins().ObtenerIdPorUsuario(Variables.Usuario);
            Variables.IdAdmin = idAdminActual;

            cargar_admin();

            if (!Variables.Jefe)
            {
                AdminLabel.Visibility = Visibility.Collapsed;
                AdminComboBox.Visibility = Visibility.Collapsed;
                AdminBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnHuellaLista.Visibility = Visibility.Collapsed;
            }

            EstablecerRangoFechaActual();

            if (Variables.AdminSeleccionado != null)
                AdminComboBox.SelectedItem = Variables.AdminSeleccionado;

            if (Variables.ObraNom != null)
                ObraComboBox.SelectedItem = Variables.ObraNom;

            CargarFiltrosDeFecha();
            ActualizarEmpleadosGrid();
        }

        private void EstablecerRangoFechaActual()
        {
            DateTime hoy = DateTime.Now;
            int diferenciaConLunes = ((int)hoy.DayOfWeek + 6) % 7;
            DateTime lunes = hoy.AddDays(-diferenciaConLunes);
            DateTime domingo = lunes.AddDays(6);

            Variables.FechaInicio = lunes.ToString("yyyy-MM-dd");
            Variables.FechaFin = domingo.ToString("yyyy-MM-dd");

            CultureInfo cultura = CultureInfo.InvariantCulture;
            int numeroMes = hoy.Month;
            int numeroSemana = cultura.Calendar.GetWeekOfYear(hoy, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            CargarAnios();
            CargarMeses();

            ComBoxAnio.SelectedItem = hoy.Year;
            ComBoxMes.SelectedIndex = numeroMes - 1;
            ComBoxSemana.SelectedIndex = numeroSemana;
        }

        private void CargarFiltrosDeFecha()
        {
            if (Variables.indexComboboxAño != null)
            {
                CargarAnios();
                ComBoxAnio.SelectedItem = Variables.indexComboboxAño;
            }

            if (Variables.indexComboboxMes != null)
            {
                CargarMeses();
                ComBoxMes.SelectedItem = Variables.indexComboboxMes;
                ActualizarSemanas();
            }

            if (Variables.indexComboboxSemana != null)
                ComBoxSemana.SelectedItem = Variables.indexComboboxSemana;
        }

        private void ActualizarEmpleadosGrid()
        {
            repo = new Datagrid_lista(ObtenerConnectionString());

            int? idAdmin = GetSelectedAdminId();
            dataGridAsistencia.ItemsSource = repo.ObtenerEmpleados(idAdmin);
            TotalPersonal.Text = dataGridAsistencia.Items.Count.ToString();
        }

        private int? GetSelectedAdminId()
        {
            if (AdminComboBox.SelectedItem != null)
                return new DB_admins().ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());
            return idAdminActual;
        }

        private string ObtenerConnectionString() =>
            new Conexion().ObtenerCadenaConexion();

        private void CargarObras()
        {
            int? idAdminObra = GetSelectedAdminId();

            try
            {
                using var conn = new SqlConnection(ObtenerConnectionString());
                string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);

                conn.Open();
                using var reader = cmd.ExecuteReader();
                ObraComboBox.Items.Clear();
                while (reader.Read())
                {
                    ObraComboBox.Items.Add(reader["obra_nombre"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar obras: " + ex.Message);
            }
        }

        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObraComboBox.SelectedItem is string nombreObra)
            {
                Variables.ObraNom = nombreObra;

                int? idObra = ObtenerID_obra(GetSelectedAdminId(), nombreObra);
                Variables.IdObra = idObra;

                UbicacionLabel.Text = ObtenerUbicacionObra(idObra);
                ActualizarEmpleadosGrid();
            }
        }

        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre)) return null;

            using var conn = new SqlConnection(ObtenerConnectionString());
            conn.Open();

            using var cmd = new SqlCommand("SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre", conn);
            cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
            cmd.Parameters.AddWithValue("@obraNombre", obraNombre);

            object resultado = cmd.ExecuteScalar();
            return resultado != null && int.TryParse(resultado.ToString(), out int idObra) ? idObra : null;
        }

        private string ObtenerUbicacionObra(int? idObra)
        {
            if (idObra == null) return string.Empty;

            using var conn = new SqlConnection(ObtenerConnectionString());
            conn.Open();

            using var cmd = new SqlCommand("SELECT obra_ubicacion FROM obra WHERE id_obra = @IdObra", conn);
            cmd.Parameters.AddWithValue("@IdObra", idObra);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader["obra_ubicacion"].ToString() : string.Empty;
        }

        private void ComBoxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComBoxSemana.SelectedItem == null) return;

            Variables.Fecha = ComBoxSemana.SelectedItem.ToString();

            var resultado = new Agregar_tiempo().ObtenerFechasDesdeGlobal();
            if (resultado.exito)
            {
                Variables.FechaInicio = resultado.fechaInicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = resultado.fechaFin.ToString("yyyy-MM-dd");
                Variables.indexComboboxSemana = ComBoxSemana.SelectedItem.ToString();
            }

            ActualizarEmpleadosGrid();
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarSemanas();
            Variables.indexComboboxMes = ComBoxMes.SelectedItem?.ToString();
        }

        private void ActualizarSemanas()
        {
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = new Agregar_tiempo().GetSemanasDelMes(anio, mes);
            }
        }

        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
            => CargarMeses();

        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
            => CargarAnios();

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComBoxAnio.SelectedItem != null)
            {
                Variables.indexComboboxAño = ComBoxAnio.SelectedItem.ToString();
                ActualizarSemanas();
            }
        }

        private void CargarMeses()
        {
            ComBoxMes.ItemsSource = new Agregar_tiempo().GetMeses();
        }

        private void CargarAnios()
        {
            ComBoxAnio.ItemsSource = new Agregar_tiempo().GetAnios();
        }

        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ListaViewModel vm)
                vm.ScanCommand.Execute(null);
        }

        private void cargar_admin()
        {
            if (AdminComboBox.Items.Count > 0) return;

            try
            {
                using var conn = new SqlConnection(new Conexion().ObtenerCadenaConexion());
                string query = "SELECT admins_usuario FROM admins";

                using var cmd = new SqlCommand(query, conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    AdminComboBox.Items.Add(reader["admins_usuario"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar administradores: " + ex.Message);
            }
        }

        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            CargarObras();
        }

        private void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdminComboBox.SelectedItem == null) return;

            string nombreadmin = AdminComboBox.SelectedItem.ToString();
            Variables.IdAdmin = new DB_admins().ObtenerIdPorUsuario(nombreadmin);
            Variables.AdminSeleccionado = nombreadmin;

            CargarObras();
            ActualizarEmpleadosGrid();
        }

        private void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // Si necesitas cargar admins dinámicamente, puedes hacerlo aquí.
        }
    }
}
