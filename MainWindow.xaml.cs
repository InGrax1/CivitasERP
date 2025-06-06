using CivitasERP.Views;
using Microsoft.Win32;
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


        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new HomePage());
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
        private void EllipseProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("¡Click en el ellipse!");

            // 1) Abrir diálogo para seleccionar imagen
            var dlg = new OpenFileDialog
            {
                Title = "Selecciona una foto de perfil",
                Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            bool? resultado = dlg.ShowDialog();
            if (resultado == true)
            {
                string rutaImagen = dlg.FileName;
                try
                {
                    // 2) Cargar la imagen en un BitmapImage
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(rutaImagen, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // 3) Crear un ImageBrush con esa imagen
                    var brush = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };

                    // 4) Asignar el ImageBrush al Fill del Ellipse
                    EllipseProfile.Fill = brush;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"No se pudo cargar la imagen:\n{ex.Message}",
                        "Error al cargar imagen",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        //BOTONES DE NAVEGACIÓN
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal)
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        private void btnNomina_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NominaPage());
        }
        private void btnLista_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ListaPage());
        }
        private void btnNueva_Obra(object sebder, RoutedEventArgs e)
        {
            NuevaObraPage nuevaObraPage = new NuevaObraPage();
            nuevaObraPage.Show();
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // 1) Instanciamos la ventana de Login (ajusta el nombre si tu clase se llama distinto)
            var loginWindow = new LoginPage();

            // 2) Mostramos Login en modo no modal (para que la app siga viva)
            loginWindow.Show();

            // 3) Cerramos la ventana actual (dejará viva únicamente la de Login)
            this.Close();
        }
        private void btnRegis_Click(object sender, RoutedEventArgs e)
        {
            // Si no existe o ya se cerró (_registerPage == null), creamos la ventana:
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
            // Si no existe o ya se cerró (_nuevaObraWindow == null), creamos la ventana:
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
    }
}