using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static angularApi.Tests.ApiDePrueba;

namespace angularApi.Tests;

public class CalificacionesTests
{
    private static object Nota(long alumnoId, long materiaId, decimal nota, string tipo = "Escrita", string fecha = "2026-06-10") =>
        new { alumnoId, materiaId, nota, fecha, tipo };

    [Fact]
    public async Task Cargar_nota_devuelve_alumno_y_materia()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumnoId = await CrearAlumnoAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, materiaId, 8.5m, "Oral"), Json);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(8.5m, json.GetProperty("nota").GetDecimal());
        Assert.Equal("Oral", json.GetProperty("tipo").GetString());
        Assert.Equal("Fernández, Lucía", json.GetProperty("alumno").GetProperty("nombreCompleto").GetString());
        Assert.Equal("Matemática", json.GetProperty("materia").GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task Nota_para_alumno_o_materia_inexistente_devuelve_404()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var materiaId = await CrearMateriaAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/Calificaciones", Nota(999, materiaId, 7), Json);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Nota_fuera_de_rango_devuelve_400(int valor)
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumnoId = await CrearAlumnoAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, materiaId, valor), Json);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Nota_con_fecha_futura_devuelve_400()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumnoId = await CrearAlumnoAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, materiaId, 7, fecha: "2999-01-01"), Json);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Boletin_calcula_promedios_y_aprobacion()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumnoId = await CrearAlumnoAsync(cliente);
        var matematica = await CrearMateriaAsync(cliente, "Matemática");
        var historia = await CrearMateriaAsync(cliente, "Historia");

        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, matematica, 8), Json);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, matematica, 9, "TrabajoPractico"), Json);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, historia, 4), Json);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, historia, 5, "Oral"), Json);

        var boletin = await cliente.GetFromJsonAsync<JsonElement>($"/Alumnos/{alumnoId}/boletin", Json);
        var materias = boletin.GetProperty("materias").EnumerateArray().ToList();

        Assert.Equal("Historia", materias[0].GetProperty("materia").GetString());
        Assert.Equal(4.5m, materias[0].GetProperty("promedio").GetDecimal());
        Assert.False(materias[0].GetProperty("aprobada").GetBoolean());
        Assert.Equal(8.5m, materias[1].GetProperty("promedio").GetDecimal());
        Assert.True(materias[1].GetProperty("aprobada").GetBoolean());
        Assert.Equal(6.5m, boletin.GetProperty("promedioGeneral").GetDecimal());
    }

    [Fact]
    public async Task Filtrar_calificaciones_por_alumno_y_materia()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumno1 = await CrearAlumnoAsync(cliente, "A-1");
        var alumno2 = await CrearAlumnoAsync(cliente, "A-2");
        var materiaId = await CrearMateriaAsync(cliente);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumno1, materiaId, 7), Json);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumno2, materiaId, 9), Json);

        var todas = await cliente.GetFromJsonAsync<JsonElement>($"/Calificaciones?materiaId={materiaId}", Json);
        var delAlumno1 = await cliente.GetFromJsonAsync<JsonElement>($"/Calificaciones?alumnoId={alumno1}", Json);

        Assert.Equal(2, todas.GetArrayLength());
        Assert.Equal(7m, Assert.Single(delAlumno1.EnumerateArray()).GetProperty("nota").GetDecimal());
    }

    [Fact]
    public async Task Borrar_un_alumno_borra_sus_notas()
    {
        using var api = new ApiDePrueba();
        var cliente = api.CreateClient();
        var alumnoId = await CrearAlumnoAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente);
        await cliente.PostAsJsonAsync("/Calificaciones", Nota(alumnoId, materiaId, 7), Json);

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/Alumnos/{alumnoId}")).StatusCode);

        var notas = await cliente.GetFromJsonAsync<JsonElement>("/Calificaciones", Json);
        Assert.Equal(0, notas.GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/Alumnos/{alumnoId}/boletin")).StatusCode);
    }
}
