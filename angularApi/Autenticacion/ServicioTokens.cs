using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using angularApi.Dtos;
using entity_library;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace angularApi.Autenticacion;

public sealed class ServicioTokens
{
    private readonly JwtOpciones opciones;

    public ServicioTokens(IOptions<JwtOpciones> opciones) => this.opciones = opciones.Value;

    public LoginResponse Crear(Usuario usuario)
    {
        // Se descartan los milisegundos: el JWT guarda los tiempos en segundos.
        var ahora = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        var vence = ahora.AddMinutes(opciones.MinutosExpiracion);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Clave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(opciones.Emisor, opciones.Audiencia, claims, ahora, vence, credenciales);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), vence);
    }
}
