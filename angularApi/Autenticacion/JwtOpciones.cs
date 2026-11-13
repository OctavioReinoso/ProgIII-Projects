using System.ComponentModel.DataAnnotations;

namespace angularApi.Autenticacion;

// Sección "Jwt" de la configuración. La Clave NO va en appsettings.json:
// se guarda con dotnet user-secrets (ver README) para que no quede en Git.
public sealed class JwtOpciones
{
    [Required]
    public string Clave { get; set; } = string.Empty;

    [Required]
    public string Emisor { get; set; } = string.Empty;

    [Required]
    public string Audiencia { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int MinutosExpiracion { get; set; } = 60;

    // Costo de BCrypt: cada +1 duplica el tiempo de cálculo. Los tests lo bajan para ir más rápido.
    [Range(4, 16)]
    public int CostoBCrypt { get; set; } = 12;
}
