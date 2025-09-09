using Velopack;
using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CivitasERP.Services
{
    public class UpdateService : IDisposable
    {
        private readonly UpdateManager _updateManager;
        private readonly string _updateUrl;
        private readonly IConfiguration _configuration;

        public UpdateService(IConfiguration configuration = null)
        {
            _configuration = configuration;

            // Obtener URL de actualizaciones desde configuración o usar default
            _updateUrl = _configuration?["UpdateService:UpdateUrl"] ?? "https://tu-servidor.com/releases";

            try
            {
                _updateManager = new UpdateManager(_updateUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando UpdateManager: {ex.Message}");
                _updateManager = null;
            }
        }

        /// <summary>
        /// Verifica si hay actualizaciones disponibles
        /// </summary>
        /// <param name="silent">Si es true, no muestra mensajes al usuario</param>
        public async Task CheckForUpdatesAsync(bool silent = false)
        {
            if (_updateManager == null)
            {
                if (!silent)
                {
                    MessageBox.Show("El sistema de actualizaciones no está disponible.",
                        "CivitasERP", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            try
            {
                var newVersion = await _updateManager.CheckForUpdatesAsync();

                if (newVersion == null)
                {
                    if (!silent)
                    {
                        MessageBox.Show("No hay actualizaciones disponibles.\n\n" +
                            $"Versión actual: {GetCurrentVersion()}",
                            "CivitasERP - Actualizado",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                // Solo mostrar diálogo si no es silencioso
                bool shouldUpdate = silent;

                if (!silent)
                {
                    var result = MessageBox.Show(
                        $"🎉 ¡Nueva versión disponible!\n\n" +
                        $"Versión actual: {GetCurrentVersion()}\n" +
                        $"Nueva versión: {newVersion.TargetFullRelease.Version}\n\n" +
                        "La actualización se descargará e instalará automáticamente.\n" +
                        "¿Deseas actualizar ahora?",
                        "Actualización disponible - CivitasERP",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    shouldUpdate = result == MessageBoxResult.Yes;
                }

                if (shouldUpdate)
                {
                    await DownloadAndApplyUpdatesAsync(newVersion, silent);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;

                // Mensajes más amigables para errores comunes
                if (errorMessage.Contains("404") || errorMessage.Contains("Not Found"))
                {
                    errorMessage = "No se pudo conectar con el servidor de actualizaciones.\nVerifique su conexión a internet.";
                }
                else if (errorMessage.Contains("timeout") || errorMessage.Contains("timed out"))
                {
                    errorMessage = "Tiempo de espera agotado al verificar actualizaciones.\nIntente más tarde.";
                }

                if (!silent)
                {
                    MessageBox.Show($"Error al verificar actualizaciones:\n\n{errorMessage}",
                        "Error - CivitasERP", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Log del error completo para debugging
                Console.WriteLine($"Error completo en CheckForUpdatesAsync: {ex}");
            }
        }

        /// <summary>
        /// Descarga y aplica las actualizaciones
        /// </summary>
        private async Task DownloadAndApplyUpdatesAsync(UpdateInfo updateInfo, bool silent)
        {
            UpdateProgressWindow progressWindow = null;

            try
            {
                if (!silent)
                {
                    progressWindow = new UpdateProgressWindow();
                    progressWindow.Show();
                }

                // Descargar la actualización con callback de progreso
                await _updateManager.DownloadUpdatesAsync(updateInfo, progress =>
                {
                    if (progressWindow != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progressWindow.UpdateDownloadProgress(progress);
                        });
                    }
                });

                if (progressWindow != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.UpdateStatus("Preparando actualización...");
                    });
                }

                // Aplicar la actualización y reiniciar
                _updateManager.ApplyUpdatesAndRestart(updateInfo);
            }
            catch (Exception ex)
            {
                progressWindow?.Close();

                var errorMessage = ex.Message;
                if (errorMessage.Contains("403") || errorMessage.Contains("Forbidden"))
                {
                    errorMessage = "No tiene permisos para descargar la actualización.";
                }
                else if (errorMessage.Contains("disk") || errorMessage.Contains("space"))
                {
                    errorMessage = "No hay suficiente espacio en disco para la actualización.";
                }

                if (!silent)
                {
                    MessageBox.Show($"Error durante la actualización:\n\n{errorMessage}\n\n" +
                        "La aplicación continuará funcionando con la versión actual.",
                        "Error - CivitasERP", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Console.WriteLine($"Error completo en DownloadAndApplyUpdatesAsync: {ex}");
            }
        }

        /// <summary>
        /// Inicializa Velopack - debe llamarse al inicio de la aplicación
        /// </summary>
        public static void Initialize()
        {
            try
            {
                VelopackApp.Build().Run();
            }
            catch (Exception ex)
            {
                // Log pero no interrumpir el inicio de la aplicación
                Console.WriteLine($"Error inicializando Velopack: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la versión actual de la aplicación
        /// </summary>
        public string GetCurrentVersion()
        {
            try
            {
                return _updateManager?.CurrentVersion?.ToString() ??
                       System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ??
                       "Desconocida";
            }
            catch
            {
                return "Desconocida";
            }
        }

        /// <summary>
        /// Verifica si el sistema de actualizaciones está habilitado
        /// </summary>
        public bool IsUpdateServiceAvailable()
        {
            return _updateManager != null && !string.IsNullOrEmpty(_updateUrl);
        }

        public void Dispose()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al dispose UpdateService: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Ventana de progreso para mostrar el avance de las actualizaciones
    /// </summary>
    public partial class UpdateProgressWindow : Window
    {
        private System.Windows.Controls.ProgressBar _progressBar;
        private System.Windows.Controls.Label _statusLabel;
        private System.Windows.Controls.Label _percentageLabel;

        public UpdateProgressWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "Actualizando CivitasERP";
            Width = 500;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            Topmost = true;
            ShowInTaskbar = false;

            // Grid principal
            var mainGrid = new System.Windows.Controls.Grid();
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            // Título
            var titleLabel = new System.Windows.Controls.Label
            {
                Content = "🔄 CivitasERP - Actualización en progreso",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 15, 10, 10),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue)
            };
            System.Windows.Controls.Grid.SetRow(titleLabel, 0);

            // Etiqueta de estado
            _statusLabel = new System.Windows.Controls.Label
            {
                Content = "Preparando descarga...",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 5),
                FontSize = 12
            };
            System.Windows.Controls.Grid.SetRow(_statusLabel, 1);

            // Barra de progreso
            _progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 25,
                Margin = new Thickness(30, 0, 30, 10),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            System.Windows.Controls.Grid.SetRow(_progressBar, 2);

            // Etiqueta de porcentaje
            _percentageLabel = new System.Windows.Controls.Label
            {
                Content = "0%",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 15),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            System.Windows.Controls.Grid.SetRow(_percentageLabel, 3);

            mainGrid.Children.Add(titleLabel);
            mainGrid.Children.Add(_statusLabel);
            mainGrid.Children.Add(_progressBar);
            mainGrid.Children.Add(_percentageLabel);

            Content = mainGrid;

            // Centrar en pantalla al mostrar
            Loaded += (s, e) => {
                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
            };
        }

        public void UpdateDownloadProgress(int percentage)
        {
            Dispatcher.Invoke(() =>
            {
                _progressBar.Value = percentage;
                _percentageLabel.Content = $"{percentage}%";
                _statusLabel.Content = $"Descargando actualización... {percentage}%";
            });
        }

        public void UpdateStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {
                _statusLabel.Content = status;
                _progressBar.IsIndeterminate = true;
                _percentageLabel.Content = "";
            });
        }
    }
}