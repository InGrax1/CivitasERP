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
    }
}
