using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using angularApi.Autenticacion;
using angularApi.Dtos;
using dao_library.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private const string CredencialesInvalidas = "Email o contraseña incorrectos.";
    private readonly IDAOUsuario usuarioDAO;
    private readonly ServicioPasswords passwords;
    private readonly ServicioTokens tokens;

    // El DAO de usuarios también sale de la Abstract Factory: funciona igual con EF y con Mock.
    public UsuariosController(IDAOFactory fabrica, ServicioPasswords passwords, ServicioTokens tokens)
    {
        usuarioDAO = fabrica.CrearDAOUsuario();
        this.passwords = passwords;
        this.tokens = tokens;
    }

    // POST /Usuarios  (registro, público)
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Registrar(RegistroRequest request)
    {
        var creado = await usuarioDAO.CrearAsync(request.Email, passwords.Hashear(request.Password));
        if (creado is null) return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Ya existe una cuenta con ese email.");
        return CreatedAtAction(nameof(Buscar), new { id = creado.Id }, new UsuarioResponse(creado.Id, creado.Email));
    }

    // POST /Usuarios/login  (público). Mismo 401 si el email no existe o la contraseña está mal.
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await usuarioDAO.BuscarPorEmailAsync(request.Email);
        var passwordOk = passwords.Verificar(request.Password, usuario?.PasswordHash);
        if (usuario is null || !passwordOk) return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: CredencialesInvalidas);
        return tokens.Crear(usuario);
    }

    // GET /Usuarios/yo  -> la cuenta dueña del token
    [HttpGet("yo")]
    public async Task<ActionResult<UsuarioResponse>> Yo()
    {
        if (IdDelToken() is not { } id) return Unauthorized();
        var usuario = await usuarioDAO.BuscarAsync(id);
        return usuario is null ? Unauthorized() : new UsuarioResponse(usuario.Id, usuario.Email);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UsuarioResponse>> Buscar(long id)
    {
        if (ControlarDuenio(id) is { } denegado) return denegado;
        var usuario = await usuarioDAO.BuscarAsync(id);
        return usuario is null ? Unauthorized() : new UsuarioResponse(usuario.Id, usuario.Email);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, ModificarUsuarioRequest request)
    {
        if (ControlarDuenio(id) is { } denegado) return denegado;
        var usuario = await usuarioDAO.BuscarAsync(id);
        if (usuario is null) return Unauthorized();
        if (!passwords.Verificar(request.PasswordActual, usuario.PasswordHash))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: CredencialesInvalidas);

        var otro = await usuarioDAO.BuscarPorEmailAsync(request.Email);
        if (otro is not null && otro.Id != id)
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Ya existe una cuenta con ese email.");

        var hashNuevo = request.PasswordNueva is null ? null : passwords.Hashear(request.PasswordNueva);
        return await usuarioDAO.ModificarAsync(id, request.Email, usuario.PasswordHash, hashNuevo)
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status409Conflict, detail: "La cuenta cambió durante la operación. Volvé a intentarlo.");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id, EliminarUsuarioRequest request)
    {
        if (ControlarDuenio(id) is { } denegado) return denegado;
        var usuario = await usuarioDAO.BuscarAsync(id);
        if (usuario is null) return Unauthorized();
        if (!passwords.Verificar(request.PasswordActual, usuario.PasswordHash))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: CredencialesInvalidas);

        return await usuarioDAO.EliminarAsync(id, usuario.PasswordHash)
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status409Conflict, detail: "La cuenta cambió durante la operación. Volvé a intentarlo.");
    }

    private long? IdDelToken() =>
        long.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    // Cada uno sólo puede ver o cambiar su propia cuenta: un id ajeno da 403.
    private ActionResult? ControlarDuenio(long id) => IdDelToken() switch
    {
        null => Unauthorized(),
        var propio when propio == id => null,
        _ => Forbid(),
    };
}
