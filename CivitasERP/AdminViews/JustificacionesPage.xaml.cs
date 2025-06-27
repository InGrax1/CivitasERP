using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CivitasERP.AdminViews
{
    /// <summary>
    /// Lógica de interacción para JustificacionesPage.xaml
    /// </summary>
    public partial class JustificacionesPage : Page
    {
        public JustificacionesPage()
        {
            InitializeComponent();
        }
        private void BtnGuardarJustificacion_Click(object sender, RoutedEventArgs e)
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
                            Admin2ComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                Admin2ComboBox.Items.Add(reader["admin_nombre"].ToString());
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


        private void cargar_empleados(int? id_obra)
        {
            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    // Suponiendo que quieres obtener todos los admins, o filtrar de alguna forma
                    string query = @"SELECT
                            CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) AS emple_nombre
                     FROM empleado 
                     WHERE 
                         id_obra=@Id_obra;
                        ";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id_obra", id_obra);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            EmpleadoComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                EmpleadoComboBox.Items.Add(reader["emple_nombre"].ToString());
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


        private void CargarDatosComboBox()
        {

            try
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (var conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra ";
                    using (var cmd = new SqlCommand(query, conn))
                    {
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


        private int? ObtenerID_obra( string obraNombre)
        {
            if (string.IsNullOrWhiteSpace(obraNombre))
                return null;

            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();

            using (var conn = new SqlConnection(connectionString))
            {
                string query = "SELECT id_obra FROM obra WHERE  obra_nombre = @obraNombre";
                using (var cmd = new SqlCommand(query, conn))
                {

                    cmd.Parameters.AddWithValue("@obraNombre", obraNombre);
                    conn.Open();

                    object resultado = cmd.ExecuteScalar();
                    if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                        return idObra;

                    return null;
                }
            }
        }


        private void Admin2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Admin2ComboBox.SelectedItem != null)
            {
                string admin = Admin2ComboBox.SelectedItem.ToString();
                DB_admins admins = new DB_admins();
                EmpleadoComboBox.Items.Clear();
                ObraComboBox.Items.Clear();
            }
            else
            {
                // Opcional: limpia los datos o muestra un mensaje
                Console.WriteLine("No se ha seleccionado ningún administrador.");
            }


        }

        private void Admin2ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            cargar_admin();
        }



        private void ObraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObraComboBox.SelectedItem != null)
            {

                DB_admins admins = new DB_admins();
                int? id_obra = 0;
                id_obra = ObtenerID_obra( ObraComboBox.SelectedItem.ToString());


                cargar_empleados(id_obra);
                Admin2ComboBox.Items.Clear();
            }


        }
        private void ObraComboBox_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
        }



        private void EmpleadoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmpleadoComboBox.SelectedItem != null)
            {
                DB_admins admins = new DB_admins();
                int? id_obra = 0;
                id_obra = ObtenerID_obra(ObraComboBox.SelectedItem.ToString());
                Admin2ComboBox.Items.Clear();
            }

        }
        private void EmpleadoComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (ObraComboBox.SelectedItem != null)
            {

                DB_admins admins = new DB_admins();
                int? id_obra = 0;
                id_obra = ObtenerID_obra(ObraComboBox.SelectedItem.ToString());


                cargar_empleados(id_obra);

            }
        }

        public int? ObtenerIdEmpleado(string nombre)
        {
            int? idEmpleado = null;

            string query = @"SELECT id_empleado ,CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) AS emple_nombre
                         FROM empleado 
                         WHERE CONCAT(emp_nombre, ' ',emp_apellidop, ' ', emp_apellidom) = @nombre ;";
            Conexion conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nombre", nombre);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        idEmpleado = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    // Manejo del error: podrías loguear o lanzar la excepción
                    Console.WriteLine("Error al obtener el ID del empleado: " + ex.Message);
                }
            }

            return idEmpleado;
        }








        Agregar_tiempo tiempo = new Agregar_tiempo();




        private void CbxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            if (CbxAnio.ItemsSource == null)
            {
                CbxAnio.ItemsSource = tiempo.GetAnios(); // GetAnios() debe devolver una lista de int (años)
            }

            // Solo establecer el año actual si aún no hay un año seleccionado
            if (CbxAnio.SelectedItem == null || !(CbxAnio.SelectedItem is int))
            {
                int anioActual = DateTime.Now.Year;
                if (CbxAnio.Items.Contains(anioActual))
                {
                    CbxAnio.SelectedItem = Variables.indexComboboxAño;
                }
            }
            ActualizarSemanas();

            Variables.indexComboboxAño = CbxAnio.SelectedItem.ToString();
        }

        private void CbxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Agregar_tiempo tiempo = new Agregar_tiempo();
            // Limpiar semanas cada vez que cambie año o mes
            CbxSemana.ItemsSource = null;
            CbxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
            if (CbxAnio.SelectedItem is int anio && CbxMes.SelectedIndex >= 0)
            {
                int mes = CbxMes.SelectedIndex + 1;
                CbxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
            ActualizarSemanas();

            Variables.indexComboboxMes = CbxMes.SelectedItem.ToString();
        }

        private void CbxSemana_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Variables.Fecha = CbxSemana.SelectedItem?.ToString() ?? string.Empty;

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


                Variables.indexComboboxSemana = CbxSemana.SelectedItem.ToString();
            }
        }

        private void CbxDia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }




        private void CargarMeses()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            CbxMes.ItemsSource = tiempo.GetMeses();
            CbxMes.SelectedItem = DateTime.Now.Year;

        }
        private void CargarAnios()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            CbxAnio.ItemsSource = tiempo.GetAnios();
            CbxAnio.SelectedItem = DateTime.Now.Year;
        }
        void ActualizarSemanas()
        {
            Agregar_tiempo tiempo = new Agregar_tiempo();
            // Limpiar semanas cada vez que cambie año o mes
            CbxSemana.ItemsSource = null;
            CbxSemana.SelectedItem = null;

            // Verificar que tanto el año como el mes sean válidos
            if (CbxAnio.SelectedItem is int anio && CbxMes.SelectedIndex >= 0)
            {
                int mes = CbxMes.SelectedIndex + 1;
                CbxSemana.ItemsSource = tiempo.GetSemanasDelMes(anio, mes);
            }
        }

        private void CargarDiasDeSemana()
        {
            if (CbxSemana.SelectedItem != null)
            {
                Agregar_tiempo tiempo = new Agregar_tiempo();
                var resultado = tiempo.ObtenerFechasDesdeGlobal();

                if (resultado.exito)
                {
                    List<string> dias = new List<string>();
                    DateTime inicio = resultado.fechaInicio;
                    DateTime fin = resultado.fechaFin;

                    for (DateTime fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
                    {
                        dias.Add(fecha.ToString("dddd dd/MM/yyyy")); // Ej: "Lunes 17/06/2025"
                    }

                    CbxDia.ItemsSource = dias;
                    CbxDia.SelectedIndex = 0; // Opcional: selecciona el primer día
                }
                else
                {
                    MessageBox.Show("No se pudieron obtener las fechas de la semana.");
                }
            }
            else
            {
                CbxDia.ItemsSource = null;
            }
        }


        private void CbxAnio_DropDownOpened(object sender, EventArgs e)
        {
            CargarAnios();
        }
        private void CbxMes_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }
        private void CbxSemana_DropDownOpened(object sender, EventArgs e)
        {
            CargarMeses();
        }
        private void CbxDia_DropDownOpened(object sender, EventArgs e)
        {
            Variables.Fecha = CbxSemana.SelectedItem?.ToString() ?? string.Empty;

            Agregar_tiempo tiempo = new Agregar_tiempo();
            var resultado = tiempo.ObtenerFechasDesdeGlobal();

            if (resultado.exito)
            {
                DateTime inicio = resultado.fechaInicio;
                DateTime fin = resultado.fechaFin;

                Variables.FechaInicio = inicio.ToString("yyyy-MM-dd");
                Variables.FechaFin = fin.ToString("yyyy-MM-dd");

                Variables.indexComboboxSemana = CbxSemana.SelectedItem.ToString();

                // ✅ Agregar carga de días aquí
                CargarDiasDeSemana();
            }
        }


        private void CbxMultiplicador_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GuardarAsistencia()
        {
            try
            {
                // Validar que hay una fecha seleccionada
                if (CbxDia.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un día para registrar la asistencia.");
                    return;
                }

                // Validar que haya un admin o empleado seleccionado
                if (EmpleadoComboBox.SelectedItem == null && Admin2ComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un empleado o un administrador para registrar la asistencia.");
                    return;
                }

                // Parsear la fecha seleccionada
                string fechaStr = CbxDia.SelectedItem.ToString(); // Ej: "Lunes 17/06/2025"
                if (!DateTime.TryParse(fechaStr.Substring(fechaStr.IndexOf(' ') + 1), out DateTime fechaSeleccionada))
                {
                    MessageBox.Show("Fecha inválida seleccionada.");
                    return;
                }

                // Horas fijas
                TimeSpan horaEntrada = new TimeSpan(8, 0, 0);  // 08:00 AM
                TimeSpan horaSalida = new TimeSpan(18, 0, 0); // 06:00 PM

                var conexion = new Conexion();
                string cs = conexion.ObtenerCadenaConexion();
                using (var conn = new SqlConnection(cs))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    // --- 1) Obtener salario semanal y calcular salario del día ---
                    decimal salarioSemanal, salarioDelDia;
                    if (EmpleadoComboBox.SelectedItem != null)
                    {
                        int? idEmp = ObtenerIdEmpleado(EmpleadoComboBox.SelectedItem.ToString());
                        if (idEmp == null)
                        {
                            MessageBox.Show("No se pudo obtener el ID del empleado.");
                            return;
                        }

                        // Leer emp_semanal
                        cmd.CommandText = "SELECT emp_semanal FROM empleado WHERE id_empleado = @id";
                        cmd.Parameters.AddWithValue("@id", idEmp);
                        salarioSemanal = (decimal)cmd.ExecuteScalar();
                        cmd.Parameters.Clear();

                        salarioDelDia = salarioSemanal / 6m;

                        // Preparar INSERT
                        cmd.CommandText = @"
                    INSERT INTO asistencia
                      (asis_dia, asis_hora, asis_salida, id_empleado, salariofecha)
                    VALUES
                      (@fecha, @horaEntrada, @horaSalida, @id, @salario)";
                        cmd.Parameters.AddWithValue("@fecha", fechaSeleccionada.Date);
                        cmd.Parameters.AddWithValue("@horaEntrada", horaEntrada);
                        cmd.Parameters.AddWithValue("@horaSalida", horaSalida);
                        cmd.Parameters.AddWithValue("@id", idEmp);
                        cmd.Parameters.AddWithValue("@salario", salarioDelDia);
                    }
                    else // admin
                    {
                        string usuarioAdmin = Admin2ComboBox.SelectedItem.ToString();
                        var dBAdmins = new DB_admins();
                        int? idAdmin = dBAdmins.ObtenerIdPorUsuario(usuarioAdmin);
                        if (idAdmin == null)
                        {
                            MessageBox.Show("No se pudo obtener el ID del administrador.");
                            return;
                        }

                        // Leer admins_semanal
                        cmd.CommandText = "SELECT admins_semanal FROM admins WHERE id_admins = @id";
                        cmd.Parameters.AddWithValue("@id", idAdmin);
                        salarioSemanal = (decimal)cmd.ExecuteScalar();
                        cmd.Parameters.Clear();

                        salarioDelDia = salarioSemanal / 6m;

                        // Preparar INSERT
                        cmd.CommandText = @"
                    INSERT INTO asistencia
                      (asis_dia, asis_hora, asis_salida, admins_id_asistencia, salariofecha)
                    VALUES
                      (@fecha, @horaEntrada, @horaSalida, @id, @salario)";
                        cmd.Parameters.AddWithValue("@fecha", fechaSeleccionada.Date);
                        cmd.Parameters.AddWithValue("@horaEntrada", horaEntrada);
                        cmd.Parameters.AddWithValue("@horaSalida", horaSalida);
                        cmd.Parameters.AddWithValue("@id", idAdmin);
                        cmd.Parameters.AddWithValue("@salario", salarioDelDia);
                    }

                    // Ejecutar
                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0)
                    {
                        MessageBox.Show($"Asistencia registrada correctamente.\nSalario del día: {salarioDelDia:C}",
                                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo guardar la asistencia.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la asistencia: " + ex.Message);
            }
        }



        private void añadir_hora_extra()
        {
            try
            {
                if (CbxDia.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un día.");
                    return;
                }

                // Convertir fecha
                string fechaStr = CbxDia.SelectedItem.ToString();
                if (!DateTime.TryParse(fechaStr.Substring(fechaStr.IndexOf(' ') + 1), out DateTime fechaSeleccionada))
                {
                    MessageBox.Show("Fecha inválida seleccionada.");
                    return;
                }

                // Obtener cantidad de horas extra desde el UI
                if (!double.TryParse(TxtHorasExtra.Text, out double horasExtraCantidad) || horasExtraCantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida de horas extra.");
                    return;
                }

                // Obtener multiplicador desde ComboBox
                decimal multiplicador = 1; // Valor por defecto
                if (CbxMultiplicador.SelectedItem is ComboBoxItem selectedItem)
                {
                    if (!decimal.TryParse(selectedItem.Content.ToString(), out multiplicador))
                    {
                        MessageBox.Show("Multiplicador inválido.");
                        return;
                    }
                }

                TimeSpan horasExtra = TimeSpan.FromHours(horasExtraCantidad);

                Conexion conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    if (EmpleadoComboBox.SelectedItem != null)
                    {
                        int? idEmpleado = ObtenerIdEmpleado(EmpleadoComboBox.SelectedItem.ToString());
                        if (idEmpleado == null)
                        {
                            MessageBox.Show("No se pudo obtener el ID del empleado.");
                            return;
                        }

                        // Obtener sueldo base del empleado
                        cmd.CommandText = "SELECT emp_semanal FROM empleado WHERE id_empleado = @idEmp";
                        cmd.Parameters.AddWithValue("@idEmp", idEmpleado);
                        object sueldoObj = cmd.ExecuteScalar();
                        if (sueldoObj == null)
                        {
                            MessageBox.Show("No se encontró el sueldo del empleado.");
                            return;
                        }

                        decimal sueldoBase = Convert.ToDecimal(sueldoObj);
                        decimal salarioDelDia = sueldoBase / 6;
                        decimal pagaHoraExtra = (salarioDelDia / 8) * multiplicador;

                        cmd.Parameters.Clear();

                        cmd.CommandText = @"
                INSERT INTO asistencia (asis_dia, asis_hora_extra, id_empleado, paga_horaXT, salariofecha, multiplicador_hora)
                VALUES (@fecha, @horasExtra, @idEmpleado, @pagaExtra, @salario, @multiplicadorHora)";
                        cmd.Parameters.AddWithValue("@fecha", fechaSeleccionada.Date);
                        cmd.Parameters.AddWithValue("@horasExtra", horasExtra);
                        cmd.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                        cmd.Parameters.AddWithValue("@pagaExtra", pagaHoraExtra);
                        cmd.Parameters.AddWithValue("@salario", salarioDelDia);
                        cmd.Parameters.AddWithValue("@multiplicadorHora", multiplicador);
                    }
                    else if (Admin2ComboBox.SelectedItem != null)
                    {
                        DB_admins dB_Admins = new DB_admins();
                        int? idAdmin = dB_Admins.ObtenerIdPorUsuario(Admin2ComboBox.SelectedItem.ToString());
                        if (idAdmin == null)
                        {
                            MessageBox.Show("No se pudo obtener el ID del administrador.");
                            return;
                        }

                        // Obtener sueldo base del administrador
                        cmd.CommandText = "SELECT admins_semanal FROM admins WHERE id_admins = @idAd";
                        cmd.Parameters.AddWithValue("@idAd", idAdmin);
                        object sueldoObj = cmd.ExecuteScalar();
                        if (sueldoObj == null)
                        {
                            MessageBox.Show("No se encontró el sueldo del administrador.");
                            return;
                        }

                        decimal sueldoBase = Convert.ToDecimal(sueldoObj);
                        decimal salarioDelDia = sueldoBase / 6;
                        decimal pagaHoraExtra = (salarioDelDia / 8) * multiplicador;

                        cmd.Parameters.Clear();

                        cmd.CommandText = @"
                INSERT INTO asistencia (asis_dia, asis_hora_extra, admins_id_asistencia, paga_horaXT, salariofecha, multiplicador_hora)
                VALUES (@fecha, @horasExtra, @idAdmin, @pagaExtra, @salario, @multiplicadorHora)";
                        cmd.Parameters.AddWithValue("@fecha", fechaSeleccionada.Date);
                        cmd.Parameters.AddWithValue("@horasExtra", horasExtra);
                        cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                        cmd.Parameters.AddWithValue("@pagaExtra", pagaHoraExtra);
                        cmd.Parameters.AddWithValue("@salario", salarioDelDia);
                        cmd.Parameters.AddWithValue("@multiplicadorHora", multiplicador);
                    }
                    else
                    {
                        MessageBox.Show("Debe seleccionar un empleado o un administrador.");
                        return;
                    }

                    int resultado = cmd.ExecuteNonQuery();

                    if (resultado > 0)
                    {
                        MessageBox.Show("Horas extra registradas correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo guardar la asistencia.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la asistencia: " + ex.Message);
            }
        }


        private void BtnGuardarInasistencia_Click(object sender, RoutedEventArgs e)
        {
            GuardarAsistencia();
            MessageBox.Show("Justificación guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void BtnGuardarHoraExt_Click(object sender, RoutedEventArgs e)
        {
            añadir_hora_extra();
            MessageBox.Show("Hora extra guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

}
