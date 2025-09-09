using CivitasERP.Models;
using CivitasERP.Services;
using Microsoft.Extensions.Configuration;
using NuGet.Versioning;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using Velopack;


namespace CivitasERP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UpdateService _updateService;
        private IConfiguration _configuration;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // CRÍTICO: Esto debe ser lo primero que se ejecute
            // Inicializar Velopack antes que cualquier otra cosa
            UpdateService.Initialize();

            base.OnStartup(e);

            try
            {
                // Configurar el sistema de configuración
                SetupConfiguration();

                // Inicializar servicio de actualizaciones
                _updateService = new UpdateService(_configuration);

                // Verificar actualizaciones automáticamente después de que la app esté completamente cargada
                // Lo hacemos con un delay para no interferir con el startup
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Esperar 15 segundos después del inicio para verificar actualizaciones
                        await Task.Delay(15000);

                        // Verificación silenciosa (no molesta al usuario al inicio)
                        await _updateService.CheckForUpdatesAsync(silent: true);
                    }
                    catch (Exception ex)
                    {
                        // Log error pero no interrumpir la aplicación
                        Console.WriteLine($"Error en verificación automática de actualizaciones: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Log error pero permitir que la aplicación continúe
                Console.WriteLine($"Error en OnStartup: {ex.Message}");

                // Mostrar mensaje de error solo si es crítico
                MessageBox.Show($"Error al inicializar la aplicación:\n{ex.Message}",
                    "Error - CivitasERP", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Configura el sistema de configuración para leer appsettings.json
        /// </summary>
        private void SetupConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<App>(optional: true);

                _configuration = builder.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando Configuration: {ex.Message}");
                // Crear configuración vacía para evitar errores
                _configuration = new ConfigurationBuilder().Build();
            }
        }

        /// <summary>
        /// Obtiene el entorno actual (Development, Production, etc.)
        /// </summary>
        private string GetEnvironment()
        {
            return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                   Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                   "Production";
        }

        /// <summary>
        /// Método público para verificar actualizaciones manualmente
        /// Puede ser llamado desde menús o botones
        /// </summary>
        public async Task CheckForUpdatesManuallyAsync()
        {
            try
            {
                if (_updateService != null)
                {
                    await _updateService.CheckForUpdatesAsync(silent: false);
                }
                else
                {
                    MessageBox.Show("El servicio de actualizaciones no está disponible.",
                        "CivitasERP", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar actualizaciones:\n{ex.Message}",
                    "Error - CivitasERP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Obtiene la versión actual de la aplicación
        /// </summary>
        public string GetCurrentVersion()
        {
            try
            {
                return _updateService?.GetCurrentVersion() ?? "Desconocida";
            }
            catch
            {
                return "Desconocida";
            }
        }

        /// <summary>
        /// Verifica si el sistema de actualizaciones está disponible
        /// </summary>
        public bool IsUpdateServiceAvailable()
        {
            return _updateService?.IsUpdateServiceAvailable() ?? false;
        }

        /// <summary>
        /// Obtiene la configuración de la aplicación
        /// </summary>
        public IConfiguration GetConfiguration()
        {
            return _configuration;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Limpiar recursos
                _updateService?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnExit: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }

    }
}
