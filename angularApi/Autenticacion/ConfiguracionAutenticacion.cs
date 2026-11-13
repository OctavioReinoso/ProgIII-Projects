using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace angularApi.Autenticacion;

public static class ConfiguracionAutenticacion
{
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection services, IConfiguration configuration)
    {
        // Si falta la clave o es corta, la API no arranca (mejor que firmar tokens inseguros).
        services.AddOptions<JwtOpciones>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .Validate(o => Encoding.UTF8.GetByteCount(o.Clave) >= 32,
                "Jwt:Clave debe tener al menos 32 bytes. Configurala con: dotnet user-secrets set \"Jwt:Clave\" \"<clave>\" --project angularApi")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOpciones>>((bearer, configuradas) =>
            {
                var o = configuradas.Value;
                bearer.MapInboundClaims = false; // conserva "sub" y "email" con su nombre original
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Clave)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidIssuer = o.Emisor,
                    ValidateAudience = true,
                    ValidAudience = o.Audiencia,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero, // vencido es vencido, sin minutos de tolerancia
                    NameClaimType = JwtRegisteredClaimNames.Email,
                };
            });

        services.AddAuthorization();
        services.AddSingleton<ServicioPasswords>();
        services.AddSingleton<ServicioTokens>();
        return services;
    }
}
