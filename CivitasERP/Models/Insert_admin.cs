using System;
using CivitasERP.Conexiones;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows;
using System.Data;

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

        public byte[] Huella { get; set; }

        public string usuario { get; set; }

        public void InsertarAdmin()
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;



            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO admins (admins_nombre, admins_apellidop, admins_apellidom, 
                                 admins_correo, admins_contra, admins_usuario, 
                                 admins_semanal, admin_categoria, admin_huella)
             VALUES (@Nombre, @ApellidoP, @ApellidoM, @Correo, @Contra, @Usuario, @Semanal, @Categoria, @Huella)";
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

                        // Parámetro para la huella (byte[])
                        var pHuella = new SqlParameter("@Huella", SqlDbType.VarBinary, -1);
                        pHuella.Value = (Huella != null) ? (object)Huella : DBNull.Value;
                        cmd.Parameters.Add(pHuella);

                        conn.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        conn.Close();

                         MessageBox.Show($"✅ Administrador registrado con exito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("❌ Error de SQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error general: " + ex.Message);
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
                MessageBox.Show("❌ Error al encriptar la contraseña: " + ex.Message);
                return new byte[32]; // Devuelve un arreglo vacío como respaldo
            }
        }


            public void InsertarJefe()
            {
                string connectionString = new DBConexion().ObtenerCadenaConexion();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Jefe (Jefe_usuario, Jefe_contra) VALUES (@usuario, @contra)", conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", Usuario);
                        cmd.Parameters.AddWithValue("@contra", HashPassword(Contra));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        
    }
}
