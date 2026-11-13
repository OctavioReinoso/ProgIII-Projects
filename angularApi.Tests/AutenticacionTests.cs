using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static angularApi.Tests.ApiDePrueba;

namespace angularApi.Tests;

public abstract class AutenticacionTests : PruebasApi
{
    [Fact]
    public async Task Registro_devuelve_solo_id_y_email()
    {
        using var api = CrearApi();
        var respuesta = await api.ClienteSinToken().PostAsJsonAsync("/Usuarios",
            new { email = "  Ana@Escuela.edu.ar ", password = "clave-segura-1" }, Json);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal("Ana@Escuela.edu.ar", json.GetProperty("email").GetString()); // sin los espacios
        Assert.Equal(["id", "email"], json.EnumerateObject().Select(p => p.Name));  // nada de hashes
    }

    [Fact]
    public async Task El_email_no_se_repite_aunque_cambien_mayusculas_o_espacios()
    {
        using var api = CrearApi();
        await api.RegistrarYEntrarAsync("ana@escuela.edu.ar");

        var respuesta = await api.ClienteSinToken().PostAsJsonAsync("/Usuarios",
            new { email = " ANA@ESCUELA.EDU.AR", password = "otra-clave-1" }, Json);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_devuelve_un_token_que_sirve_para_consultar_la_cuenta()
    {
        using var api = CrearApi();
        var (id, token, cliente) = await api.RegistrarYEntrarAsync("ana@escuela.edu.ar");

        Assert.Equal(3, token.Split('.').Length); // header.payload.firma
        var yo = await cliente.GetFromJsonAsync<JsonElement>("/Usuarios/yo", Json);
        Assert.Equal(id, yo.GetProperty("id").GetInt64());
        Assert.Equal("ana@escuela.edu.ar", yo.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Password_incorrecta_y_email_inexistente_dan_el_mismo_401()
    {
        using var api = CrearApi();
        await api.RegistrarYEntrarAsync("ana@escuela.edu.ar", "clave-segura-1");
        var cliente = api.ClienteSinToken();

        var malaPassword = await cliente.PostAsJsonAsync("/Usuarios/login", new { email = "ana@escuela.edu.ar", password = "otra-cosa-1" }, Json);
        var noExiste = await cliente.PostAsJsonAsync("/Usuarios/login", new { email = "nadie@escuela.edu.ar", password = "clave-segura-1" }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, malaPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, noExiste.StatusCode);
        // Mismo mensaje: no se puede averiguar qué emails están registrados.
        var detalle1 = (await malaPassword.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("detail").GetString();
        var detalle2 = (await noExiste.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("detail").GetString();
        Assert.Equal(detalle1, detalle2);
    }

    [Theory]
    [InlineData("sin-token")]
    [InlineData("basura")]
    [InlineData("firma-alterada")]
    [InlineData("otra-clave")]
    public async Task Yo_rechaza_tokens_ausentes_o_invalidos(string variante)
    {
        using var api = CrearApi();
        var (_, token, _) = await api.RegistrarYEntrarAsync();
        var cliente = api.ClienteSinToken();

        string? tokenUsado = variante switch
        {
            "sin-token" => null,
            "basura" => "esto.no.esunjwt",
            "firma-alterada" => token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA"),
            _ => TokenFirmadoConOtraClave(),
        };
        if (tokenUsado is not null) cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsado);

        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/Usuarios/yo")).StatusCode);
    }

    [Theory]
    [InlineData("POST", "/Alumnos")]
    [InlineData("PUT", "/Alumnos/1")]
    [InlineData("DELETE", "/Alumnos/1")]
    [InlineData("POST", "/Calificaciones")]
    [InlineData("PUT", "/Divisiones/1/alumnos/1")]
    [InlineData("DELETE", "/Materias/1/docentes/1")]
    public async Task Crear_modificar_y_borrar_requieren_token(string metodo, string ruta)
    {
        using var api = CrearApi();
        var pedido = new HttpRequestMessage(new HttpMethod(metodo), ruta)
        {
            Content = metodo == "DELETE" ? null : new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var respuesta = await api.ClienteSinToken().SendAsync(pedido);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("/Alumnos")]
    [InlineData("/Docentes")]
    [InlineData("/Divisiones")]
    [InlineData("/Materias")]
    [InlineData("/Calificaciones")]
    public async Task Las_consultas_siguen_siendo_publicas(string ruta)
    {
        using var api = CrearApi();
        Assert.Equal(HttpStatusCode.OK, (await api.ClienteSinToken().GetAsync(ruta)).StatusCode);
    }

    [Fact]
    public async Task Con_el_token_de_un_login_real_se_puede_cargar_un_alumno()
    {
        using var api = CrearApi();
        var (_, _, cliente) = await api.RegistrarYEntrarAsync();

        var id = await CrearAlumnoAsync(cliente);

        Assert.True(id > 0);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Nadie_puede_ver_ni_cambiar_la_cuenta_de_otro(string metodo)
    {
        using var api = CrearApi();
        var (idAna, _, _) = await api.RegistrarYEntrarAsync("ana@escuela.edu.ar");
        var (_, _, clienteBeto) = await api.RegistrarYEntrarAsync("beto@escuela.edu.ar");

        var pedido = new HttpRequestMessage(new HttpMethod(metodo), $"/Usuarios/{idAna}")
        {
            Content = metodo switch
            {
                "PUT" => JsonContent.Create(new { email = "robada@escuela.edu.ar", passwordActual = "clave-segura-1" }, options: Json),
                "DELETE" => JsonContent.Create(new { passwordActual = "clave-segura-1" }, options: Json),
                _ => null
            }
        };

        Assert.Equal(HttpStatusCode.Forbidden, (await clienteBeto.SendAsync(pedido)).StatusCode);
        var login = await api.ClienteSinToken().PostAsJsonAsync("/Usuarios/login",
            new { email = "ana@escuela.edu.ar", password = "clave-segura-1" }, Json);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode); // la cuenta de Ana sigue intacta
    }

    [Fact]
    public async Task Cambiar_email_y_password_pide_la_password_actual()
    {
        using var api = CrearApi();
        var (id, _, cliente) = await api.RegistrarYEntrarAsync("ana@escuela.edu.ar", "clave-vieja-1");

        var conPasswordMala = await cliente.PutAsJsonAsync($"/Usuarios/{id}",
            new { email = "ana.nueva@escuela.edu.ar", passwordActual = "no-es-esta-1", passwordNueva = "clave-nueva-1" }, Json);
        Assert.Equal(HttpStatusCode.Unauthorized, conPasswordMala.StatusCode);

        var ok = await cliente.PutAsJsonAsync($"/Usuarios/{id}",
            new { email = "ana.nueva@escuela.edu.ar", passwordActual = "clave-vieja-1", passwordNueva = "clave-nueva-1" }, Json);
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);

        var anonimo = api.ClienteSinToken();
        var conVieja = await anonimo.PostAsJsonAsync("/Usuarios/login", new { email = "ana.nueva@escuela.edu.ar", password = "clave-vieja-1" }, Json);
        var conNueva = await anonimo.PostAsJsonAsync("/Usuarios/login", new { email = "ana.nueva@escuela.edu.ar", password = "clave-nueva-1" }, Json);
        Assert.Equal(HttpStatusCode.Unauthorized, conVieja.StatusCode);
        Assert.Equal(HttpStatusCode.OK, conNueva.StatusCode);
    }

    [Fact]
    public async Task Sin_password_nueva_se_conserva_la_anterior()
    {
        using var api = CrearApi();
        var (id, _, cliente) = await api.RegistrarYEntrarAsync("ana@escuela.edu.ar", "clave-segura-1");

        var respuesta = await cliente.PutAsJsonAsync($"/Usuarios/{id}",
            new { email = "ana@escuela.edu.ar", passwordActual = "clave-segura-1", passwordNueva = (string?)null }, Json);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var login = await api.ClienteSinToken().PostAsJsonAsync("/Usuarios/login", new { email = "ana@escuela.edu.ar", password = "clave-segura-1" }, Json);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task No_se_puede_cambiar_al_email_de_otra_cuenta()
    {
        using var api = CrearApi();
        await api.RegistrarYEntrarAsync("ana@escuela.edu.ar");
        var (idBeto, _, clienteBeto) = await api.RegistrarYEntrarAsync("beto@escuela.edu.ar");

        var respuesta = await clienteBeto.PutAsJsonAsync($"/Usuarios/{idBeto}",
            new { email = "ANA@escuela.edu.ar", passwordActual = "clave-segura-1" }, Json);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_la_cuenta_deja_sin_efecto_su_token()
    {
        using var api = CrearApi();
        var (id, _, cliente) = await api.RegistrarYEntrarAsync("ana@escuela.edu.ar", "clave-segura-1");

        var eliminar = new HttpRequestMessage(HttpMethod.Delete, $"/Usuarios/{id}")
        {
            Content = JsonContent.Create(new { passwordActual = "clave-segura-1" }, options: Json)
        };
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.SendAsync(eliminar)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/Usuarios/yo")).StatusCode);
        var login = await api.ClienteSinToken().PostAsJsonAsync("/Usuarios/login", new { email = "ana@escuela.edu.ar", password = "clave-segura-1" }, Json);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Theory]
    [InlineData("""{"email":"no-es-un-email","password":"clave-segura-1"}""")]
    [InlineData("""{"email":"ana@escuela.edu.ar","password":"corta"}""")]
    [InlineData("""{"email":"ana@escuela.edu.ar"}""")]
    [InlineData("""{"email":"ana@escuela.edu.ar","password":"clave-segura-1","rol":"admin"}""")]
    public async Task Registro_invalido_devuelve_400(string json)
    {
        using var api = CrearApi();
        var respuesta = await api.ClienteSinToken().PostAsync("/Usuarios", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Password_de_mas_de_72_bytes_se_rechaza_y_la_de_72_se_acepta()
    {
        using var api = CrearApi();
        var cliente = api.ClienteSinToken();
        var de72 = new string('ñ', 36);  // 'ñ' ocupa 2 bytes en UTF-8
        var de74 = new string('ñ', 37);

        var largo = await cliente.PostAsJsonAsync("/Usuarios", new { email = "a@escuela.edu.ar", password = de74 }, Json);
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);

        await api.RegistrarYEntrarAsync("b@escuela.edu.ar", de72); // registra y entra sin errores
    }

    private static string TokenFirmadoConOtraClave()
    {
        using var otraApi = new ApiDePrueba(claveJwt: "otra-clave-distinta-de-la-api-para-firmar-tokens-falsos");
        return otraApi.CreateClient().DefaultRequestHeaders.Authorization!.Parameter!;
    }
}

public class ConfiguracionJwtTests
{
    [Theory]
    [InlineData("")]
    [InlineData("demasiado-corta")]
    public void La_api_no_arranca_sin_una_clave_jwt_segura(string clave)
    {
        using var api = new ApiDePrueba(claveJwt: clave);
        var error = Assert.ThrowsAny<Exception>(() => api.CreateClient());
        Assert.Contains("Jwt", error.ToString());
    }
}
