using dao_library.Data;
using dao_library.Interfaces;

namespace dao_library.EntityFramework;

// Fábrica concreta 1: todos los DAOs comparten el mismo DbContext (misma base, misma transacción).
public class EFDAOFactory : IDAOFactory
{
    private readonly EscuelaDbContext db;

    public EFDAOFactory(EscuelaDbContext db) => this.db = db;

    public IDAOAlumno CrearDAOAlumno() => new EFDAOAlumno(db);
    public IDAODocente CrearDAODocente() => new EFDAODocente(db);
    public IDAODivision CrearDAODivision() => new EFDAODivision(db);
    public IDAOMateria CrearDAOMateria() => new EFDAOMateria(db);
    public IDAOCalificacion CrearDAOCalificacion() => new EFDAOCalificacion(db);
}
