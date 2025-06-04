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
using CivitasERP.Models;
using CivitasERP.Views;
using CivitasERP.ViewModels;
using static CivitasERP.Views.LoginPage;
using System.Data.SqlClient;
using System.Reflection;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para NuevoEmpleadoPage.xaml
    /// </summary>
    public partial class NuevoEmpleadoPage : Window
    {

        public NuevoEmpleadoPage()
        {
            InitializeComponent();
          
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnHuellaR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de huella no implementada aún.");
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {

            string Nombre="", apellidop = "", apellidom = "", categoria = "", usuario = "", obra = "";
            int? Id_admin=0 ,id_obra=0;
            decimal sueldo=0;
            bool exito;
            Nombre = txtNombre.Text;
            apellidop = txtApellidoPaterno.Text;
            apellidom = txtApellidoMaterno.Text;
            categoria = txtCategoria.Text;
            exito = decimal.TryParse(txtSueldo.Text, out sueldo);

            

            DB_admins id_ADMIN = new DB_admins();
            usuario = GlobalVariables1.usuario;
            Id_admin = id_ADMIN.ObtenerIdPorUsuario(usuario);
            obra = cmbObra.Text;
            id_obra = ObtenerID_obra(Id_admin, obra);

            Insert_Empleado insert_Empleado = new Insert_Empleado()
            {
                Nombre = Nombre,
                ApellidoP = apellidop,
                ApellidoM = apellidom,
                Paga_semanal = sueldo,
                Paga_diaria = sueldo / 6,
                Paga_Hora_Extra = (sueldo / 6) / 8,
                Obra = obra,
                id_admin = Id_admin,
                id_obra = id_obra,
                categoria = categoria
            };
            insert_Empleado.InsertEmpleado();
            MessageBox.Show("Empleado Registrado Con Exito");
        }

        private void cmbObra_DropDownOpened(object sender, EventArgs e)
        {
            CargarDatosComboBox();
        }
        private void CargarDatosComboBox()
        {
            string usuario;
            usuario =GlobalVariables1.usuario;
            DB_admins dB_Admins = new DB_admins();
            int? idAdminObra;
            idAdminObra = dB_Admins.ObtenerIdPorUsuario(usuario);

            try
            {
                Conexion Sconexion = new Conexion();
                string connectionString;

                string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
                connectionString = obtenerCadenaConexion;


                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT obra_nombre FROM obra WHERE id_admin_obra = @idAdminObra";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbObra.Items.Clear();

                    while (reader.Read())
                    {
                        cmbObra.Items.Add(reader["obra_nombre"].ToString());
                    }

                    reader.Close();
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

            Conexion Sconexion = new Conexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT id_obra FROM obra WHERE id_admin_obra = @idAdminObra AND obra_nombre = @obraNombre";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);
                cmd.Parameters.AddWithValue("@obraNombre", obraNombre);

                conn.Open();
                object resultado = cmd.ExecuteScalar();

                if (resultado != null && int.TryParse(resultado.ToString(), out int idObra))
                {
                    return idObra;
                }

                return null; // No se encontró coincidencia
            }
        }
    }
}
