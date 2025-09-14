using System;
using CivitasERP.Conexiones;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;


/* * Clase para manejar la conexión a la base de datos.
 * Clase ejemplo para el arcvhivo de Conexiones.cs, copiar y pegar codgio solamente
 * Permite cambiar la configuración según el entorno (desarrollo, producción, etc.).
 * Asegura que la cadena de conexión esté disponible y válida.
 */
namespace CivitasERP.ConexionesExample  
{
    public class DBConexion
    {
        private static readonly IConfigurationRoot _config;

        static DBConexion()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "Conexiones"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true, reloadOnChange: true);

#if DEBUG
            builder.AddUserSecrets<DBConexion>();
#endif

            builder.AddEnvironmentVariables();
            _config = builder.Build();
        }



        public string ObtenerCadenaConexion()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(
                    "No se encontró 'DefaultConnection' en la configuración.");
            return cs;
        }

    }
}

