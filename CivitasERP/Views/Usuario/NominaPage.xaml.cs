// NominaPage.xaml.cs - Code Behind con Calendar en Popup

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
using System.Windows.Controls.Primitives;

namespace CivitasERP.Views.Usuario
{
    public partial class NominaPage : Page
    {
        private DataGridNominas repo;
        private NuevoEmpleadoPage _NuevoEmpleadoPage = null;
        private string _connectionString;
        private DB_admins _dbAdmins;
        private bool _isInitializing = true;
        private bool _updatingCalendar = false;

        // Propiedades para el rango seleccionado en el calendar
        private DateTime? FechaInicioSeleccionada => CalendarRango.SelectedDates.Count > 0 ?
            CalendarRango.SelectedDates.Min() : (DateTime?)null;

        private DateTime? FechaFinSeleccionada => CalendarRango.SelectedDates.Count > 0 ?
            CalendarRango.SelectedDates.Max() : (DateTime?)null;

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
                ConfigurarUI();
                await CargarDatosInicialesAsync();
                ConfigurarCalendarioPorDefecto();
                ProcesarVariablesGuardadas();
                _isInitializing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ConfigurarCalendarioPorDefecto()
        {
            if (_updatingCalendar) return;
            _updatingCalendar = true;

            try
            {
                // Configurar semana actual (Lunes a Domingo)
                DateTime hoy = DateTime.Now;
                int diferenciaConLunes = (int)hoy.DayOfWeek == 0 ? 6 : (int)hoy.DayOfWeek - 1;
                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);

                // Seleccionar rango en el calendario
                CalendarRango.SelectedDates.Clear();

                // Agregar todas las fechas del rango
                for (DateTime fecha = lunes; fecha <= domingo; fecha = fecha.AddDays(1))
                {
                    CalendarRango.SelectedDates.Add(fecha);
                }

                // Establecer fecha de visualización en el lunes
                CalendarRango.DisplayDate = lunes;

                // Actualizar variables globales
                Variables.FechaInicio = lunes.ToString("yyyy-MM-dd");
                Variables.FechaFin = domingo.ToString("yyyy-MM-dd");

                ActualizarInfoRango();
            }
            finally
            {
                _updatingCalendar = false;
            }
        }

        private void ActualizarInfoRango()
        {
            if (FechaInicioSeleccionada.HasValue && FechaFinSeleccionada.HasValue)
            {
                var inicio = FechaInicioSeleccionada.Value;
                var fin = FechaFinSeleccionada.Value;
                var duracion = (fin - inicio).Days + 1;

                lblRangoInfo.Text = $"{inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy} ({duracion} día{(duracion != 1 ? "s" : "")})";

                // Actualizar variables globales
                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");

                // Actualizar el texto del botón
                btnSeleccionarFechas.Content = $"📅 {inicio:dd/MM} - {fin:dd/MM}";
            }
            else if (CalendarRango.SelectedDates.Count == 1)
            {
                var fecha = CalendarRango.SelectedDates.First();
                lblRangoInfo.Text = $"{fecha:dd/MM/yyyy} (selecciona fecha final)";
                Variables.FechaInicio = fecha.ToString("yyyy-MM-dd");
                Variables.FechaFin = fecha.ToString("yyyy-MM-dd");
                btnSeleccionarFechas.Content = $"📅 {fecha:dd/MM} (incompleto)";
            }
            else
            {
                lblRangoInfo.Text = "Selecciona un rango de fechas";
                Variables.FechaInicio = null;
                Variables.FechaFin = null;
                btnSeleccionarFechas.Content = "📅 Seleccionar fechas";
            }
        }

        // Event Handlers del Popup y Calendar
        private void AbrirCalendario_Click(object sender, RoutedEventArgs e)
        {
            PopupCalendario.IsOpen = true;
        }

        private void CerrarCalendario_Click(object sender, RoutedEventArgs e)
        {
            PopupCalendario.IsOpen = false;
        }

        private async void CalendarRango_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _updatingCalendar) return;

            ActualizarInfoRango();

            // Solo cargar empleados si tenemos un rango completo
            if (FechaInicioSeleccionada.HasValue && FechaFinSeleccionada.HasValue)
            {
                await CargarEmpleadosAsync();

                // Cerrar el popup automáticamente cuando se completa la selección
                if (CalendarRango.SelectedDates.Count > 1)
                {
                    PopupCalendario.IsOpen = false;
                }
            }
        }

        private void CalendarRango_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            // Mantener el modo de día para permitir selección de rango
            if (CalendarRango.DisplayMode != CalendarMode.Month)
            {
                CalendarRango.DisplayMode = CalendarMode.Month;
            }
        }

        // Métodos para botones de rango rápido
        private async void BtnSemanaActual_Click(object sender, RoutedEventArgs e)
        {
            await SeleccionarRangoAsync(() =>
            {
                DateTime hoy = DateTime.Now;
                int diferenciaConLunes = (int)hoy.DayOfWeek == 0 ? 6 : (int)hoy.DayOfWeek - 1;
                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);
                return (lunes, domingo);
            });
        }

        private async void BtnMesActual_Click(object sender, RoutedEventArgs e)
        {
            await SeleccionarRangoAsync(() =>
            {
                DateTime hoy = DateTime.Now;
                DateTime primerDia = new DateTime(hoy.Year, hoy.Month, 1);
                DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);
                return (primerDia, ultimoDia);
            });
        }

        private async void BtnSemanaAnterior_Click(object sender, RoutedEventArgs e)
        {
            await SeleccionarRangoAsync(() =>
            {
                DateTime hoy = DateTime.Now;
                int diferenciaConLunes = (int)hoy.DayOfWeek == 0 ? 6 : (int)hoy.DayOfWeek - 1;
                DateTime lunesActual = hoy.AddDays(-diferenciaConLunes);
                DateTime lunesAnterior = lunesActual.AddDays(-7);
                DateTime domingoAnterior = lunesAnterior.AddDays(6);
                return (lunesAnterior, domingoAnterior);
            });
        }

        private async void BtnMesAnterior_Click(object sender, RoutedEventArgs e)
        {
            await SeleccionarRangoAsync(() =>
            {
                DateTime hoy = DateTime.Now;
                DateTime mesAnterior = hoy.AddMonths(-1);
                DateTime primerDia = new DateTime(mesAnterior.Year, mesAnterior.Month, 1);
                DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);
                return (primerDia, ultimoDia);
            });
        }

        // Método helper para seleccionar rangos
        private async Task SeleccionarRangoAsync(Func<(DateTime inicio, DateTime fin)> obtenerRango)
        {
            _updatingCalendar = true;
            try
            {
                var (inicio, fin) = obtenerRango();

                CalendarRango.SelectedDates.Clear();

                // Agregar todas las fechas del rango
                for (DateTime fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
                {
                    CalendarRango.SelectedDates.Add(fecha);
                }

                // Establecer fecha de visualización
                CalendarRango.DisplayDate = inicio;

                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");

                ActualizarInfoRango();
            }
            finally
            {
                _updatingCalendar = false;
            }

            await CargarEmpleadosAsync();
        }

        private async Task CargarDatosInicialesAsync()
        {
            var adminsTask = Task.Run(() => CargarAdministradores());
            await adminsTask;
        }

        private async Task CargarEmpleadosAsync()
        {
            if (_isInitializing) return;

            // Verificar que tengamos un rango válido
            if (!FechaInicioSeleccionada.HasValue || !FechaFinSeleccionada.HasValue)
            {
                dataGridNomina.ItemsSource = null;
                LimpiarTotales();
                return;
            }

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
                MessageBox.Show($"Error al cargar empleados: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LimpiarTotales();
            }
        }

        private void ProcesarVariablesGuardadas()
        {
            if (!string.IsNullOrEmpty(Variables.FechaInicio) && !string.IsNullOrEmpty(Variables.FechaFin))
            {
                _updatingCalendar = true;
                try
                {
                    if (DateTime.TryParse(Variables.FechaInicio, out DateTime fechaInicio) &&
                        DateTime.TryParse(Variables.FechaFin, out DateTime fechaFin))
                    {
                        CalendarRango.SelectedDates.Clear();

                        // Agregar todas las fechas del rango guardado
                        for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
                        {
                            CalendarRango.SelectedDates.Add(fecha);
                        }

                        CalendarRango.DisplayDate = fechaInicio;
                        ActualizarInfoRango();
                    }
                }
                finally
                {
                    _updatingCalendar = false;
                }
            }

            // Procesar otras variables guardadas (Admin, Obra, etc.)
            if (Variables.AdminSeleccionado != null)
                AdminComboBox.SelectedItem = Variables.AdminSeleccionado;

            if (Variables.ObraNom != null)
            {
                _ = CargarObrasAsync();
                ObraComboBox.SelectedItem = Variables.ObraNom;
            }
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
                    _ = CargarEmpleadosAsync();
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

        private void LimpiarTotales()
        {
            TotalPersonal.Text = "0";
            TotalSuelJornal.Text = "$0.00";
            TotalSuelSemanal.Text = "$0.00";
            TotalHrsExtra.Text = "0.00";
            TotalPrecioHrsExt.Text = "$0.00";
            TotalSuelExt.Text = "$0.00";
            TotalSuelTrabajado.Text = "$0.00";
            TotalGeneral.Text = "$0.00";
        }

        private void CalcularTotales(IEnumerable<DataGridNominas.Empleado> lista)
        {
            if (lista == null)
            {
                LimpiarTotales();
                return;
            }

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

        private string ObtenerConexion()
        {
            return new DBConexion().ObtenerCadenaConexion();
        }

        private void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // Los administradores ya se cargan en Initialize
        }
    }
}