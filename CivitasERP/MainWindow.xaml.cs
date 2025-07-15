using CivitasERP.AdminViews;
using CivitasERP.Views;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CivitasERP.Views.ForgotPasswordPage;
using CivitasERP.Models;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace CivitasERP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 1) Campos privados para cada ventana flotante
        private RegisterPage _registerPage = null;
        private NuevaObraPage _nuevaObraPage = null;

        // Field para llevar la cuenta del botón activo
        private Button _activeNavButton;

        public MainWindow()
        {
            try
            {
                // 1) Obtenemos la cadena de conexión
                var cs = new Conexion().ObtenerCadenaConexion();

                // 2) Probamos abrir la conexión
                using var conn = new SqlConnection(cs);
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al conectar");
            }


            InitializeComponent();
            MainFrame.Navigate(new HomePage());

            if (Variables.IdAdmin.HasValue)
                CargarFotoPerfil(Variables.IdAdmin.Value);


            //Ocultar botones de navegación según el tipo de usuario
            if (Variables.Jefe==false) 
            {
                btnJustificaciones.Visibility= Visibility.Collapsed;
                btnEmpleados.Visibility = Visibility.Collapsed;
                btnAdmins.Visibility = Visibility.Collapsed;
                btnRegis.Visibility = Visibility.Collapsed;
                //ComboBox de administradores en nomina y lista page esta en code behind de cada una
            }
            if (Variables.Jefe == false)
            {
                AdminDataAccess acceso = new AdminDataAccess();
                int idAdminBuscado = Variables.IdAdmin.Value;

                AdminInfo admin = acceso.ObtenerUsuarioYCategoriaPorId(idAdminBuscado);

                if (admin != null)
                {
                    LabelName.Text = admin.Usuario;
                    LabelRol.Text = admin.Categoria;
                }
                else
                {
                    Console.WriteLine("Admin no encontrado.");
                }
            }
            else {


                LabelName.Text = ObtenerJefePorUsuario(Variables.Usuario).Jefe_Usuario;
                LabelRol.Text = "Director";
            }
            // Cada vez que cambie el estado (Normal, Minimized, Maximized) problemas de forms abajo
            // NO ACTIVAR HASTA LEER *WINDOW STYLE FORMS*
            //this.StateChanged += MainWindow_StateChanged;
        }

        //Poder mover la ventana con libertad
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();

            //cuando esta maximizada, restaurarla al tamaño normal al arrastrar
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);

            //Maximo de ventana 
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }


        //WINDOW STYLE FORMS
        // Manejo del estado de la ventana para maximizar correctamente USANDO WINDOWS FORMS (problemas "ambiguous reference")
        // al usar forms y wpf genera conflicto entre ambos y por eso ocaciona la ambiguiedad
        ///habilitar FORMS en <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
        ///
        /*
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // 1) Averigua el HWND de esta ventana
                var hwnd = new WindowInteropHelper(this).Handle;
                // 2) Determina la pantalla (monitor) donde está
                var screen = Screen.FromHandle(hwnd);
                // 3) Toma su área de trabajo (excluye barra de tareas)
                var wa = screen.WorkingArea;

                // 4) Ajusta el tamaño y la posición
                this.Top = wa.Top;
                this.Left = wa.Left;
                this.Width = wa.Width;
                this.Height = wa.Height;
            }
        }*/




        //CAMBIO DE FOTO DE PERFIL
        /// Manejador que se activa cuando el usuario hace clic en el Ellipse de perfil.
        /// Abre un OpenFileDialog para que elija una imagen, y luego la asigna como Fill del Ellipse.
        /// <summary>
        /// Carga la foto de perfil desde la BD y la pinta en el Ellipse.
        /// </summary>
        public void CargarFotoPerfil(int idAdmin)
        {
            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            const string sql = @"
            SELECT FotoPerfil 
              FROM admins 
             WHERE id_admins = @id";
            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAdmin;
                cn.Open();
                var result = cmd.ExecuteScalar();
                if (result is byte[] fotoBytes && fotoBytes.Length > 0)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(fotoBytes);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    EllipseProfile.Fill = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    ImgPlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Opcional: pinta un placeholder si no hay foto
                    EllipseProfile.Fill = Brushes.LightGray;
                    ImgPlaceholder.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Guarda el array de bytes en la BD para el id de admin actual.
        /// </summary>
        public void GuardarFotoPerfil(byte[] fotoBytes, int idAdmin)
        {
            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            const string sql = @"
            UPDATE admins
               SET FotoPerfil = @foto
             WHERE id_admins = @id";
            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@foto", SqlDbType.VarBinary, fotoBytes.Length).Value = fotoBytes;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAdmin;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // JEFE
        
        public void CargarFotoPerfilJefe(int idAdmin)
        {
            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            const string sql = @"
            SELECT FotoPerfilJefe
              FROM Jefe 
             WHERE id_Jefe = @id";
            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAdmin;
                cn.Open();
                var result = cmd.ExecuteScalar();
                if (result is byte[] fotoBytes && fotoBytes.Length > 0)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(fotoBytes);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    EllipseProfile.Fill = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    ImgPlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Opcional: pinta un placeholder si no hay foto
                    EllipseProfile.Fill = Brushes.LightGray;
                    ImgPlaceholder.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Guarda el array de bytes en la BD para el id de admin actual.
        /// </summary>
        public void GuardarFotoPerfilJefe(byte[] fotoBytes, int idAdmin)
        {
            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            const string sql = @"
            UPDATE Jefe
               SET FotoPerfilJefe = @foto
             WHERE id_Jefe = @id";
            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@foto", SqlDbType.VarBinary, fotoBytes.Length).Value = fotoBytes;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAdmin;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        
        private void EllipseProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Variables.Jefe == false) { 

            var conexion = new Conexion();
            string connectionString = conexion.ObtenerCadenaConexion();
            var dlg = new OpenFileDialog
            {
                Title = "Selecciona una foto de perfil",
                Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            };
            if (dlg.ShowDialog() != true) return;

            var ruta = dlg.FileName;
            byte[] imagenBytes = File.ReadAllBytes(ruta);

            // 1) Mostrarla inmediatamente
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(imagenBytes);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            EllipseProfile.Fill = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            ImgPlaceholder.Visibility = Visibility.Collapsed;


            // 2) Guardarla en la BD
            const string sql = @"
                              UPDATE admins 
                                 SET FotoPerfil = @foto 
                               WHERE id_admins = @id";
            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@foto", SqlDbType.VarBinary, imagenBytes.Length)
                   .Value = imagenBytes;
                cmd.Parameters.Add("@id", SqlDbType.Int)
                   .Value = Variables.IdAdmin;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
               
        }

            if (Variables.Jefe == true)
            {
                var conexion = new Conexion();
                string connectionString = conexion.ObtenerCadenaConexion();
                var dlg = new OpenFileDialog
                {
                    Title = "Selecciona una foto de perfil",
                    Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                };
                if (dlg.ShowDialog() != true) return;

                var ruta = dlg.FileName;
                byte[] imagenBytes = File.ReadAllBytes(ruta);

                // 1) Mostrarla inmediatamente
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(imagenBytes);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                EllipseProfile.Fill = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };

                // 2) Guardarla en la BD
                const string sql = @"
                              UPDATE Jefe 
                                 SET FotoPerfilJefe = @foto 
                               WHERE id_Jefe = @id";
                using (var cn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@foto", SqlDbType.VarBinary, imagenBytes.Length)
                       .Value = imagenBytes;
                    cmd.Parameters.Add("@id", SqlDbType.Int)
                       .Value = Variables.IdAdmin;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

            }
        }


        //BOTONES DE NAVEGACIÓN
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null);

            // Navegacion de botones Generales
            switch (btn.Name)
            {
                //Botones de navegación del menú lateral
                case "btnMenu":
                    MainFrame.Navigate(new HomePage());
                    break;
                case "btnNomina":
                    MainFrame.Navigate(new NominaPage());     
                    break;
                case "btnLista":
                    MainFrame.Navigate(new ListaPage());
                    break;
                case "btnLogout":
                    var loginWindow = new LoginPage();
                    loginWindow.Show();
                    this.Close();
                    break;
                case "btnJustificaciones":
                    MainFrame.Navigate(new JustificacionesPage());
                    break;
                case "btnEmpleados":
                    MainFrame.Navigate(new EditEmpleadoPage());
                    break;
                case "btnAdmins":
                    MainFrame.Navigate(new EditAdminPage());
                    break;
                case "btnConfiguracion":
                    MainFrame.Navigate(new ConfigurationPage());
                    break;


            }
            // Resaltar el botón activo
            HighlightNavButton(btn);
        }

        //Resaltar el botón activo
        private void HighlightNavButton(Button btnToActivate)
        {
            // Lista de todos los botones de menú
            var navButtons = new[] { btnMenu, btnNomina, btnLista, btnJustificaciones, btnEmpleados, btnAdmins, btnConfiguracion };

            // Rescata los brushes de los recursos
            var defaultBrush = (Brush)FindResource("buttonColor2"); // #4772E3
            var activeBrush = (Brush)FindResource("buttonColor1"); // #274288

            // Reset a todos
            foreach (var b in navButtons)
                b.Background = defaultBrush;

            // Activa el que tocamos
            btnToActivate.Background = activeBrush;

            // Guarda la referencia
            _activeNavButton = btnToActivate;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            /// ventana flotante
            /// Si no existe o ya se cerró (_registerPage == null), creamos la ventana:
            if (_registerPage == null)
            {
                _registerPage = new RegisterPage();
                // Cuando se cierre por completo, dejamos la referencia en null:
                _registerPage.Closed += (s, args) =>
                {
                    _registerPage = null;
                };
                _registerPage.ShowDialog();
            }
            else
            {
                _registerPage.Activate();
            }
        }
        private void btnNuevaObra_Click(object sender, RoutedEventArgs e)
        {
            /// ventana flotante
            /// Si no existe o ya se cerró (_nuevaObraWindow == null), creamos la ventana:
            if (_nuevaObraPage == null)
            {
                _nuevaObraPage = new NuevaObraPage();
                // Cuando se cierre por completo, dejamos la referencia en null:
                _nuevaObraPage.Closed += (s, args) =>
                {
                    _nuevaObraPage = null;
                };
                _nuevaObraPage.ShowDialog();
            }
            else
            {
                _nuevaObraPage.Activate();
            }
        }
        private void TopBar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch(btn.Name)
            {
                //Botones de la barra superior
                case "btnExit":
                    Application.Current.Shutdown();
                    break;
                case "btnMin":
                    WindowState = WindowState.Minimized;
                    break;
                case "btnMax":
                    WindowState = (WindowState == WindowState.Normal)
                        ? WindowState.Maximized : WindowState.Normal;
                    break;
            }

        }

        private void EllipseProfile_DpiChanged(object sender, DpiChangedEventArgs e)
        {

        }

        public class AdminInfo
        {
            public string Usuario { get; set; }
            public string Categoria { get; set; }
        }

        public class AdminDataAccess
        {
            
            
            public AdminInfo ObtenerUsuarioYCategoriaPorId(int idAdmin)
            {
                var cs = new Conexion().ObtenerCadenaConexion();
                AdminInfo admin = null;

                string query = "SELECT admins_usuario, admin_categoria FROM admins WHERE id_admins = @IdAdmin";

                using (SqlConnection connection = new SqlConnection(cs))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdAdmin", idAdmin);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            admin = new AdminInfo
                            {
                                Usuario = reader["admins_usuario"].ToString(),
                                Categoria = reader["admin_categoria"].ToString()
                            };
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al obtener los datos del admin: " + ex.Message);
                    }
                }

                return admin;
            }
        }
        public class Jefe
        {
            public int Id_Jefe { get; set; }
            public string Jefe_Usuario { get; set; }
        }
        public Jefe ObtenerJefePorUsuario(string usuario)
        {
            Jefe jefe = null;
            var cs = new Conexion().ObtenerCadenaConexion();
            using (SqlConnection conn = new SqlConnection(cs))
            {
                string query = "SELECT id_Jefe, Jefe_usuario FROM Jefe WHERE Jefe_usuario = @Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Usuario", usuario);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    jefe = new Jefe
                    {
                        Id_Jefe = reader.GetInt32(0),
                        Jefe_Usuario = reader.GetString(1)
                    };
                }
            }

            return jefe;
        }
    }
}
