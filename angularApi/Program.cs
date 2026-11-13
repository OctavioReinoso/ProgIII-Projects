using System.Text.Json.Serialization;
using angularApi.Autenticacion;
using angularApi.Filtros;
using dao_library.Data;
using dao_library.EntityFramework;
using dao_library.Interfaces;
using dao_library.Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Abstract Factory: acá, y sólo acá, se elige qué familia de DAOs usa toda la API.
// "AccesoDatos" se cambia en appsettings.json o al ejecutar: dotnet run -- --AccesoDatos=Mock
var accesoDatos = builder.Configuration["AccesoDatos"] ?? "EF";
var usaMock = accesoDatos.Equals("Mock", StringComparison.OrdinalIgnoreCase);

if (usaMock)
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

// Login con JWT (ver carpeta Autenticacion).
builder.Services.AddAutenticacionJwt(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Botón "Authorize" de Swagger para pegar el token.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        Description = "Pegá el token que devuelve POST /Usuarios/login.",
    });
    options.OperationFilter<SeguridadSwaggerFilter>();
});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// En modo Mock se crea una cuenta de prueba para poder entrar sin registrarse.
if (usaMock && !app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var fabrica = scope.ServiceProvider.GetRequiredService<IDAOFactory>();
    var passwords = scope.ServiceProvider.GetRequiredService<ServicioPasswords>();
    await fabrica.CrearDAOUsuario().CrearAsync("preceptor@escuela.edu.ar", passwords.Hashear("escuela2026"));
}

app.Run();

// Necesario para que el proyecto de pruebas pueda referenciar la API.
public partial class Program { }
