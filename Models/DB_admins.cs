using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitasERP.Models
{
    class DB_admins
    {

        public byte[] ObtenerHashContraseña(string Usuario)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            string query = "SELECT admins_contra from admins WHERE admins_usuario = @usuario";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@usuario", Usuario);

                        object result = command.ExecuteScalar();

                        if (result != DBNull.Value && result != null)
                        {
                            return (byte[])result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener la contraseña: " + ex.Message);
                }
            }
            return null;
        }
        public int? ObtenerIdPorUsuario(string nombreUsuario)
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {

                try
                {
                    conexion.Open();
                    string query = "SELECT id_admins FROM admins WHERE admins_usuario = @NombreUsuario";

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

                        object resultado = comando.ExecuteScalar();

                        if (resultado != null && int.TryParse(resultado.ToString(), out int id))
                        {
                            return id;
                        }
                        else
                        {
                            return null; // Usuario no encontrado
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al acceder a la base de datos: {ex.Message}");
                    return null;
                }
            }
        }
    }
}