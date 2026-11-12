using entity_library;

namespace angularApi.Dtos;

// Conversión entre DTOs y entidades.
public static class Mapeos
{
    public static Alumno AEntidad(this AlumnoRequest r) => new()
    {
        Nombre = r.Nombre.Trim(), Apellido = r.Apellido.Trim(), Dni = r.Dni,
        FechaNacimiento = r.FechaNacimiento!.Value, Legajo = r.Legajo.Trim()
    };

    public static Docente AEntidad(this DocenteRequest r) => new()
    {
        Nombre = r.Nombre.Trim(), Apellido = r.Apellido.Trim(), Dni = r.Dni,
        FechaNacimiento = r.FechaNacimiento!.Value, Titulo = r.Titulo.Trim(), Email = r.Email
    };

    public static Division AEntidad(this DivisionRequest r) => new()
    {
        Anio = r.Anio, Letra = r.Letra, Turno = r.Turno!.Value, Cupo = r.Cupo
    };

    public static Materia AEntidad(this MateriaRequest r) => new()
    {
        Nombre = r.Nombre.Trim(), HorasSemanales = r.HorasSemanales
    };

    public static Calificacion AEntidad(this CalificacionRequest r) => new()
    {
        AlumnoId = r.AlumnoId, MateriaId = r.MateriaId, Nota = r.Nota,
        Fecha = r.Fecha!.Value, Tipo = r.Tipo!.Value, Observacion = r.Observacion
    };

    public static PersonaResumen AResumen(this Persona p) => new(p.Id, p.NombreCompleto, p.Dni);
    public static MateriaResumen AResumen(this Materia m) => new(m.Id, m.Nombre);
    public static DivisionResumen AResumen(this Division d) => new(d.Id, d.Nombre);

    public static AlumnoResponse AResponse(this Alumno a) => new(a.Id, a.Nombre, a.Apellido, a.Dni,
        a.FechaNacimiento, a.Legajo, a.Division?.AResumen());

    public static DocenteResponse AResponse(this Docente d) => new(d.Id, d.Nombre, d.Apellido, d.Dni,
        d.FechaNacimiento, d.Titulo, d.Email, d.Materias.OrderBy(m => m.Nombre).Select(m => m.AResumen()).ToList());

    public static DivisionResponse AResponse(this Division d) => new(d.Id, d.Anio, d.Letra, d.Turno, d.Nombre,
        d.Cupo, d.Alumnos.Count,
        d.Alumnos.OrderBy(a => a.Apellido).ThenBy(a => a.Nombre).Select(a => a.AResumen()).ToList());

    public static MateriaResponse AResponse(this Materia m) => new(m.Id, m.Nombre, m.HorasSemanales,
        m.Docentes.OrderBy(d => d.Apellido).Select(d => d.AResumen()).ToList());

    public static CalificacionResponse AResponse(this Calificacion c) => new(c.Id, c.Nota, c.Fecha, c.Tipo,
        c.Observacion, c.Alumno!.AResumen(), c.Materia!.AResumen());
}
