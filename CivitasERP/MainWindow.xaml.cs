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
            InitializeComponent();
            MainFrame.Navigate(new HomePage());

            if (Variables.IdAdmin.HasValue)
                CargarFotoPerfil(Variables.IdAdmin.Value);

        }

        //Poder mover la ventana con libertad
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }



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
                }
                else
                {
                    // Opcional: pinta un placeholder si no hay foto
                    EllipseProfile.Fill = Brushes.LightGray;
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
                }
                else
                {
                    // Opcional: pinta un placeholder si no hay foto
                    EllipseProfile.Fill = Brushes.LightGray;
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
                    /// 1) Instanciamos la ventana de Login
                    var loginWindow = new LoginPage();

                    /// 2) Mostramos Login en modo no modal (para que la app siga viva)
                    loginWindow.Show();

                    /// 3) Cerramos la ventana actual (dejará viva únicamente la de Login)
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
             
            }
            // Resaltar el botón activo
            HighlightNavButton(btn);
        }

        private void btnpruea_Click(object sender, RoutedEventArgs e)
        {
            prueba prueba = new prueba();
            prueba.ShowDialog();
        }
        //Resaltar el botón activo
        private void HighlightNavButton(Button btnToActivate)
        {
            // Lista de todos los botones de menú
            var navButtons = new[] { btnMenu, btnNomina, btnLista };

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
        
    }
}