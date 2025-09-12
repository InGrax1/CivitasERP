
using System;
using System.Threading.Tasks;
using System.Windows;
using Velopack;

namespace CivitasERP.Services
{
    public class UpdateService
    {
        private static async Task ActualizarMiAplicacion()
        {
            var mgr = new UpdateManager("https://mi-servidor/actualizaciones");

            // Verificar si hay una nueva versión
            var nuevaVersion = await mgr.CheckForUpdatesAsync();
            if (nuevaVersion == null)
                return; // No hay actualizaciones disponibles

            // Descargar la nueva versión
            await mgr.DownloadUpdatesAsync(nuevaVersion);

            // Aplicar la actualización e iniciar la aplicación
            mgr.ApplyUpdatesAndRestart(nuevaVersion);
        }
    }
}
