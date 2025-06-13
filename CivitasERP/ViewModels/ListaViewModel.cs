using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CivitasERP.Models;
using BiometriaDP.Services;
using static CivitasERP.Models.Datagrid_lista;
using static CivitasERP.Views.LoginPage;
using CivitasERP.Views;
using static CivitasERP.Models.DataGridNominas;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;


namespace CivitasERP.ViewModels
{
    /// <summary>
    /// Inicializa la conexión y el repositorio (_repo) para cargar datos.
    /// Crea el FingerprintService y se suscribe a OnVerificationComplete y OnError.
    /// Expone una colección Empleados ligada al DataGrid y una propiedad EstadoHuella para el texto de estado.
    /// Define ScanCommand que dispara StartScan(), el cual arranca la verificación biométrica.
    /// En OnVerified marca la asistencia (admin o empleado) usando _repo y recarga la lista.
    /// En OnError actualiza el estado y muestra un mensaje.
    /// RelayCommand para enlazar el botón de huella con esta lógica.
    public class ListaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Raise(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private readonly string _cs;
        private readonly Datagrid_lista _repo;
        private readonly FingerprintService _fingerService;

        public ObservableCollection<Empleado_Asistencia> Empleados { get; }
            = new ObservableCollection<Empleado_Asistencia>();

        private string _estadoHuella;
        public string EstadoHuella
        {
            get => _estadoHuella;
            set { _estadoHuella = value; Raise(nameof(EstadoHuella)); }
        }

        public ICommand ScanCommand { get; }

        public ListaViewModel()
        {
            _cs = new Conexion().ObtenerCadenaConexion();
            _repo = new Datagrid_lista(_cs);

            int loggedAdmin = Variables.IdAdmin
                ?? throw new InvalidOperationException("No hay admin logueado");
            _fingerService = new FingerprintService(_cs, loggedAdmin); _fingerService.OnVerificationComplete += OnVerified;

            _fingerService.OnError += OnError;

            ScanCommand = new RelayCommand(_ => StartScan());
            if (Variables.FechaInicio == null || Variables.FechaFin == null)
            {
                
            }else
            {
                LoadEmpleados();
            }
        }

        private void LoadEmpleados()
        {
            Empleados.Clear();
            foreach (var e in _repo.ObtenerEmpleados())
                Empleados.Add(e);
        }

        private void StartScan()
        {
            EstadoHuella = "⏳ Coloca tu dedo…";
            _fingerService.StartVerification();
        }

        private void OnError(object s, string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EstadoHuella = $"❌ {msg}";
                MessageBox.Show(msg, "Error huella", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnVerified(object s, FingerprintVerifiedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.IdUsuario < 0)
                {
                    EstadoHuella = "❌ Huella no reconocida.";
                    MessageBox.Show("Huella no coincide con ningún usuario.", "Verificación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (e.IsAdmin)
                    _repo.MarcarAsistenciaAdmin(e.IdUsuario);
                else
                    _repo.MarcarAsistencia(e.IdUsuario);

                EstadoHuella = e.IsAdmin
                    ? $"✔ Asistencia admin #{e.IdUsuario} registrada."
                    : $"✔ Asistencia empleado #{e.IdUsuario} registrada.";

                LoadEmpleados();
                MessageBox.Show("Asistencia registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    //para enlazar el botón de huella con esta lógica.
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _exec;
        public RelayCommand(Action<object> exec) => _exec = exec;
        public bool CanExecute(object p) => true;
        public void Execute(object p) => _exec(p);
        public event EventHandler CanExecuteChanged;
    }

}
