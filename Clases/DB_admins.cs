using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Security.Cryptography;

namespace CivitasERP.Clases
{
    public class Bd_admin 
    {
        string conexionString = "Server=JESUSNEGRETE;Database=Proyecto;Integrated Security=True;";

        public byte[] ObtenerHashContraseña(string Usuario)
        {
            string query = "SELECT admins_contra from admins WHERE admins_usuario = @usuario";

            using (SqlConnection connection = new SqlConnection(conexionString))
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
