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

        public ListaPage()
        {
            InitializeComponent();
            cargar_admin();
            //Ocultar botones de navegación según el tipo de usuario
            if (Variables.Jefe == false)
            {
                AdminLabel.Visibility = Visibility.Collapsed;
                AdminComboBox.Visibility = Visibility.Collapsed;
                AdminBorder.Visibility = Visibility.Collapsed;
            }

            //InicializarPagina();
            EstablecerRangoFechaActual();
            if (Variables.ObraNom != null)
            {
                CargarObras();
                ObraComboBox.SelectedItem = Variables.ObraNom;
                ActualizarEmpleadosGrid();
            }
            if (Variables.AdminSeleccionado != null) {
                cargar_admin();
                AdminComboBox.SelectedItem= Variables.AdminSeleccionado;
                    ActualizarEmpleadosGrid();
            }
            if (Variables.indexComboboxAño != null || Variables.indexComboboxMes != null)
            {
                if (Variables.ObraNom != null)
                {
                    CargarObras();
                    ObraComboBox.SelectedItem = Variables.ObraNom;
                    ActualizarEmpleadosGrid();
                }

                if (Variables.indexComboboxAño != null)
                {
                    CargarAnios();
                    ComBoxAnio.SelectedItem = Variables.indexComboboxAño;
                }

                if (Variables.indexComboboxMes != null)
                {
                    CargarMeses();
                    ComBoxMes.SelectedItem = Variables.indexComboboxMes;

                    Agregar_tiempo tiempo = new Agregar_tiempo();

                    // Limpiar semanas antes de actualizar
                    ComBoxSemana.ItemsSource = null;
                    ComBoxSemana.SelectedItem = null;

                    if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
                    {
                        int mes = ComBoxMes.SelectedIndex + 1;
                        ComBoxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
                    }
                }

                if (Variables.indexComboboxSemana != null)
                {
                    ComBoxSemana.SelectedItem = Variables.indexComboboxSemana;
                }


            }
        }
        private void InicializarPagina()
        {


            CargarMeses();
            CargarAnios();
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = new Agregar_tiempo().GetSemanasDelMes(anio, mes);
            }
            string usuario = Variables.Usuario;
            int? idAdmin = new DB_admins().ObtenerIdPorUsuario(usuario);

            if (Variables.ObraNom != null)
            {
                CargarObras();
                ObraComboBox.SelectedItem = Variables.ObraNom;
            }

            if (Variables.indexComboboxAño != null || Variables.indexComboboxMes != null)
            {

                if (Variables.indexComboboxAño != null)
                    ComBoxAnio.SelectedItem = Variables.indexComboboxAño;

                if (Variables.indexComboboxMes != null)
                {
                    ComBoxMes.SelectedItem = Variables.indexComboboxMes;
                    ActualizarSemanas();
                }

                if (Variables.indexComboboxSemana != null)
                    ComBoxSemana.SelectedItem = Variables.indexComboboxSemana;
            }

            if (Variables.FechaInicio == null || Variables.FechaFin == null)
                EstablecerRangoFechaActual();

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

        private void ActualizarEmpleadosGrid()
        {
            /*repo = new Datagrid_lista(ObtenerConnectionString());
            dataGridAsistencia.ItemsSource = repo.ObtenerEmpleados();
            TotalPersonal.Text = dataGridAsistencia.Items.Count.ToString();*/

            DB_admins dB_Admins = new DB_admins();
            if (AdminComboBox.SelectedItem != null)
            {

                repo = new Datagrid_lista(ObtenerConnectionString());
                dataGridAsistencia.ItemsSource = repo.ObtenerEmpleados(dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString()));
                TotalPersonal.Text = dataGridAsistencia.Items.Count.ToString();
            }
            else
            {

                repo = new Datagrid_lista(ObtenerConnectionString());
                dataGridAsistencia.ItemsSource = repo.ObtenerEmpleados(Variables.IdAdmin);
                TotalPersonal.Text = dataGridAsistencia.Items.Count.ToString();
            }
        }

        private string ObtenerConnectionString() =>
            new Conexion().ObtenerCadenaConexion();

        private void CargarObras()
        {

            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            int? idAdminObra;

            if (AdminComboBox.SelectedItem != null)
            {
                idAdminObra = dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());
            }
            else
            {
                idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);
            }
            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            ObraComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                ObraComboBox.Items.Add(reader["obra_nombre"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }

        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string usuario = Variables.Usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin;
            if (AdminComboBox.SelectedItem != null)
            {
                idAdmin = dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());

                if (ObraComboBox.SelectedItem is string nombreObra)
                {
                    Variables.ObraNom = nombreObra;

                    idAdmin = new DB_admins().ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());
                    int? idObra = ObtenerID_obra(idAdmin, nombreObra);
                    Variables.IdObra = idObra;

                    UbicacionLabel.Text = ObtenerUbicacionObra(idObra);
                    ActualizarEmpleadosGrid();

                }
            }
            else
            {
                idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
                if (ObraComboBox.SelectedItem is string nombreObra)
                {
                    Variables.ObraNom = nombreObra;

                    idAdmin = new DB_admins().ObtenerIdPorUsuario(Variables.Usuario);
                    int? idObra = ObtenerID_obra(idAdmin, nombreObra);
                    Variables.IdObra = idObra;

                    UbicacionLabel.Text = ObtenerUbicacionObra(idObra);
                    ActualizarEmpleadosGrid();

                }
            }
            
           

        }

        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre)) return null;

            using var conn = new SqlConnection(ObtenerConnectionString());
            conn.Open();
            var cmd = new SqlCommand("SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre", conn);
            cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
            cmd.Parameters.AddWithValue("@obraNombre", obraNombre);

            object resultado = cmd.ExecuteScalar();
            return resultado != null && int.TryParse(resultado.ToString(), out int idObra) ? idObra : null;
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

        public string ObtenerUbicacionObra(int? idObra)
        {
            if (idObra == null) return string.Empty;

            string query = "SELECT obra_ubicacion FROM obra WHERE id_obra = @IdObra";

            using var conn = new SqlConnection(ObtenerConnectionString());
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IdObra", idObra);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader["obra_ubicacion"].ToString() : string.Empty;
        }

        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            int? idAdmin = new DB_admins().ObtenerIdPorUsuario(Variables.Usuario);
            CargarObras();
        }

        private void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DB_admins dB_Admins = new DB_admins();
            string nombreadmin = "";
            AdminComboBox.SelectedItem = nombreadmin;
            Variables.IdAdmin = dB_Admins.ObtenerIdPorUsuario(nombreadmin);
            Variables.AdminSeleccionado = AdminComboBox.SelectedItem.ToString();
        }
        private void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {

        }
        

        private void cargar_admin()
        {

            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            Variables.IdAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
            int? idAdminObra = Variables.IdAdmin;

            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    // Suponiendo que quieres obtener todos los admins, o filtrar de alguna forma
                    string query = @"SELECT
                            admins_usuario AS admin_nombre
                     FROM admins;";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            AdminComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                AdminComboBox.Items.Add(reader["admin_nombre"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar administradores: " + ex.Message);
            }


        }
    }
}
