using System.ComponentModel.DataAnnotations;

namespace entity_library;

// Cuenta para ingresar al sistema. La contraseña nunca se guarda: sólo su hash BCrypt.
public class Usuario
{
    public long Id { get; set; }

    [Required, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    // Email en mayúsculas y sin espacios: evita cuentas duplicadas como "Ana@x.com" y " ana@x.com".
    [Required, StringLength(254)]
    public string EmailNormalizado { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string PasswordHash { get; set; } = string.Empty;

    public static string NormalizarEmail(string email) => email.Trim().ToUpperInvariant();
}
