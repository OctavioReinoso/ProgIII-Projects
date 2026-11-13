using entity_library;

namespace dao_library.Mock;

// "Base de datos" en memoria. Se registra como Singleton en Program.cs: todos los pedidos
// HTTP ven las mismas listas mientras la aplicación esté corriendo.
public class MockDatabase
{
    // Las listas no son thread-safe y la API atiende pedidos en paralelo: todo acceso usa este lock.
    public object Sync { get; } = new();

    public List<Alumno> Alumnos { get; } = new();
    public List<Docente> Docentes { get; } = new();
    public List<Division> Divisiones { get; } = new();
    public List<Materia> Materias { get; } = new();
    public List<Calificacion> Calificaciones { get; } = new();
    public List<Usuario> Usuarios { get; } = new();

    // Alumnos y docentes comparten la secuencia de Id, igual que en la tabla Personas.
    private long ultimoIdPersona, ultimoIdDivision, ultimoIdMateria, ultimoIdCalificacion, ultimoIdUsuario;

    public long SiguienteIdPersona() => ++ultimoIdPersona;
    public long SiguienteIdDivision() => ++ultimoIdDivision;
    public long SiguienteIdMateria() => ++ultimoIdMateria;
    public long SiguienteIdCalificacion() => ++ultimoIdCalificacion;
    public long SiguienteIdUsuario() => ++ultimoIdUsuario;

    public IEnumerable<Persona> Personas => Alumnos.Cast<Persona>().Concat(Docentes);
}
