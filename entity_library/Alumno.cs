using System.ComponentModel.DataAnnotations;

namespace entity_library;

public class Alumno : Persona
{
    [Required, StringLength(10, MinimumLength = 3)]
    public string Legajo { get; set; } = string.Empty;

    // Un alumno está en una sola división (o en ninguna).
    public long? DivisionId { get; set; }
    public Division? Division { get; set; }

    public List<Calificacion> Calificaciones { get; set; } = new();
}
