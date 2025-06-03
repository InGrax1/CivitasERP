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
using DPFP;
using DPFP.Capture;
using DPFP.Processing;


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
            

        }
    }
}
