using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;


namespace CivitasERP.Clases
{
    public class Conexion
    {
        public string ObtenerCadenaConexion() {

            string conexion;
            conexion = "Server=INGRAX;Database=CivitasERP;Integrated Security=True;";
            return conexion;
        }
    }
}
