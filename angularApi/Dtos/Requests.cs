using System.ComponentModel.DataAnnotations;
using entity_library.Enums;

namespace angularApi.Dtos;

// Los requests no tienen Id: el Id lo genera la base (POST) o viene en la URL (PUT).

public abstract class PersonaRequest : IValidatableObject
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string Apellido { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos, sin puntos.")]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public DateOnly? FechaNacimiento { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", [nameof(FechaNacimiento)]);
    }
}

public sealed class AlumnoRequest : PersonaRequest
{
    [Required, StringLength(10, MinimumLength = 3)]
    public string Legajo { get; set; } = string.Empty;
}

public sealed class DocenteRequest : PersonaRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Titulo { get; set; } = string.Empty;

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }
}

public sealed class DivisionRequest
{
    [Range(1, 6, ErrorMessage = "El año debe estar entre 1 y 6.")]
    public int Anio { get; set; }

    [Required, RegularExpression("^[A-Z]$", ErrorMessage = "La letra debe ser una mayúscula, por ejemplo \"A\".")]
    public string Letra { get; set; } = string.Empty;

    [Required, EnumDataType(typeof(Turno))]
    public Turno? Turno { get; set; }

    [Range(1, 50)]
    public int Cupo { get; set; } = 30;
}

public sealed class MateriaRequest
{
    [Required, StringLength(80, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 10)]
    public int HorasSemanales { get; set; }
}

public sealed class CalificacionRequest : IValidatableObject
{
    [Range(1, long.MaxValue)]
    public long AlumnoId { get; set; }

    [Range(1, long.MaxValue)]
    public long MateriaId { get; set; }

    [Range(1, 10, ErrorMessage = "La nota debe estar entre 1 y 10.")]
    public decimal Nota { get; set; }

    [Required]
    public DateOnly? Fecha { get; set; }

    [Required, EnumDataType(typeof(TipoEvaluacion))]
    public TipoEvaluacion? Tipo { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fecha > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("No se pueden cargar notas con fecha futura.", [nameof(Fecha)]);
    }
}
