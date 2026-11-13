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
    Task<bool> EliminarAsync(long id);
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
    Task<Resultado> CrearAsync(Materia materia);
    Task<Resultado> ModificarAsync(long id, Materia datos);
    Task<bool> EliminarAsync(long id);
    Task<Resultado> AsignarDocenteAsync(long id, long docenteId);
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
