namespace dao_library.Interfaces;

// Fábrica abstracta: crea una familia completa de DAOs que trabajan sobre el mismo origen de datos.
// La API sólo conoce esta interfaz; qué familia se usa se decide en Program.cs.
public interface IDAOFactory
{
    IDAOAlumno CrearDAOAlumno();
    IDAODocente CrearDAODocente();
    IDAODivision CrearDAODivision();
    IDAOMateria CrearDAOMateria();
    IDAOCalificacion CrearDAOCalificacion();
    IDAOUsuario CrearDAOUsuario();
}
