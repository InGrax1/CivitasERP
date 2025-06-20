using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static CivitasERP.Models.DataGridNominas;

namespace CivitasERP.Repositories
{
    /*
    // ApiEmpleadoRepository.cs
    public class ApiEmpleadoRepository : IEmpleadoRepository
    {
        private readonly HttpClient _http;
        public ApiEmpleadoRepository(HttpClient http) => _http = http;

        public Task<List<Empleado>> GetAllAsync()
            => _http.GetFromJsonAsync<List<Empleado>>("api/empleados");

        public Task AddAsync(Empleado e)
            => _http.PostAsJsonAsync("api/empleados", e)
                    .ContinueWith(t => {
                        if (!t.Result.IsSuccessStatusCode)
                            throw new ApplicationException("Error al crear");
                    });
    }*/

}
