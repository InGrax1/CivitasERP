using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CivitasERP.Models.DataGridNominas;
using CivitasERP.Models;

namespace CivitasERP.Repositories
{
    /*
    // SqlEmpleadoRepository.cs
    public class SqlEmpleadoRepository : IEmpleadoRepository
    {
        private readonly string _cs;
        public SqlEmpleadoRepository(string connectionString)
            => _cs = connectionString;

        public async Task<List<Empleado>> GetAllAsync()
        {
            var lista = new List<Empleado>();
            using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT ID, Nombre, Categoria FROM Empleados", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Empleado
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Categoria = reader.GetString(2)
                });
            }
            return lista;
        }

        public Task AddAsync(Empleado e)
        {
            // idem a tu INSERT actual
            throw new NotImplementedException();
        }
    }*/

}
