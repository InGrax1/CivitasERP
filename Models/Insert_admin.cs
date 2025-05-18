using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CivitasERP.Models;

namespace CivitasERP.Models
{
    public class Insert_admin
    {
        public string Nombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public string Correo { get; set; }
        public string Usuario { get; set; }
        public string Contra { get; set; } // Contraseña en texto plano
        public decimal Semanal { get; set; }
        public string Categoria { get; set; }




        public void InsertarAdmin()
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;



            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO admins (admins_nombre, admins_apellidop, admins_apellidom, 
                                                     admins_correo, admins_contra, admins_usuario, 
                                                     admins_semanal, admin_categoria)
                                 VALUES (@Nombre, @ApellidoP, @ApellidoM, @Correo, @Contra, @Usuario, @Semanal, @Categoria)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", Nombre);
                        cmd.Parameters.AddWithValue("@ApellidoP", ApellidoP);
                        cmd.Parameters.AddWithValue("@ApellidoM", ApellidoM);
                        cmd.Parameters.AddWithValue("@Correo", Correo);
                        cmd.Parameters.AddWithValue("@Contra", HashPassword(Contra));
                        cmd.Parameters.AddWithValue("@Usuario", Usuario);
                        cmd.Parameters.AddWithValue("@Semanal", Semanal);
                        cmd.Parameters.AddWithValue("@Categoria", Categoria);

                        conn.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        conn.Close();

                        Console.WriteLine($"✅ Inserción exitosa. Filas afectadas: {filasAfectadas}");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("❌ Error de SQL:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error general:");
                Console.WriteLine(ex.Message);
            }
        }

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
                Console.WriteLine("❌ Error al encriptar la contraseña:");
                Console.WriteLine(ex.Message);
                return new byte[32]; // Devuelve un arreglo vacío como respaldo
            }
        }
    }
}
