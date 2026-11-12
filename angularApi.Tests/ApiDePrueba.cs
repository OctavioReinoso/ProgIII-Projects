using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using dao_library.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace angularApi.Tests;

// Levanta la API completa en memoria, cambiando SQL Server por SQLite en memoria.
// Cada test crea su propia instancia, así las pruebas no comparten datos.
public sealed class ApiDePrueba : WebApplicationFactory<Program>
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SqliteConnection conexion = new("Data Source=:memory:");

    public ApiDePrueba() => conexion.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<EscuelaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<EscuelaDbContext>>();
            services.AddDbContext<EscuelaDbContext>(options => options.UseSqlite(conexion));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<EscuelaDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        conexion.Dispose();
    }

    // ---- Ayudas para crear datos en los tests ----

    private static int contador = 1000000;
    public static string NuevoDni() => Interlocked.Increment(ref contador).ToString("D8");

    public static async Task<long> CrearAsync(HttpClient cliente, string recurso, object cuerpo)
    {
        var respuesta = await cliente.PostAsJsonAsync(recurso, cuerpo, Json);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>(Json);
        return json.GetProperty("id").GetInt64();
    }

    public static Task<long> CrearAlumnoAsync(HttpClient c, string legajo = "A-100", string? dni = null) =>
        CrearAsync(c, "/Alumnos", new
        {
            nombre = "Lucía", apellido = "Fernández", dni = dni ?? NuevoDni(),
            fechaNacimiento = "2010-04-12", legajo
        });

    public static Task<long> CrearDocenteAsync(HttpClient c, string? dni = null) =>
        CrearAsync(c, "/Docentes", new
        {
            nombre = "Jorge", apellido = "Benítez", dni = dni ?? NuevoDni(),
            fechaNacimiento = "1980-09-01", titulo = "Profesor de Matemática"
        });

    public static Task<long> CrearDivisionAsync(HttpClient c, string letra = "A", int cupo = 30) =>
        CrearAsync(c, "/Divisiones", new { anio = 3, letra, turno = "Manana", cupo });

    public static Task<long> CrearMateriaAsync(HttpClient c, string nombre = "Matemática") =>
        CrearAsync(c, "/Materias", new { nombre, horasSemanales = 5 });
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }
}
