using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1) Carga de configuración
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)                         // valores comunes
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                 optional: true, reloadOnChange: true)                                             // valores Dev o Production
    .AddEnvironmentVariables();                                                                   // para Azure App Settings

// 2) Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3) Middleware de Swagger en Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4) Ejemplo de endpoint usando la conexión correcta
app.MapGet("/api/empleados", async (IConfiguration config) =>
{
    var lista = new List<object>();
    // lee el ConnectionString ya sea de Dev o de Prod
    string cs = config.GetConnectionString("DefaultConnection");

    await using var conn = new SqlConnection(cs);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("SELECT ID, Nombre, Categoria FROM Empleados", conn);
    using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    while (await reader.ReadAsync())
    {
        lista.Add(new
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Categoria = reader.GetString(2)
        });
    }

    return Results.Ok(lista);
});

app.Run();
