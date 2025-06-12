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
            if (GlobalVariables1.obra_nom != null)
            {
                CargarDatosComboBox();
                ObraComboBox.SelectedItem = GlobalVariables1.obra_nom;
                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;

                repo = new DataGridNominas(connectionString);

                var empleados = repo.ObtenerEmpleados();
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
            if (GlobalVariables1.fecha_fin == null || GlobalVariables1.fecha_inicio == null)
            {
                DateTime hoy = DateTime.Now;

                int diferenciaConLunes = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
                if (diferenciaConLunes < 0)
                    diferenciaConLunes += 7;

                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);

                string formato = "yyyy-MM-dd";

                GlobalVariables1.fecha_inicio = lunes.ToString(formato);
                GlobalVariables1.fecha_fin = domingo.ToString(formato);



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

            string usuario = GlobalVariables1.usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

            if (ObraComboBox.SelectedItem != null)
            {

                string nombre_obra = ObraComboBox.SelectedItem.ToString();
                MessageBox.Show("Seleccionaste: " + nombre_obra);

                GlobalVariables1.obra_nom = nombre_obra;

                int? id_obra = ObtenerID_obra(idAdmin, nombre_obra);
                GlobalVariables1.id_obra = id_obra;



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

        private void CargarDatosComboBox()
        {
            string usuario = GlobalVariables1.usuario;
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

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComBoxAnio.ItemsSource == null)
            {
                ComBoxAnio.ItemsSource = GetAnios(); // GetAnios() debe devolver una lista de int (años)
            }

            // Solo establecer el año actual si aún no hay un año seleccionado
            if (ComBoxAnio.SelectedItem == null || !(ComBoxAnio.SelectedItem is int))
            {
                int anioActual = DateTime.Now.Year;
                if (ComBoxAnio.Items.Contains(anioActual))
                {
                    ComBoxAnio.SelectedItem = anioActual;
                }
            }
            ActualizarSemanas();
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Limpiar semanas cada vez que cambie año o mes
            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = GetSemanasDelMes(anio, mes);
            }
            ActualizarSemanas();
        }
        private void ComBoxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            GlobalVariables1.fecha = ComBoxSemana.SelectedItem?.ToString() ?? string.Empty;

            var resultado = ObtenerFechasDesdeGlobal();

            if (resultado.exito)
            {
                // Ya puedes usar fechaInicio y fechaFin como DateTime
                DateTime inicio = resultado.fechaInicio;
                DateTime fin = resultado.fechaFin;

                // Opcional: convertir a formato SQL
                GlobalVariables1.fecha_inicio = inicio.ToString("yyyy-MM-dd");
                GlobalVariables1.fecha_fin = fin.ToString("yyyy-MM-dd");

                MessageBox.Show($"Desde: {GlobalVariables1.fecha_inicio} - Hasta: {GlobalVariables1.fecha_fin}");
            }
            else
            {
                MessageBox.Show("El formato de la semana seleccionada no es válido.");
            }

            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;
            repo = new DataGridNominas(connectionString);

            var empleados = repo.ObtenerEmpleados();
            dataGridNomina.ItemsSource = empleados;
        }
        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }




        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
        {
            CargarAnios();
        }


        private void ComBoxSemana_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void CargarAnios()
        {
            ComBoxAnio.ItemsSource = GetAnios();
            ComBoxAnio.SelectedItem = DateTime.Now.Year;
        }

        private void CargarMeses()
        {
            ComBoxMes.ItemsSource = GetMeses();
            ComBoxMes.SelectedItem = DateTime.Now.Year;

        }


        private List<int> GetAnios()
        {
            int anioActual = DateTime.Now.Year;
            List<int> anios = new List<int>();
            for (int i = anioActual - 5; i <= anioActual + 25; i++)
            {
                anios.Add(i);
            }
            return anios;
        }

        private List<string> GetMeses()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();
        }

        private List<string> GetSemanasDelMes(int anio, int mes)
        {
            List<string> semanas = new List<string>();
            DateTime primerDia = new DateTime(anio, mes, 1);
            DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);
            DateTime actual = primerDia;

            while (actual <= ultimoDia)
            {
                // Alinear al lunes anterior (o el mismo día si es lunes)
                DateTime inicioSemana = actual.DayOfWeek == DayOfWeek.Monday
                    ? actual
                    : actual.AddDays(-(int)actual.DayOfWeek + (actual.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

                DateTime finSemana = inicioSemana.AddDays(6);
                if (finSemana > ultimoDia)
                    finSemana = ultimoDia;

                if (finSemana >= primerDia)
                {
                    semanas.Add($"{inicioSemana:dd/MM/yyyy} - {finSemana:dd/MM/yyyy}");
                }

                actual = finSemana.AddDays(1);
            }

            return semanas.Distinct().ToList();
        }

        void ActualizarSemanas()
        {
            // Limpiar semanas cada vez que cambie año o mes
            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = GetSemanasDelMes(anio, mes);
            }
        }

        public (bool exito, DateTime fechaInicio, DateTime fechaFin) ObtenerFechasDesdeGlobal()
        {
            string texto = GlobalVariables1.fecha;

            if (string.IsNullOrWhiteSpace(texto))
                return (false, DateTime.MinValue, DateTime.MinValue);

            string[] partes = texto.Split(" - ");

            if (partes.Length == 2 &&
                DateTime.TryParseExact(partes[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime inicio) &&
                DateTime.TryParseExact(partes[1], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fin))
            {
                return (true, inicio, fin);
            }

            return (false, DateTime.MinValue, DateTime.MinValue);
        }
    }
}
