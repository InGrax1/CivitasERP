// ListaPage.xaml.cs - Adaptado con el nuevo Header de Calendario en Popup

using CivitasERP.Models;
using CivitasERP.Conexiones;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CivitasERP.Models.Datagrid_lista;
using System.Data.SqlClient;
using System;
using System.Linq;
using CivitasERP.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace CivitasERP.Views.Usuario
{
    public partial class ListaPage : Page
    {
        private Datagrid_lista repo;
        private string _connectionString;
        private DB_admins _dbAdmins;
        private bool _isInitializing = true;
        private bool _updatingCalendar = false;

        // Propiedades para el rango seleccionado en el calendar
        private DateTime? FechaInicioSeleccionada => CalendarRango.SelectedDates.Count > 0 ?
            CalendarRango.SelectedDates.Min() : (DateTime?)null;

        private DateTime? FechaFinSeleccionada => CalendarRango.SelectedDates.Count > 0 ?
            CalendarRango.SelectedDates.Max() : (DateTime?)null;

        public ListaPage()
        {
            InitializeComponent();
            DataContext = new ListaViewModel();

            // Cachear conexión y objetos reutilizables
            _connectionString = new DBConexion().ObtenerCadenaConexion();
            _dbAdmins = new DB_admins();
            repo = new Datagrid_lista(_connectionString);

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

                // Carga inicial de empleados con la fecha por defecto
                await CargarEmpleadosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar la página de lista: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            else
            {
                Stack3Menu.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfigurarCalendarioPorDefecto()
        {
            if (_updatingCalendar) return;
            _updatingCalendar = true;

            try
            {
                // Configurar semana actual COMPLETA (Lunes a Domingo)
                DateTime hoy = DateTime.Now;
                int diferenciaConLunes = (int)hoy.DayOfWeek == 0 ? 6 : (int)hoy.DayOfWeek - 1;
                DateTime lunes = hoy.AddDays(-diferenciaConLunes);
                DateTime domingo = lunes.AddDays(6);

                CalendarRango.SelectedDates.Clear();
                for (DateTime fecha = lunes; fecha <= domingo; fecha = fecha.AddDays(1))
                {
                    CalendarRango.SelectedDates.Add(fecha);
                }

                CalendarRango.DisplayDate = lunes;

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
                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");
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

        #region Eventos y Lógica del Calendario (Copiado de NominaPage)

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

            if (FechaInicioSeleccionada.HasValue && FechaFinSeleccionada.HasValue)
            {
                await CargarEmpleadosAsync();
                if (CalendarRango.SelectedDates.Count > 1)
                {
                    PopupCalendario.IsOpen = false;
                }
            }
        }

        private void CalendarRango_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (CalendarRango.DisplayMode != CalendarMode.Month)
            {
                CalendarRango.DisplayMode = CalendarMode.Month;
            }
        }

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

        private async Task SeleccionarRangoAsync(Func<(DateTime inicio, DateTime fin)> obtenerRango)
        {
            _updatingCalendar = true;
            try
            {
                var (inicio, fin) = obtenerRango();
                CalendarRango.SelectedDates.Clear();
                for (DateTime fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
                {
                    CalendarRango.SelectedDates.Add(fecha);
                }
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

        #endregion

        private async Task CargarDatosInicialesAsync()
        {
            // Cargamos los administradores en un hilo separado
            await Task.Run(() => CargarAdministradores());
        }

        private void ProcesarVariablesGuardadas()
        {
            // Restaura el rango de fechas si existe
            if (!string.IsNullOrEmpty(Variables.FechaInicio) && !string.IsNullOrEmpty(Variables.FechaFin))
            {
                _updatingCalendar = true;
                try
                {
                    if (DateTime.TryParse(Variables.FechaInicio, out DateTime fechaInicio) &&
                        DateTime.TryParse(Variables.FechaFin, out DateTime fechaFin))
                    {
                        CalendarRango.SelectedDates.Clear();
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

            // Restaura otras selecciones
            if (Variables.AdminSeleccionado != null)
                AdminComboBox.SelectedItem = Variables.AdminSeleccionado;

            if (Variables.ObraNom != null)
            {
                _ = CargarObrasAsync();
                ObraComboBox.SelectedItem = Variables.ObraNom;
            }
        }

        // MÉTODO PRINCIPAL DE CARGA DE DATOS (Adaptado del original)
        private async Task CargarEmpleadosAsync()
        {
            if (_isInitializing) return;

            // Verificar que tengamos un rango válido
            if (!FechaInicioSeleccionada.HasValue || !FechaFinSeleccionada.HasValue)
            {
                dataGridAsistencia.ItemsSource = null;
                TotalPersonal.Text = "0";
                return;
            }

            try
            {
                int? idAdmin = GetSelectedAdminId();
                // Usamos Task.Run para no bloquear la UI mientras se consulta la BD
                var empleados = await Task.Run(() => repo.ObtenerEmpleados(idAdmin));

                dataGridAsistencia.ItemsSource = empleados;
                TotalPersonal.Text = empleados.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de asistencia: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dataGridAsistencia.ItemsSource = null;
                TotalPersonal.Text = "0";
            }
        }

        #region Lógica de ComboBoxes (Admin y Obra)

        private int? GetSelectedAdminId()
        {
            string usuarioSeleccionado = AdminComboBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(usuarioSeleccionado))
            {
                return _dbAdmins.ObtenerIdPorUsuario(usuarioSeleccionado);
            }
            return _dbAdmins.ObtenerIdPorUsuario(Variables.Usuario); // Valor por defecto
        }

        private async void AdminComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || AdminComboBox.SelectedItem == null) return;

            string nombreadmin = AdminComboBox.SelectedItem.ToString();
            Variables.IdAdmin = _dbAdmins.ObtenerIdPorUsuario(nombreadmin);
            Variables.AdminSeleccionado = nombreadmin;

            // Limpiar selección de obra anterior
            ObraComboBox.SelectedItem = null;
            UbicacionLabel.Text = "";
            Variables.IdObra = null;
            Variables.ObraNom = null;

            await CargarObrasAsync();
            await CargarEmpleadosAsync();
        }
        private async void AdminComboBox_DropDownOpened(object sender, EventArgs e)
        {
            
        }
        private async void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (AdminComboBox.SelectedValue == null && Variables.Jefe)
            {
                MessageBox.Show("⚠️ Primero selecciona un administrador antes de elegir la obra.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await CargarObrasAsync();
        }

        private async void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || ObraComboBox.SelectedItem == null) return;

            string nombreObra = ObraComboBox.SelectedItem.ToString();
            Variables.ObraNom = nombreObra;

            int? idAdmin = GetSelectedAdminId();
            Variables.IdObra = await ObtenerIdObraAsync(idAdmin, nombreObra);
            UbicacionLabel.Text = await ObtenerUbicacionObraAsync(Variables.IdObra);

            await CargarEmpleadosAsync();
        }

        #endregion

        #region Métodos de Acceso a Datos (Async)

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

        private async Task CargarObrasAsync()
        {
            int? idAdminObra = GetSelectedAdminId();
            if (idAdminObra == null) return;

            try
            {
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
                MessageBox.Show("Error al cargar obras: " + ex.Message);
            }
        }

        private async Task<int?> ObtenerIdObraAsync(int? idAdminObra, string obraNombre)
        {
            if (idAdminObra == null || string.IsNullOrWhiteSpace(obraNombre)) return null;

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

        private async Task<string> ObtenerUbicacionObraAsync(int? idObra)
        {
            if (idObra == null) return string.Empty;

            return await Task.Run(() =>
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT obra_ubicacion FROM obra WHERE id_obra = @IdObra";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdObra", idObra);
                        conn.Open();
                        var resultado = cmd.ExecuteScalar();
                        return resultado?.ToString() ?? string.Empty;
                    }
                }
            });
        }

        #endregion

        // Botón de huella (si aplica)
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ListaViewModel vm)
                vm.ScanCommand.Execute(null);
        }
    }
}