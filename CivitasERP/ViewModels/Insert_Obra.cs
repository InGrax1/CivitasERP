using CivitasERP.ViewModels;
using System;
using CivitasERP.Conexiones;
using System.Collections.Generic;
using System.Data;
using CivitasERP.Conexion;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace CivitasERP.ViewModels
{
    public class Insert_Obra
    {
        public bool AgregarObra(string nombreObra, string ubicacion, int? idAdminObra)
        {
            DBConexion Sconexion = new DBConexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;

            if (string.IsNullOrWhiteSpace(nombreObra) || string.IsNullOrWhiteSpace(ubicacion))
                return false;

            string query = "INSERT INTO obra (obra_nombre, obra_ubicacion, id_admin_obra) " +
                           "VALUES (@nombreObra, @ubicacion, @idAdminObra)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombreObra", nombreObra);
                        cmd.Parameters.AddWithValue("@ubicacion", ubicacion);
                        cmd.Parameters.AddWithValue("@idAdminObra", idAdminObra);

                        conn.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar la obra: {ex.Message}");
                return false;
            }
        }
    }
}
