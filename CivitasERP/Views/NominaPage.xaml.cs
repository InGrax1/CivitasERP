using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Win32;
using static CivitasERP.Views.LoginPage;
using System.Data.SqlClient;
using System.Globalization;
using static CivitasERP.Views.ForgotPasswordPage;



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
            if (Variables.ObraNom != null)
            {
                CargarDatosComboBox();
                ObraComboBox.SelectedItem = Variables.ObraNom;
                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;

                repo = new DataGridNominas(connectionString);

                var empleados = repo.ObtenerEmpleados();
                dataGridNomina.ItemsSource = empleados;
            }


            if (Variables.indexComboboxAño != null || Variables.indexComboboxMes != null || Variables.indexComboboxMes != null)
            {
                if (Variables.ObraNom != null)
                {
                    CargarDatosComboBox();
                    ObraComboBox.SelectedItem = Variables.ObraNom;
                    Conexion Sconexion = new Conexion();
                    string connectionString;

                    string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                    connectionString = obtenerCadenaConexion;
                    repo = new DataGridNominas(connectionString);

                    var empleados = repo.ObtenerEmpleados();
                    dataGridNomina.ItemsSource = empleados;
                }
                if (Variables.indexComboboxAño != null || Variables.indexComboboxMes != null || Variables.indexComboboxMes != null)
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




                        Agregar_tiempo tiempo = new Agregar_tiempo();
                        // Limpiar semanas cada vez que cambie año o mes
                        ComBoxSemana.ItemsSource = null;
                        ComBoxSemana.SelectedItem = null;

                        // Verificar que tanto el año como el mes sean válidos
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




        // OBRA
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



                // Obtener cultura invariable con reglas ISO 8601
                CultureInfo cultura = CultureInfo.InvariantCulture;
                System.Globalization.Calendar calendario = cultura.Calendar;

                // Calcular número de semana según regla ISO 8601

                // Obtener nombre del mes y número
                int numeroMes = hoy.Month;

                // Obtener año
                string año = hoy.Year.ToString();


                int numeroSemana = calendario.GetWeekOfYear(hoy, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                CargarAnios();
                CargarMeses();

                ComBoxSemana.SelectedIndex = numeroSemana;
                ComBoxMes.SelectedIndex = --numeroMes;
                ComBoxAnio.SelectedItem = año;

            }

            string usuario = Variables.Usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

            if (ObraComboBox.SelectedItem != null)
            {

                string nombre_obra = ObraComboBox.SelectedItem.ToString();

                Variables.ObraNom = nombre_obra;

                int? id_obra = ObtenerID_obra(idAdmin, nombre_obra);
                Variables.IdObra = id_obra;



                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;
                repo = new DataGridNominas(connectionString);

                var empleados = repo.ObtenerEmpleados();
                dataGridNomina.ItemsSource = empleados;

                //this.Loaded += HomePage_Loaded;

            }
            else
            {
            }
        }
        private int? ObtenerID_obra(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre))
                return null;

            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
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
            int? idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);

            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

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








        //    SEMANA

        private void ComBoxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Variables.Fecha = ComBoxSemana.SelectedItem?.ToString() ?? string.Empty;

            Agregar_tiempo tiempo = new Agregar_tiempo();

            var resultado = tiempo.ObtenerFechasDesdeGlobal();

            if (resultado.exito)
            {
                // Ya puedes usar fechaInicio y fechaFin como DateTime
                DateTime inicio = resultado.fechaInicio;
                DateTime fin = resultado.fechaFin;

                // Opcional: convertir a formato SQL
                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");


                Variables.indexComboboxSemana = ComBoxSemana.SelectedItem.ToString();
            }



            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            repo = new DataGridNominas(connectionString);

            var empleados = repo.ObtenerEmpleados();
            dataGridNomina.ItemsSource = empleados;


        }


        void ActualizarSemanas()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            // Limpiar semanas cada vez que cambie año o mes
            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
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

        // MES
        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {



            Agregar_tiempo tiempo = new Agregar_tiempo();
            // Limpiar semanas cada vez que cambie año o mes
            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
            ActualizarSemanas();

            Variables.indexComboboxMes = ComBoxMes.SelectedItem.ToString();
        }
        private void CargarMeses()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            ComBoxMes.ItemsSource = tiempo.GetMeses();
            ComBoxMes.SelectedItem = DateTime.Now.Year;

        }

        // AÑO
        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Agregar_tiempo tiempo = new Agregar_tiempo();
            if (ComBoxAnio.ItemsSource == null)
            {
                ComBoxAnio.ItemsSource = tiempo.GetAnios(); // GetAnios() debe devolver una lista de int (años)
            }

            // Solo establecer el año actual si aún no hay un año seleccionado
            if (ComBoxAnio.SelectedItem == null || !(ComBoxAnio.SelectedItem is int))
            {
                int anioActual = DateTime.Now.Year;
                if (ComBoxAnio.Items.Contains(anioActual))
                {
                    ComBoxAnio.SelectedItem = Variables.indexComboboxAño;
                }
            }
            ActualizarSemanas();

            Variables.indexComboboxAño = ComBoxAnio.SelectedItem.ToString();
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




    }
}
