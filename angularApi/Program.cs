using System.Text.Json.Serialization;
using angularApi.Filtros;
using dao_library;
using dao_library.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("Escuela")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Escuela' en appsettings.json.");

builder.Services.AddDbContext<EscuelaDbContext>(options => options.UseSqlServer(cadenaConexion));

builder.Services.AddScoped<AlumnoDAO>();
builder.Services.AddScoped<DocenteDAO>();
builder.Services.AddScoped<DivisionDAO>();
builder.Services.AddScoped<MateriaDAO>();
builder.Services.AddScoped<CalificacionDAO>();

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
