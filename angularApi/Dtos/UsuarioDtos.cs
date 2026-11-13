using System.ComponentModel.DataAnnotations;
using System.Text;

namespace angularApi.Dtos;

public abstract class ConEmailRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
}

public sealed class RegistroRequest : ConEmailRequest
{
    [Required, MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres."), MaximoBytesBCrypt]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest : ConEmailRequest
{
    [Required, MaximoBytesBCrypt]
    public string Password { get; set; } = string.Empty;
}

public sealed class ModificarUsuarioRequest : ConEmailRequest
{
    // Para cambiar algo de la cuenta hay que volver a escribir la contraseña actual.
    [Required, MaximoBytesBCrypt]
    public string PasswordActual { get; set; } = string.Empty;

    // Opcional: si no viene (o es null) se conserva la contraseña.
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres."), MaximoBytesBCrypt]
    public string? PasswordNueva { get; set; }
}

public sealed class EliminarUsuarioRequest
{
    [Required, MaximoBytesBCrypt]
    public string PasswordActual { get; set; } = string.Empty;
}

// BCrypt sólo usa los primeros 72 bytes: si se aceptara más, dos contraseñas distintas
// que empiezan igual darían el mismo hash.
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaximoBytesBCryptAttribute : ValidationAttribute
{
    public MaximoBytesBCryptAttribute() : base("La contraseña no puede superar los 72 bytes.") { }

    public override bool IsValid(object? value) => value is not string texto || Encoding.UTF8.GetByteCount(texto) <= 72;
}

public record UsuarioResponse(long Id, string Email);
public record LoginResponse(string Token, DateTime Vence);
