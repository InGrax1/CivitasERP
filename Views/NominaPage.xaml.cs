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

        private void ComBoxMes_DropDownOpened(object sender, EventArgs e)
        {
           // LlenarComboSemanasMesActual(ComBoxSemanaOp);
        }

        private void ComBoxMes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (ComBoxSemanaOp.SelectedItem != null)
            {
                string seleccion = ComBoxSemanaOp.SelectedItem.ToString();

                // Separar las fechas
                string[] fechas = seleccion.Split('-');
                if (fechas.Length == 2)
                {
                    string fechaInicio = fechas[0].Trim();
                    string fechaFin = fechas[1].Trim();

                    Fecha1.Text = fechaInicio;
                    Fecha2.Text = fechaFin;
                    
                }
            }*/
        }
        private void LlenarComboSemanasMesActual(ComboBox comboBox)
        {/*
            comboBox.Items.Clear();

            int mes = DateTime.Now.Month;
            int anio = DateTime.Now.Year;

            DateTime inicioMes = new DateTime(anio, mes, 1);
            DateTime finMes = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            DateTime inicioSemana = inicioMes;

            while (inicioSemana <= finMes)
            {
                DateTime finSemana = inicioSemana.AddDays(6);
                if (finSemana > finMes)
                    finSemana = finMes;

                string rango = $"{inicioSemana:dd/MM/yyyy} - {finSemana:dd/MM/yyyy}";
                comboBox.Items.Add(rango);

                inicioSemana = finSemana.AddDays(1);
            }

            // Selecciona la primera semana si quieres
            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;*/
        }

        private void ComBoxAnio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ComBoxAnio_DropDownOpened(object sender, EventArgs e)
        {
            // LlenarComboAnios(ComBoxAnio);
        }
    }
    
}
