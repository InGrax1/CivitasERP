using CivitasERP.Models;
using CivitasERP.Conexiones;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using GlobalCalendar = System.Globalization.Calendar;

namespace CivitasERP.Views.Usuario
{
    public partial class NominaPage : Page
    {
        private DataGridNominas repo;
        private NuevoEmpleadoPage _NuevoEmpleadoPage = null;
        private string _connectionString;
        private DB_admins _dbAdmins;
        private bool _isInitializing = true;

        public NominaPage()
        {
            InitializeComponent();

            // Cachear conexión y objetos reutilizables
            _connectionString = ObtenerConexion();
            _dbAdmins = new DB_admins();
            repo = new DataGridNominas(_connectionString);

            // Cargar datos de forma asíncrona
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Mostrar indicador de carga (opcional)
                // LoadingIndicator.Visibility = Visibility.Visible;

                // Cargar configuración de UI sin bloquear
                ConfigurarUI();

                // Cargar datos iniciales de forma asíncrona
                await CargarDatosInicialesAsync();

                // Procesar variables guardadas
                ProcesarVariablesGuardadas();

                _isInitializing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Ocultar indicador de carga
                // LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfigurarUI()
        {
            // Configurar visibilidad según tipo de usuario
            if (Variables.Jefe == false)
            {
                AdminLabel.Visibility = Visibility.Collapsed;
                AdminComboBox.Visibility = Visibility.Collapsed;
                AdminBorder.Visibility = Visibility.Collapsed;
            }
            else if (Variables.Jefe == true)
            {
                btnNuevoEmpleadoBorder.Visibility = Visibility.Collapsed;
            }
        }

        private async Task CargarDatosInicialesAsync()
        {
            // 1) Lee datos en background
            var adminsTask = Task.Run(() => {
                CargarAdministradores(); // este usa Dispatcher.Invoke internamente
            });
            var aniosTask = Task.Run(() => new Agregar_tiempo().GetAnios());
            var mesesTask = Task.Run(() => new Agregar_tiempo().GetMeses());

            // 2) Espera resultados
            await adminsTask;
            var anios = await aniosTask;
            var meses = await mesesTask;

            // 3) Actualiza UI en el hilo correcto
            Dispatcher.Invoke(() =>
            {
                ComBoxAnio.ItemsSource = anios;
                ComBoxAnio.SelectedItem = DateTime.Now.Year;

                ComBoxMes.ItemsSource = meses;
                ComBoxMes.SelectedItem = DateTime.Now.Month;
            });
        }


        private void ProcesarVariablesGuardadas()
        {
            // Procesar variables guardadas sin recargar innecesariamente
            if (Variables.AdminSeleccionado != null)
            {
                AdminComboBox.SelectedItem = Variables.AdminSeleccionado;
            }

            if (Variables.ObraNom != null)
            {
                _ = CargarObrasAsync();
                ObraComboBox.SelectedItem = Variables.ObraNom;
            }

            if (Variables.indexComboboxAño != null)
            {
                ComBoxAnio.SelectedItem = Variables.indexComboboxAño;
            }

            if (Variables.indexComboboxMes != null)
            {
                ComBoxMes.SelectedItem = Variables.indexComboboxMes;
                ActualizarSemanas();
            }

            if (Variables.indexComboboxSemana != null)
            {
                ComBoxSemana.SelectedItem = Variables.indexComboboxSemana;
            }
        }

        private string ObtenerConexion()
        {
            return new DBConexion().ObtenerCadenaConexion();
        }

        private async Task CargarEmpleadosAsync()
        {
            if (_isInitializing) return;

            try
            {
                int? idAdmin = AdminComboBox.SelectedItem != null
                    ? _dbAdmins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString())
                    : Variables.IdAdmin;

                string usuario = AdminComboBox.SelectedItem?.ToString() ?? Variables.Usuario;

                var empleados = await Task.Run(() => repo.ObtenerEmpleados(idAdmin, usuario));

                dataGridNomina.ItemsSource = empleados;
                CalcularTotales(empleados);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar empleados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarEmpleados()
        {
            _ = CargarEmpleadosAsync();
        }

        private void btnNuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (_NuevoEmpleadoPage == null)
            {
                _NuevoEmpleadoPage = new NuevoEmpleadoPage();
                _NuevoEmpleadoPage.Closed += (s, args) =>
                {
                    _NuevoEmpleadoPage = null;
                    // Recargar empleados después de cerrar
                    CargarEmpleados();
                };
                _NuevoEmpleadoPage.ShowDialog();
            }
            else
            {
                _NuevoEmpleadoPage.Activate();
            }
        }

        private async void BtnEliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is DataGridNominas.Empleado empleado)
            {
                var resp = MessageBox.Show(
                    $"¿Seguro que deseas eliminar a {empleado.Nombre}?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resp != MessageBoxResult.Yes) return;

                try
                {
                    await repo.EliminarEmpleadoAsync(empleado.ID);
                    await CargarEmpleadosAsync();

                    MessageBox.Show("Empleado eliminado correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            await CargarObrasAsync();
        }

        private async void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            await ProcesarSeleccionObraAsync();
        }

        private async Task ProcesarSeleccionObraAsync()
        {
            // Configurar fechas por defecto solo si no existen
            if (Variables.FechaFin == null || Variables.FechaInicio == null)
            {
                ConfigurarFechasPorDefecto();
            }

            if (ObraComboBox.SelectedItem != null)
            {
                string nombreObra = ObraComboBox.SelectedItem.ToString();
                Variables.ObraNom = nombreObra;

                int? idAdmin = AdminComboBox.SelectedItem != null
                    ? _dbAdmins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString())
                    : _dbAdmins.ObtenerIdPorUsuario(Variables.Usuario);

                var tasks = new List<Task>();

                // Ejecutar operaciones en paralelo
                tasks.Add(Task.Run(async () =>
                {
                    Variables.IdObra = await ObtenerIdObraAsync(idAdmin, nombreObra);
                    var ubicacion = await ObtenerUbicacionObraAsync(Variables.IdObra);

                    Dispatcher.Invoke(() => UbicacionLabel.Text = ubicacion);
                }));

                tasks.Add(CargarEmpleadosAsync());

                await Task.WhenAll(tasks);
            }
        }

        private void ConfigurarFechasPorDefecto()
        {
            DateTime hoy = DateTime.Now;
            int diferenciaConLunes = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
            if (diferenciaConLunes < 0) diferenciaConLunes += 7;

            DateTime lunes = hoy.AddDays(-diferenciaConLunes);
            DateTime domingo = lunes.AddDays(6);

            Variables.FechaInicio = lunes.ToString("yyyy-MM-dd");
            Variables.FechaFin = domingo.ToString("yyyy-MM-dd");

            // Configurar selecciones por defecto
            var cultura = CultureInfo.InvariantCulture;
            var calendario = cultura.Calendar;
            int numeroSemana = calendario.GetWeekOfYear(hoy, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            ComBoxSemana.SelectedIndex = numeroSemana;
            ComBoxMes.SelectedIndex = hoy.Month - 1;
            ComBoxAnio.SelectedItem = hoy.Year.ToString();
        }

        private async Task<int?> ObtenerIdObraAsync(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre))
                return null;

            return await Task.Run(() =>
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                        cmd.Parameters.AddWithValue("@obraNombre", obraNombre);
                        conn.Open();

                        var resultado = cmd.ExecuteScalar();
                        return resultado != null && int.TryParse(resultado.ToString(), out int idObra) ? idObra : (int?)null;
                    }
                }
            });
        }

        private async Task CargarObrasAsync()
        {
            try
            {
                int? idAdminObra = AdminComboBox.SelectedItem != null
                    ? _dbAdmins.ObtenerIdPorUsuario(AdminComboBox.SelectedItem.ToString())
                    : _dbAdmins.ObtenerIdPorUsuario(Variables.Usuario);

                var obras = await Task.Run(() =>
                {
                    var listaObras = new List<string>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                            conn.Open();

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    listaObras.Add(reader["obra_nombre"].ToString());
                                }
                            }
                        }
                    }
                    return listaObras;
                });

                ObraComboBox.Items.Clear();
                foreach (var obra in obras)
                {
                    ObraComboBox.Items.Add(obra);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar obras: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ComBoxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            Variables.Fecha = ComBoxSemana.SelectedItem?.ToString() ?? string.Empty;
            Variables.indexComboboxSemana = ComBoxSemana.SelectedItem?.ToString();

            var tiempo = new Agregar_tiempo();
            var resultado = tiempo.ObtenerFechasDesdeGlobal();

            if (resultado.exito)
            {
                Variables.FechaInicio = resultado.fechaInicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = resultado.fechaFin.ToString("yyyy-MM-dd");
            }

            await CargarEmpleadosAsync();
        }

        private void ActualizarSemanas()
        {
            var tiempo = new Agregar_tiempo();

            ComBoxSemana.ItemsSource = null;
            ComBoxSemana.SelectedItem = null;

            if (ComBoxAnio.SelectedItem is int anio && ComBoxMes.SelectedIndex >= 0)
            {
                int mes = ComBoxMes.SelectedIndex + 1;
                ComBoxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            ActualizarSemanas();
            Variables.indexComboboxMes = ComBoxMes.SelectedItem?.ToString();
        }

        private void CargarMeses()
        {
            var tiempo = new Agregar_tiempo();
            ComBoxMes.ItemsSource = tiempo.GetMeses();
            ComBoxMes.SelectedItem = DateTime.Now.Month;
        }

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            ActualizarSemanas();
            Variables.indexComboboxAño = ComBoxAnio.SelectedItem?.ToString();
        }

        private void CargarAnios()
        {
            var tiempo = new Agregar_tiempo();
            ComBoxAnio.ItemsSource = tiempo.GetAnios();
            ComBoxAnio.SelectedItem = DateTime.Now.Year;
        }

        private void CalcularTotales(IEnumerable<DataGridNominas.Empleado> lista)
        {
            if (lista == null) return;

            var empleados = lista.ToList(); // Materializar una vez

            TotalPersonal.Text = empleados.Count.ToString();
            TotalSuelJornal.Text = empleados.Sum(e => e.SueldoJornada).ToString("C2");
            TotalSuelSemanal.Text = empleados.Sum(x => x.SueldoSemanal).ToString("C2");
            TotalHrsExtra.Text = empleados.Sum(x => x.HorasExtra).ToString("N2");
            TotalPrecioHrsExt.Text = empleados.Sum(x => x.PHoraExtra).ToString("C2");
            TotalSuelExt.Text = empleados.Sum(x => x.SuelExtra).ToString("C2");
            TotalSuelTrabajado.Text = empleados.Sum(x => x.SuelTrabajado).ToString("C2");
            TotalGeneral.Text = empleados.Sum(x => x.SuelTotal).ToString("C2");
        }

        private async Task<string> ObtenerUbicacionObraAsync(int? idObra)
        {
            if (idObra == null) return string.Empty;

            return await Task.Run(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT obra_ubicacion FROM obra WHERE id_obra = @IdObra", connection))
                {
                    command.Parameters.AddWithValue("@IdObra", idObra);
                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            return reader.Read() ? reader["obra_ubicacion"].ToString() : string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener ubicación: {ex.Message}");
                        return string.Empty;
                    }
                }
            });
        }

        private async void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (AdminComboBox.SelectedItem != null)
            {
                string nombreAdmin = AdminComboBox.SelectedItem.ToString();
                Variables.IdAdmin = _dbAdmins.ObtenerIdPorUsuario(nombreAdmin);
                Variables.AdminSeleccionado = nombreAdmin;

                // Recargar obras y empleados
                await CargarObrasAsync();
                await CargarEmpleadosAsync();
            }
        }

        private void CargarAdministradores()
        {
            try
            {
                var admins = new List<string>();
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT admins_usuario FROM admins";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                admins.Add(reader["admins_usuario"].ToString());
                            }
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    AdminComboBox.Items.Clear();
                    foreach (var admin in admins)
                    {
                        AdminComboBox.Items.Add(admin);
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error al cargar administradores: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        // Eventos simplificados que no requieren lógica adicional
        private void ComBoxSemana_DropDownOpened(object sender, EventArgs e) => CargarMeses();
        private void ComBoxMes_DropDownOpened(object sender, EventArgs e) => CargarMeses();
        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e) => CargarAnios();
        private void AdminComboBox_DropDownOpened(object sender, EventArgs e) { } // Ya se carga en Initialize
    }
}