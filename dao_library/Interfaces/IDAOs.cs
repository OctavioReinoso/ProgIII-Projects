using entity_library;

namespace dao_library.Interfaces;

// Productos abstractos de la Abstract Factory: lo que la API sabe hacer con cada entidad,
// sin saber si los datos están en SQL Server (EF) o en memoria (Mock).

public interface IDAOAlumno
{
    Task<List<Alumno>> ListarAsync(long? divisionId = null);
    Task<Alumno?> BuscarAsync(long id);
    Task<Resultado> CrearAsync(Alumno alumno);
    Task<Resultado> ModificarAsync(long id, Alumno datos);
    Task<bool> EliminarAsync(long id);
    Task<List<PromedioMateria>?> BoletinAsync(long id);
}

public interface IDAODocente
{
    Task<List<Docente>> ListarAsync();
    Task<Docente?> BuscarAsync(long id);
    Task<Resultado> CrearAsync(Docente docente);
    Task<Resultado> ModificarAsync(long id, Docente datos);
    // MinimoRequerido si es el único docente de alguna materia.
    Task<Resultado> EliminarAsync(long id);
}

public interface IDAODivision
{
    Task<List<Division>> ListarAsync();
    Task<Division?> BuscarAsync(long id);
    Task<Resultado> CrearAsync(Division division);
    Task<Resultado> ModificarAsync(long id, Division datos);
    Task<bool> EliminarAsync(long id);
    Task<Resultado> InscribirAlumnoAsync(long id, long alumnoId);
    Task<Resultado> QuitarAlumnoAsync(long id, long alumnoId);
}

public interface IDAOMateria
{
    Task<List<Materia>> ListarAsync();
    Task<Materia?> BuscarAsync(long id);
    // Una materia se crea con al menos un docente que ya exista.
    Task<Resultado> CrearAsync(Materia materia, IReadOnlyCollection<long> docenteIds);
    Task<Resultado> ModificarAsync(long id, Materia datos);
    Task<bool> EliminarAsync(long id);
    Task<Resultado> AsignarDocenteAsync(long id, long docenteId);
    // MinimoRequerido si es el último docente de la materia.
    Task<Resultado> QuitarDocenteAsync(long id, long docenteId);
}

public interface IDAOCalificacion
{
    Task<List<Calificacion>> ListarAsync(long? alumnoId = null, long? materiaId = null);
    Task<Calificacion?> BuscarAsync(long id);
    Task<Resultado> CrearAsync(Calificacion calificacion);
    Task<Resultado> ModificarAsync(long id, Calificacion datos);
    Task<bool> EliminarAsync(long id);
}

// El "DAOUser" de la consigna de Abstract Factory.
public interface IDAOUsuario
{
    Task<Usuario?> BuscarAsync(long id);
    Task<Usuario?> BuscarPorEmailAsync(string email);
    // Devuelve null si el email ya está registrado.
    Task<Usuario?> CrearAsync(string email, string passwordHash);
    // Sólo modifica/elimina si el hash sigue siendo el que se verificó: si otra operación
    // cambió la contraseña o borró la cuenta en el medio, devuelve false.
    Task<bool> ModificarAsync(long id, string email, string hashVerificado, string? hashNuevo);
    Task<bool> EliminarAsync(long id, string hashVerificado);
}
