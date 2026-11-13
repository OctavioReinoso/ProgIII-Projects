using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using angularApi.Autenticacion;
using dao_library.Data;
using entity_library;
using dao_library.Mock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace angularApi.Tests;

// Levanta la API completa en memoria con la familia de DAOs indicada:
// - "EF": Entity Framework sobre SQLite en memoria (en lugar de SQL Server).
// - "Mock": MockDatabase vacía.
// Cada test crea su propia instancia, así las pruebas no comparten datos.
// CreateClient() devuelve un cliente YA autenticado (con token), porque las escrituras piden login.
// Para probar los 401 se usa ClienteSinToken().
public sealed class ApiDePrueba : WebApplicationFactory<Program>
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public const string ClaveJwtDePrueba = "clave-solo-para-tests-de-la-api-de-la-escuela-2026";

    private readonly string accesoDatos;
    private readonly string claveJwt;
    private readonly SqliteConnection conexion = new("Data Source=:memory:");

    public ApiDePrueba(string accesoDatos = "EF", string claveJwt = ClaveJwtDePrueba)
    {
        this.accesoDatos = accesoDatos;
        this.claveJwt = claveJwt;
        conexion.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("AccesoDatos", accesoDatos);
        builder.UseSetting("Jwt:Clave", claveJwt);
        builder.UseSetting("Jwt:CostoBCrypt", "4"); // BCrypt rápido: los tests no necesitan el costo real
        builder.ConfigureTestServices(services =>
        {
            if (accesoDatos == "Mock")
            {
                // Sin los datos de ejemplo, para que cada test arranque vacío.
                services.RemoveAll<MockDatabase>();
                services.AddSingleton(new MockDatabase());
                return;
            }

            services.RemoveAll<DbContextOptions<EscuelaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<EscuelaDbContext>>();
            services.AddDbContext<EscuelaDbContext>(options => options.UseSqlite(conexion));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<EscuelaDbContext>().Database.EnsureCreated();
        });
    }

    // Token firmado por la propia API para un usuario ficticio: alcanza para las rutas de la escuela,
    // que sólo piden un token válido.
    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        var token = Services.GetRequiredService<ServicioTokens>()
            .Crear(new Usuario { Id = 999_999, Email = "tests@escuela.edu.ar" }).Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public HttpClient ClienteSinToken()
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = null;
        return cliente;
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

    // Una materia necesita al menos un docente: si no se indica, se crea uno.
    public static async Task<long> CrearMateriaAsync(HttpClient c, string nombre = "Matemática", params long[] docenteIds)
    {
        if (docenteIds.Length == 0) docenteIds = [await CrearDocenteAsync(c)];
        return await CrearAsync(c, "/Materias", new { nombre, horasSemanales = 5, docenteIds });
    }

    // Registra una cuenta, inicia sesión y devuelve un cliente con ese token.
    public async Task<(long Id, string Token, HttpClient Cliente)> RegistrarYEntrarAsync(
        string email = "preceptora@escuela.edu.ar", string password = "clave-segura-1")
    {
        var cliente = ClienteSinToken();
        var registro = await cliente.PostAsJsonAsync("/Usuarios", new { email, password }, Json);
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        var id = (await registro.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("id").GetInt64();

        var login = await cliente.PostAsJsonAsync("/Usuarios/login", new { email, password }, Json);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("token").GetString()!;

        var conToken = ClienteSinToken();
        conToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (id, token, conToken);
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }
}
