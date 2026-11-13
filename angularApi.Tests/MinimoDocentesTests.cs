using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static angularApi.Tests.ApiDePrueba;

namespace angularApi.Tests;

// Regla: una materia nunca puede quedar sin docente.
public abstract class MinimoDocentesTests : PruebasApi
{
    [Theory]
    [InlineData("""{"nombre":"Historia","horasSemanales":3}""")]                     // falta la lista
    [InlineData("""{"nombre":"Historia","horasSemanales":3,"docenteIds":null}""")]   // lista nula
    [InlineData("""{"nombre":"Historia","horasSemanales":3,"docenteIds":[]}""")]     // lista vacía
    [InlineData("""{"nombre":"Historia","horasSemanales":3,"docenteIds":[0]}""")]    // id inválido
    public async Task Crear_materia_sin_docentes_devuelve_400(string json)
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();

        var respuesta = await cliente.PostAsync("/Materias", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, (await cliente.GetFromJsonAsync<JsonElement>("/Materias", Json)).GetArrayLength());
    }

    [Fact]
    public async Task Si_un_docente_no_existe_no_se_crea_la_materia()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var docenteId = await CrearDocenteAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/Materias",
            new { nombre = "Historia", horasSemanales = 3, docenteIds = new[] { docenteId, 999 } }, Json);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal(0, (await cliente.GetFromJsonAsync<JsonElement>("/Materias", Json)).GetArrayLength());
    }

    [Fact]
    public async Task Los_docentes_repetidos_se_toman_una_sola_vez()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var docenteId = await CrearDocenteAsync(cliente);

        var materiaId = await CrearMateriaAsync(cliente, "Historia", docenteId, docenteId);

        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{materiaId}", Json);
        Assert.Equal(docenteId, Assert.Single(materia.GetProperty("docentes").EnumerateArray()).GetProperty("id").GetInt64());
        var docente = await cliente.GetFromJsonAsync<JsonElement>($"/Docentes/{docenteId}", Json);
        Assert.Single(docente.GetProperty("materias").EnumerateArray()); // la relación se ve desde los dos lados
    }

    [Fact]
    public async Task No_se_puede_quitar_al_ultimo_docente()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var docenteId = await CrearDocenteAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente, "Historia", docenteId);

        var quitar = await cliente.DeleteAsync($"/Materias/{materiaId}/docentes/{docenteId}");

        Assert.Equal(HttpStatusCode.Conflict, quitar.StatusCode);
        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{materiaId}", Json);
        Assert.Single(materia.GetProperty("docentes").EnumerateArray());
    }

    [Fact]
    public async Task Con_un_reemplazo_si_se_puede_quitar()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var titular = await CrearDocenteAsync(cliente);
        var suplente = await CrearDocenteAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente, "Historia", titular);

        await cliente.PutAsync($"/Materias/{materiaId}/docentes/{suplente}", null);
        var quitar = await cliente.DeleteAsync($"/Materias/{materiaId}/docentes/{titular}");
        var otraVez = await cliente.DeleteAsync($"/Materias/{materiaId}/docentes/{titular}");

        Assert.Equal(HttpStatusCode.NoContent, quitar.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, otraVez.StatusCode); // ya no estaba: no es error
        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{materiaId}", Json);
        Assert.Equal(suplente, Assert.Single(materia.GetProperty("docentes").EnumerateArray()).GetProperty("id").GetInt64());
    }

    [Fact]
    public async Task No_se_puede_borrar_al_unico_docente_de_una_materia()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var docenteId = await CrearDocenteAsync(cliente);
        var otro = await CrearDocenteAsync(cliente);
        await CrearMateriaAsync(cliente, "Historia", docenteId, otro); // acá tiene compañero
        await CrearMateriaAsync(cliente, "Geografía", docenteId);      // acá es el único

        var borrar = await cliente.DeleteAsync($"/Docentes/{docenteId}");

        Assert.Equal(HttpStatusCode.Conflict, borrar.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/Docentes/{docenteId}")).StatusCode);
        var historia = (await cliente.GetFromJsonAsync<JsonElement>("/Materias", Json)).EnumerateArray()
            .Single(m => m.GetProperty("nombre").GetString() == "Historia");
        Assert.Equal(2, historia.GetProperty("docentes").GetArrayLength()); // no se borró nada a medias
    }

    [Fact]
    public async Task Si_todas_sus_materias_tienen_otro_docente_si_se_puede_borrar()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var docenteId = await CrearDocenteAsync(cliente);
        var otro = await CrearDocenteAsync(cliente);
        var materiaId = await CrearMateriaAsync(cliente, "Historia", docenteId, otro);

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/Docentes/{docenteId}")).StatusCode);

        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{materiaId}", Json);
        Assert.Equal(otro, Assert.Single(materia.GetProperty("docentes").EnumerateArray()).GetProperty("id").GetInt64());
    }

    [Fact]
    public async Task Modificar_una_materia_no_cambia_sus_docentes()
    {
        using var api = CrearApi();
        var cliente = api.CreateClient();
        var materiaId = await CrearMateriaAsync(cliente, "Historia");

        var put = await cliente.PutAsJsonAsync($"/Materias/{materiaId}", new { nombre = "Historia Argentina", horasSemanales = 4 }, Json);
        var conDocentes = await cliente.PutAsJsonAsync($"/Materias/{materiaId}", new { nombre = "Historia", horasSemanales = 4, docenteIds = new[] { 1 } }, Json);

        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, conDocentes.StatusCode); // PUT no acepta docenteIds
        var materia = await cliente.GetFromJsonAsync<JsonElement>($"/Materias/{materiaId}", Json);
        Assert.Single(materia.GetProperty("docentes").EnumerateArray());
    }
}
