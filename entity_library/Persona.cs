using System.ComponentModel.DataAnnotations;

namespace entity_library;

// Clase base de alumnos y docentes. EF la guarda en una sola tabla (TPH).
public abstract class Persona
{
    public long Id { get; set; }

    [Required, StringLength(60, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string Apellido { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{7,8}$")]
    public string Dni { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string NombreCompleto => $"{Apellido}, {Nombre}";
}
