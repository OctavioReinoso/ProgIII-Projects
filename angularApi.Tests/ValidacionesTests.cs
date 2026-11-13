using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static angularApi.Tests.ApiDePrueba;

namespace angularApi.Tests;

public abstract class ValidacionesTests : PruebasApi
{
    [Fact]
    public async Task Crear_y_consultar_un_alumno()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();

        var id = await CrearAlumnoAsync(cliente, legajo: "A-001", dni: "45123456");

        var alumno = await cliente.GetFromJsonAsync<JsonElement>($"/Alumnos/{id}", Json);
        Assert.Equal("45123456", alumno.GetProperty("dni").GetString());
        Assert.Equal("A-001", alumno.GetProperty("legajo").GetString());
        Assert.Equal(JsonValueKind.Null, alumno.GetProperty("division").ValueKind);
    }

    [Fact]
    public async Task No_se_repite_el_dni_entre_alumnos_y_docentes()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        await CrearAlumnoAsync(cliente, dni: "40111222");

        var respuesta = await cliente.PostAsJsonAsync("/Docentes", new
        {
            nombre = "Ana", apellido = "Pérez", dni = "40111222", fechaNacimiento = "1985-01-01", titulo = "Profesora"
        }, Json);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_se_repite_el_legajo()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        await CrearAlumnoAsync(cliente, legajo: "A-777");

        var respuesta = await cliente.PostAsJsonAsync("/Alumnos", new
        {
            nombre = "Otro", apellido = "Alumno", dni = NuevoDni(), fechaNacimiento = "2011-01-01", legajo = "A-777"
        }, Json);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("""{"nombre":"Lu","apellido":"Paz","dni":"12.345.678","fechaNacimiento":"2010-01-01","legajo":"A-1"}""")] // DNI con puntos
    [InlineData("""{"nombre":"Lu","apellido":"Paz","dni":"12345678","fechaNacimiento":"2999-01-01","legajo":"A-1"}""")] // fecha futura
    [InlineData("""{"nombre":"Lu","apellido":"Paz","dni":"12345678","legajo":"A-1"}""")]                                // falta fecha
    [InlineData("""{"nombre":"Lu","apellido":"Paz","dni":"12345678","fechaNacimiento":"2010-01-01","legajo":"A-1","divisionId":1}""")] // campo desconocido
    public async Task Alumno_invalido_devuelve_400(string json)
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();

        var respuesta = await cliente.PostAsync("/Alumnos", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var alumnos = await cliente.GetFromJsonAsync<JsonElement>("/Alumnos", Json);
        Assert.Equal(0, alumnos.GetArrayLength());
    }

    [Theory]
    [InlineData("""{"anio":7,"letra":"A","turno":"Manana"}""")]  // año fuera de rango
    [InlineData("""{"anio":3,"letra":"a","turno":"Manana"}""")]  // letra minúscula
    [InlineData("""{"anio":3,"letra":"A","turno":"Noche"}""")]   // turno inexistente
    [InlineData("""{"anio":3,"letra":"A","turno":99}""")]        // turno numérico inexistente
    public async Task Division_invalida_devuelve_400(string json)
    {
        using var api = CrearApi();
        var respuesta = await api.CreateClient()
            .PostAsync("/Divisiones", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_puede_haber_dos_divisiones_iguales()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        await CrearDivisionAsync(cliente, "B");

        var respuesta = await cliente.PostAsJsonAsync("/Divisiones", new { anio = 3, letra = "B", turno = "Manana" }, Json);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Modificar_y_eliminar_inexistentes_devuelve_404()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();

        var put = await cliente.PutAsJsonAsync("/Materias/999", new { nombre = "Historia", horasSemanales = 3 }, Json);
        var delete = await cliente.DeleteAsync("/Materias/999");
        var get = await cliente.GetAsync("/Materias/999");

        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Modificar_un_docente()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var id = await CrearDocenteAsync(cliente, dni: "30123456");

        var respuesta = await cliente.PutAsJsonAsync($"/Docentes/{id}", new
        {
            nombre = "Jorge", apellido = "Benítez", dni = "30123456", fechaNacimiento = "1980-09-01",
            titulo = "Licenciado en Matemática", email = "jbenitez@escuela.edu.ar"
        }, Json);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var docente = await cliente.GetFromJsonAsync<JsonElement>($"/Docentes/{id}", Json);
        Assert.Equal("Licenciado en Matemática", docente.GetProperty("titulo").GetString());
    }
}
