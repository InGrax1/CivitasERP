using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace CivitasERP.Models
{
    class Insert_Empleado
    {
        
        public string Nombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public decimal Paga_semanal { get; set; }
        public decimal Paga_diaria { get; set; }
        public decimal Paga_Hora_Extra { get; set; }
        public string Obra { get; set; }
        public int? id_admin { get; set; }
        public int? id_obra { get; set; }

        public string categoria { get; set; }
        public byte[] Huella { get; set; }

        public void InsertEmpleado()
        {
            Conexion Sconexion = new Conexion();
            string connectionString;

            string obtenerCadenaConexion = Sconexion.ObtenerCadenaConexion();
            connectionString = obtenerCadenaConexion;



            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO empleado (emp_nombre, emp_apellidop, emp_apellidom, 
                                                     emp_dia, emp_semanal,emp_hora_extra, id_admins,
                                                     id_obra,emp_puesto, emp_huella)
                                 VALUES (@Nombre, @ApellidoP, @ApellidoM, @emp_dia, @emp_semanal,@emp_hora_extra, @id_admins, @id_obra, @Categoria, @Huella)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", Nombre);
                        cmd.Parameters.AddWithValue("@ApellidoP", ApellidoP);
                        cmd.Parameters.AddWithValue("@ApellidoM", ApellidoM);


                        cmd.Parameters.AddWithValue("@emp_dia", Paga_diaria);
                        cmd.Parameters.AddWithValue("@emp_semanal", Paga_semanal);
                        cmd.Parameters.AddWithValue("@emp_hora_extra", Paga_Hora_Extra);

                        cmd.Parameters.AddWithValue("@id_admins", id_admin);
                        cmd.Parameters.AddWithValue("@id_obra", id_obra);
                        cmd.Parameters.AddWithValue("@Categoria", categoria);

                        // Parámetro para la huella (byte[])
                        var pHuella = new SqlParameter("@Huella", SqlDbType.VarBinary, -1);
                        pHuella.Value = (Huella != null) ? (object)Huella : DBNull.Value;
                        cmd.Parameters.Add(pHuella);

                        conn.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        conn.Close();

                        MessageBox.Show($"✅ Empleado registrado con exito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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

    }
    }


