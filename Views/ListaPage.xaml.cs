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
using static CivitasERP.Models.Datagrid_lista;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Data;
using BiometriaDP.Services;
using static CivitasERP.Views.LoginPage;
using System.Globalization;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para ListaPage.xaml
    /// </summary>
    public partial class ListaPage : Page
    {
        private Datagrid_lista repo;

        private readonly string _connectionString;
        private FingerprintService _fingerService;
        public ListaPage()
        {
            InitializeComponent();

    

            // 1) Obtener cadena de conexión y crear el servicio
            _connectionString = new Conexion().ObtenerCadenaConexion();
            _fingerService = new FingerprintService(_connectionString);
            // 2) Suscribirse a eventos de verificación
            _fingerService.OnVerificationComplete += FingerService_OnVerificationComplete;
            _fingerService.OnError += FingerService_OnError;
        }


        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _fingerService.StartVerification();
                lblEstadoHuella.Text = "⏳ Coloca tu dedo para validar...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar verificación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FingerService_OnVerificationComplete(object sender, FingerprintVerifiedEventArgs e)
        {
            int idEmpleadoMatch = e.IdEmpleado;
            if (idEmpleadoMatch > 0)
            {
                // Marcar hora de entrada/salida en la base de datos
                var repo = new Datagrid_lista(new Conexion().ObtenerCadenaConexion());
                repo.MarcarAsistencia(idEmpleadoMatch);

                Dispatcher.Invoke(() =>
                {
                    lblEstadoHuella.Text = $"✔ Huella reconocida. Empleado #{idEmpleadoMatch}.";
                    MessageBox.Show($"Asistencia registrada para empleado #{idEmpleadoMatch}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarEmpleadosEnGrilla();
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    lblEstadoHuella.Text = "❌ Huella no reconocida.";
                    MessageBox.Show("Huella no coincide con ningún empleado.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void FingerService_OnError(object sender, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                lblEstadoHuella.Text = $"❌ {mensaje}";
                MessageBox.Show(mensaje, "Error huella", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        private void CargarEmpleadosEnGrilla()
        {
            // Crea un repositorio con la cadena de conexión y obtiene la lista de empleados/asistencias
            var repo = new Datagrid_lista(new Conexion().ObtenerCadenaConexion());
            var listaEmpleados = repo.ObtenerEmpleados();

            // Asigna esa lista al DataGrid de la UI
            dataGridAsistencia.ItemsSource = listaEmpleados;
        }

        /// <summary>
        /// Devuelve una lista de tuplas (idEmpleado, byte[] template) leídas desde la tabla 'admins'.
        /// </summary>
        private List<Tuple<int, byte[]>> ObtenerTemplatesDesdeBD()
        {
            var lista = new List<Tuple<int, byte[]>>();
            string connectionString = new Conexion().ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT admins_id, admin_huella 
                               FROM admins 
                               WHERE admin_huella IS NOT NULL";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        byte[] huellaBytes = reader["admin_huella"] as byte[];
                        lista.Add(new Tuple<int, byte[]>(id, huellaBytes));
                    }
                }
                conn.Close();
            }
            return lista;
        }

        /// <summary>
        /// Dado un ID de usuario, busca su nombre en la tabla 'admins' para mostrarlo.
        /// </summary>
        private string ObtenerUsernamePorId(int userId)
        {
            string nombreUsuario = string.Empty;
            string connectionString = new Conexion().ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT admins_usuario 
                               FROM admins 
                               WHERE admins_id = @Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", userId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        nombreUsuario = result.ToString();
                }
                conn.Close();
            }
            return nombreUsuario;
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
        }

        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
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
        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
        {
            CargarAnios();
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

        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string usuario = GlobalVariables1.usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdmin = dB_Admins.ObtenerIdPorUsuario(usuario);

            if (ObraComboBox.SelectedItem != null)
            {
                string nombre_obra = ObraComboBox.SelectedItem.ToString();
                MessageBox.Show("Seleccionaste: " + nombre_obra);

                int? id_obra = ObtenerID_obra(idAdmin, nombre_obra);
                GlobalVariables1.id_obra = id_obra;



                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;

                repo = new Datagrid_lista(connectionString);

                var empleados = repo.ObtenerEmpleados();
                dataGridAsistencia.ItemsSource = empleados;

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
        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
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

    }
}

