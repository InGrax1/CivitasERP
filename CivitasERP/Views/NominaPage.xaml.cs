using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using GlobalCalendar = System.Globalization.Calendar; // Alias para evitar ambigüedad

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NominaPage.xaml
    /// </summary>
    public partial class NominaPage : Page
    {
        private DataGridNominas repo;

        public NominaPage()
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


            if (Variables.AdminSeleccionado != null)
            {
                cargar_admin();
                AdminComboBox.SelectedItem = Variables.AdminSeleccionado;
                CargarEmpleados();
            }


            if (Variables.ObraNom != null)
            {
                CargarDatosComboBox();
                ObraComboBox.SelectedItem = Variables.ObraNom;
                CargarEmpleados();
                CargarYSumar();
            }

            if (Variables.indexComboboxAño != null || Variables.indexComboboxMes != null)
            {
                if (Variables.ObraNom != null)
                {
                    CargarDatosComboBox();
                    ObraComboBox.SelectedItem = Variables.ObraNom;
                    CargarEmpleados();
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

        private string ObtenerConexion()
        {
            return new Conexion().ObtenerCadenaConexion();
        }

        private void CargarEmpleados()
        {
            DB_admins dB_Admins = new DB_admins();
            if (AdminComboBox.SelectedItem != null)
            {

                repo = new DataGridNominas(ObtenerConexion());
                var empleados = repo.ObtenerEmpleados(dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString()), AdminComboBox.SelectedItem.ToString());
                dataGridNomina.ItemsSource = empleados;
            }
            else {

                repo = new DataGridNominas(ObtenerConexion());
                var empleados = repo.ObtenerEmpleados(Variables.IdAdmin, Variables.Usuario);
                dataGridNomina.ItemsSource = empleados;
            }

        }

        private void btnNuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            NuevoEmpleadoPage nuevoEmpleadoPage = new NuevoEmpleadoPage();
            nuevoEmpleadoPage.Show();
        }

        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
        }

        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Variables.FechaFin == null || Variables.FechaInicio == null)
            {
                DateTime hoy = DateTime.Now;

                int diferenciaConLunes = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
                if (diferenciaConLunes < 0)
                    diferenciaConLunes += 7;

                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);

                string formato = "yyyy-MM-dd";

                Variables.FechaInicio = lunes.ToString(formato);
                Variables.FechaFin = domingo.ToString(formato);

                CultureInfo cultura = CultureInfo.InvariantCulture;
                GlobalCalendar calendario = cultura.Calendar;

                int numeroMes = hoy.Month;
                string año = hoy.Year.ToString();

                int numeroSemana = calendario.GetWeekOfYear(hoy, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                CargarAnios();
                CargarMeses();

                ComBoxSemana.SelectedIndex = numeroSemana;
                ComBoxMes.SelectedIndex = numeroMes - 1;
                ComBoxAnio.SelectedItem = año;
            }

            string usuario = Variables.Usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin;
            if (AdminComboBox.SelectedItem != null)
            {
                idAdmin = dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());

            }
            else
            {
                 idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);
            }
            if (ObraComboBox.SelectedItem != null)
            {
                string nombre_obra = ObraComboBox.SelectedItem.ToString();

                Variables.ObraNom = nombre_obra;

                int? id_obra = ObtenerID_obra(idAdmin, nombre_obra);
                Variables.IdObra = id_obra;

                UbicacionLabel.Text = ObtenerUbicacionObra(id_obra);

                CargarEmpleados();
            }


        }

        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre))
                return null;

            using (var conn = new SqlConnection(ObtenerConexion()))
            {
                string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                    cmd.Parameters.AddWithValue("@obraNombre", obraNombre);
                    conn.Open();

                    object resultado = cmd.ExecuteScalar();
                    if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                        return idObra;


                    return null;
                }

            }
        }

        private void CargarDatosComboBox()
        {
            
            string usuario = Variables.Usuario;
            var dB_Admins = new DB_admins();
            int? idAdminObra;

            if (AdminComboBox.SelectedItem!=null) {
                idAdminObra = dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString());
            }
            else
            {
                 idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);
            }

            try
            {
                using (var conn = new SqlConnection(ObtenerConexion()))
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

        private void ComBoxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Variables.Fecha = ComBoxSemana.SelectedItem?.ToString() ?? string.Empty;

            Agregar_tiempo tiempo = new Agregar_tiempo();

            var resultado = tiempo.ObtenerFechasDesdeGlobal();

            if (resultado.exito)
            {
                DateTime inicio = resultado.fechaInicio;
                DateTime fin = resultado.fechaFin;

                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");

                Variables.indexComboboxSemana = ComBoxSemana.SelectedItem.ToString();
            }

            CargarEmpleados();
        }

        private void ActualizarSemanas()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();

            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
        }

        private void ComBoxSemana_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }

        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();

            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
            ActualizarSemanas();

            Variables.indexComboboxMes = ComBoxMes.SelectedItem?.ToString();
        }

        private void CargarMeses()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            ComBoxMes.ItemsSource = tiempo.GetMeses();
            ComBoxMes.SelectedItem = DateTime.Now.Month;  // Corregí para que sea el mes actual (no año)
        }

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();

            if (ComBoxAnio.ItemsSource == null)
            {
                ComBoxAnio.ItemsSource = tiempo.GetAnios();
            }

            if (ComBoxAnio.SelectedItem == null || !(ComBoxAnio.SelectedItem is int))
            {
                int anioActual = DateTime.Now.Year;
                if (ComBoxAnio.Items.Contains(anioActual))
                {
                    ComBoxAnio.SelectedItem = Variables.indexComboboxAño;
                }
            }
            ActualizarSemanas();

            Variables.indexComboboxAño = ComBoxAnio.SelectedItem?.ToString();
        }

        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
        {
            CargarAnios();
        }

        private void CargarAnios()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            ComBoxAnio.ItemsSource = tiempo.GetAnios();
            ComBoxAnio.SelectedItem = DateTime.Now.Year;
        }

        private void CalcularTotales(IEnumerable<DataGridNominas.Empleado> lista)
        {
            int totalPersonal = lista.Count();
            decimal tJornada = lista.Sum(e => e.SueldoJornada);
            decimal tSemanal = lista.Sum(x => x.SueldoSemanal);
            decimal tHrsExtra = lista.Sum(x => x.HorasExtra);
            decimal tPrecioHE = lista.Sum(x => x.PHoraExtra);
            decimal tSuelExtra = lista.Sum(x => x.SuelExtra);
            decimal tTrabajado = lista.Sum(x => x.SuelTrabajado);
            decimal tGeneral = lista.Sum(x => x.SuelTotal);

            TotalPersonal.Text = totalPersonal.ToString();
            TotalSuelJornal.Text = tJornada.ToString("C2");
            TotalSuelSemanal.Text = tSemanal.ToString("C2");
            TotalHrsExtra.Text = tHrsExtra.ToString("N2");
            TotalPrecioHrsExt.Text = tPrecioHE.ToString("C2");
            TotalSuelExt.Text = tSuelExtra.ToString("C2");
            TotalSuelTrabajado.Text = tTrabajado.ToString("C2");
            TotalGeneral.Text = tGeneral.ToString("C2");
        }

        private void CargarYSumar()
        {


            DB_admins dB_Admins = new DB_admins();
            if (AdminComboBox.SelectedItem != null)
            {

                var empleados = repo.ObtenerEmpleados(dB_Admins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString()), AdminComboBox.SelectedItem.ToString());
                dataGridNomina.ItemsSource = empleados;
                CalcularTotales(empleados);

            }
            else
            {

                var empleados = repo.ObtenerEmpleados(Variables.IdAdmin, Variables.Usuario);
                dataGridNomina.ItemsSource = empleados;
                CalcularTotales(empleados);

            }
        }

        public string ObtenerUbicacionObra(int? idObra)
        {
            string ubicacion = null;

            using (SqlConnection connection = new SqlConnection(ObtenerConexion()))
            using (SqlCommand command = new SqlCommand("SELECT obra_ubicacion FROM obra WHERE id_obra = @IdObra", connection))
            {
                command.Parameters.AddWithValue("@IdObra", idObra);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ubicacion = reader["obra_ubicacion"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener la ubicación: " + ex.Message);
                }
            }

            return ubicacion;
        }

        private void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DB_admins dB_Admins = new DB_admins();  
            string nombreadmin="";
            AdminComboBox.SelectedItem = nombreadmin;
            Variables.IdAdmin= dB_Admins.ObtenerIdPorUsuario(nombreadmin);
            Variables.AdminSeleccionado=AdminComboBox.SelectedItem.ToString();
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