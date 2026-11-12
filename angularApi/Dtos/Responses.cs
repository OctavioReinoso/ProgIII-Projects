using dao_library;
using entity_library.Enums;

namespace angularApi.Dtos;

// Versiones cortas para mostrar relaciones sin ciclos (alumno -> división -> alumnos -> ...).
public record DivisionResumen(long Id, string Nombre);
public record PersonaResumen(long Id, string NombreCompleto, string Dni);
public record MateriaResumen(long Id, string Nombre);

public record AlumnoResponse(long Id, string Nombre, string Apellido, string Dni, DateOnly FechaNacimiento,
    string Legajo, DivisionResumen? Division);

public record DocenteResponse(long Id, string Nombre, string Apellido, string Dni, DateOnly FechaNacimiento,
    string Titulo, string? Email, List<MateriaResumen> Materias);

public record DivisionResponse(long Id, int Anio, string Letra, Turno Turno, string Nombre, int Cupo,
    int Inscriptos, List<PersonaResumen> Alumnos);

public record MateriaResponse(long Id, string Nombre, int HorasSemanales, List<PersonaResumen> Docentes);

public record CalificacionResponse(long Id, decimal Nota, DateOnly Fecha, TipoEvaluacion Tipo, string? Observacion,
    PersonaResumen Alumno, MateriaResumen Materia);

public record BoletinResponse(PersonaResumen Alumno, DivisionResumen? Division, List<PromedioMateria> Materias,
    decimal? PromedioGeneral);
