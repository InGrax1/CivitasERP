using System;
using CivitasERP.Conexiones;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CivitasERP.Models
{
    internal class Reset_Password
    {
        private byte[] HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al encriptar la contraseña: " + ex.Message);
                return new byte[32]; // Devuelve un arreglo vacío como respaldo
            }
        }

        public bool CambiarContraseña(string nombreUsuario, string nuevaContraseña, int? id_admin)
        {
            
           


            DBConexion Sconexion = new DBConexion();
            string connectionString = Sconexion.ObtenerCadenaConexion();

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();
                    string consulta = "UPDATE admins SET admins_contra = @nuevaContraseña WHERE admins_usuario = @nombreUsuario AND id_admins = @idAdmin";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@idAdmin", id_admin);
                        comando.Parameters.AddWithValue("@nuevaContraseña", HashPassword(nuevaContraseña));
                        comando.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);


                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;

                        MessageBox.Show("contraseña cambiada correctamente");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cambiar la contraseña: " + ex.Message);
                    return false;
                }
            }

        }
    }
}

