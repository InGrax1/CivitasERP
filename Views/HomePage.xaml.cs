    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
using System.Windows.Interop;
using Microsoft.Win32;


namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        private RegisterPage _registerPage;
        public HomePage()
        {
            InitializeComponent();
            this.Loaded += HomePage_Loaded;

        }



        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Cambio de tamaño de la ventana
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.MinWidth = 800;
                this.MinHeight = 600;
                // Máximo al área útil de la pantalla:
                this.MaxWidth = SystemParameters.WorkArea.Width;
                this.MaxHeight = SystemParameters.WorkArea.Height;
            }

            // Verificamos si quedó fuera del área útil y la centramos:
            var wa = SystemParameters.WorkArea;
            if (this.Left < wa.Left ||
                this.Top < wa.Top ||
                this.Left + this.ActualWidth > wa.Right ||
                this.Top + this.ActualHeight > wa.Bottom)
            {
                this.Left = wa.Left + (wa.Width - this.ActualWidth) / 2;
                this.Top = wa.Top + (wa.Height - this.ActualHeight) / 2;
            }
        }


        //CAMBIO DE FOTO DE PERFIL
        /// Manejador que se activa cuando el usuario hace clic en el Ellipse de perfil.
        /// Abre un OpenFileDialog para que elija una imagen, y luego la asigna como Fill del Ellipse.
        /// 

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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                       ? WindowState.Normal
                       : WindowState.Maximized;
        }

        private void btnNomina_Click(object sender, RoutedEventArgs e)
        {
            NominaPage nominaPage = new NominaPage();
            nominaPage.Show();
            this.Close();
        }
        private void btnLista_Click(object sender, RoutedEventArgs e)
        {
            ListaPage listaPage = new ListaPage();
            listaPage.Show();
            this.Close();
        }
        private void btnNueva_Obra(object sebder, RoutedEventArgs e)
        {
            NuevaObraPage nuevaObraPage = new NuevaObraPage();
            nuevaObraPage.Show();
            this.Close();
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
            // ¿Ya hay una instancia abierta?
            if (_registerPage == null)
            {
                // 1) Creamos la ventana (solo si no existe o ya se cerró)
                _registerPage = new RegisterPage();

                // 2) Subscribimos al evento Closed: cuando se cierre, ponemos _registerWindow = null
                _registerPage.Closed += (s, args) =>
                {
                    _registerPage = null;
                };

                // 3) Mostramos la ventana (puedes usar ShowDialog() si quieres modal,
                //    o Show() si quieres que el usuario pueda seguir interactuando con la principal)
                _registerPage.Show();
            }
            else
            {
                // Si ya está abierta, solo la activamos (la ponemos al frente)
                _registerPage.Activate();
            }
        }

        
    }
}
