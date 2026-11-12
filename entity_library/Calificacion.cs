using System.ComponentModel.DataAnnotations;
using entity_library.Enums;

namespace entity_library;

// Nota de un alumno en una materia.
public class Calificacion
{
    public long Id { get; set; }

    [Range(1, 10)]
    public decimal Nota { get; set; }

    public DateOnly Fecha { get; set; }

    public TipoEvaluacion Tipo { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public long AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public long MateriaId { get; set; }
    public Materia? Materia { get; set; }
}
