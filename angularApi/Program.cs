using System.Text.Json.Serialization;
using angularApi.Filtros;
using dao_library.Data;
using dao_library.EntityFramework;
using dao_library.Interfaces;
using dao_library.Mock;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Abstract Factory: acá, y sólo acá, se elige qué familia de DAOs usa toda la API.
// "AccesoDatos" se cambia en appsettings.json o al ejecutar: dotnet run -- --AccesoDatos=Mock
var accesoDatos = builder.Configuration["AccesoDatos"] ?? "EF";

if (accesoDatos.Equals("Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton(_ => MockDAOFactory.CrearConDatosDeEjemplo());
    builder.Services.AddScoped<IDAOFactory, MockDAOFactory>();
}
else
{
    var cadenaConexion = builder.Configuration.GetConnectionString("Escuela")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'Escuela' en appsettings.json.");
    builder.Services.AddDbContext<EscuelaDbContext>(options => options.UseSqlServer(cadenaConexion));
    builder.Services.AddScoped<IDAOFactory, EFDAOFactory>();
}

builder.Services.AddControllers(options => options.Filters.Add<ErroresBaseDeDatosFilter>())
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto ("Manana", "Escrita") en lugar de números.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Un campo que no existe en el DTO es un error (400), no se ignora en silencio.
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Permite que el front de Angular (ng serve) consuma la API.
builder.Services.AddCors(options => options.AddPolicy("Angular", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Angular");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necesario para que el proyecto de pruebas pueda referenciar la API.
public partial class Program { }
