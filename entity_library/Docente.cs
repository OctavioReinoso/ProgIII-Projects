using System.ComponentModel.DataAnnotations;

namespace entity_library;

public class Docente : Persona
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Titulo { get; set; } = string.Empty;

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    public List<Materia> Materias { get; set; } = new();
}
