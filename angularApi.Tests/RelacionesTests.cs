using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static angularApi.Tests.ApiDePrueba;

namespace angularApi.Tests;

public abstract class RelacionesTests : PruebasApi
{
    [Fact]
    public async Task Inscribir_alumno_en_division_se_ve_desde_los_dos_lados()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var divisionId = await CrearDivisionAsync(cliente);
        var alumnoId = await CrearAlumnoAsync(cliente);

        var primera = await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/{alumnoId}", null);
        var repetida = await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/{alumnoId}", null);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repetida.StatusCode); // inscribir dos veces no duplica

        var division = await cliente.GetFromJsonAsync<JsonElement>($"/Divisiones/{divisionId}", Json);
        Assert.Equal(1, division.GetProperty("inscriptos").GetInt32());
        Assert.Equal("Manana", division.GetProperty("turno").GetString());

        var alumno = await cliente.GetFromJsonAsync<JsonElement>($"/Alumnos/{alumnoId}", Json);
        Assert.Equal(divisionId, alumno.GetProperty("division").GetProperty("id").GetInt64());

        var filtrados = await cliente.GetFromJsonAsync<JsonElement>($"/Alumnos?divisionId={divisionId}", Json);
        Assert.Equal(1, filtrados.GetArrayLength());
    }

    [Fact]
    public async Task Un_alumno_no_puede_estar_en_dos_divisiones()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var divisionA = await CrearDivisionAsync(cliente, "A");
        var divisionB = await CrearDivisionAsync(cliente, "B");
        var alumnoId = await CrearAlumnoAsync(cliente);
        await cliente.PutAsync($"/Divisiones/{divisionA}/alumnos/{alumnoId}", null);

        var mover = await cliente.PutAsync($"/Divisiones/{divisionB}/alumnos/{alumnoId}", null);
        Assert.Equal(HttpStatusCode.Conflict, mover.StatusCode);

        // Quitándolo primero de la división A, sí se puede pasar a la B.
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/Divisiones/{divisionA}/alumnos/{alumnoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsync($"/Divisiones/{divisionB}/alumnos/{alumnoId}", null)).StatusCode);
    }

    [Fact]
    public async Task No_se_puede_superar_el_cupo_de_la_division()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var divisionId = await CrearDivisionAsync(cliente, cupo: 1);
        var primero = await CrearAlumnoAsync(cliente, "A-1");
        var segundo = await CrearAlumnoAsync(cliente, "A-2");

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/{primero}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/{segundo}", null)).StatusCode);

        // Tampoco se puede bajar el cupo por debajo de los inscriptos.
        var bajarCupo = await cliente.PutAsJsonAsync($"/Divisiones/{divisionId}", new { anio = 3, letra = "A", turno = "Manana", cupo = 0 }, Json);
        Assert.Equal(HttpStatusCode.BadRequest, bajarCupo.StatusCode);
    }

    [Fact]
    public async Task Borrar_una_division_no_borra_a_sus_alumnos()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var divisionId = await CrearDivisionAsync(cliente);
        var alumnoId = await CrearAlumnoAsync(cliente);
        await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/{alumnoId}", null);

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/Divisiones/{divisionId}")).StatusCode);

        var alumno = await cliente.GetFromJsonAsync<JsonElement>($"/Alumnos/{alumnoId}", Json);
        Assert.Equal(JsonValueKind.Null, alumno.GetProperty("division").ValueKind);
    }

    [Fact]
    public async Task Inscribir_con_ids_inexistentes_devuelve_404()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var divisionId = await CrearDivisionAsync(cliente);

        Assert.Equal(HttpStatusCode.NotFound, (await cliente.PutAsync($"/Divisiones/{divisionId}/alumnos/999", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.PutAsync("/Divisiones/999/alumnos/1", null)).StatusCode);
    }

    [Fact]
    public async Task Docentes_y_materias_muchos_a_muchos()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var matematica = await CrearMateriaAsync(cliente, "Matemática");
        var fisica = await CrearMateriaAsync(cliente, "Física");
        var docenteId = await CrearDocenteAsync(cliente);

        await cliente.PutAsync($"/Materias/{matematica}/docentes/{docenteId}", null);
        await cliente.PutAsync($"/Materias/{fisica}/docentes/{docenteId}", null);
        await cliente.PutAsync($"/Materias/{fisica}/docentes/{docenteId}", null); // repetido, no duplica

        var docente = await cliente.GetFromJsonAsync<JsonElement>($"/Docentes/{docenteId}", Json);
        Assert.Equal(2, docente.GetProperty("materias").GetArrayLength());
        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{fisica}", Json);
        Assert.Equal(1, materia.GetProperty("docentes").GetArrayLength());

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/Materias/{fisica}/docentes/{docenteId}")).StatusCode);
        docente = await cliente.GetFromJsonAsync<JsonElement>($"/Docentes/{docenteId}", Json);
        Assert.Equal("Matemática", Assert.Single(docente.GetProperty("materias").EnumerateArray()).GetProperty("nombre").GetString());
    }
}
