using System.ComponentModel.DataAnnotations;

namespace entity_library;

public class Materia
{
    public long Id { get; set; }

    [Required, StringLength(80, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 10)]
    public int HorasSemanales { get; set; }

    public List<Docente> Docentes { get; set; } = new();
    public List<Calificacion> Calificaciones { get; set; } = new();
}
