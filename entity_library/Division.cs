using System.ComponentModel.DataAnnotations;
using entity_library.Enums;

namespace entity_library;

// Un curso de la escuela, por ejemplo "3° B - Tarde".
public class Division
{
    public long Id { get; set; }

    [Range(1, 6)]
    public int Anio { get; set; }

    [Required, RegularExpression("^[A-Z]$")]
    public string Letra { get; set; } = string.Empty;

    public Turno Turno { get; set; }

    [Range(1, 50)]
    public int Cupo { get; set; } = 30;

    public List<Alumno> Alumnos { get; set; } = new();

    public string Nombre => $"{Anio}° {Letra} - {Turno}";
}
