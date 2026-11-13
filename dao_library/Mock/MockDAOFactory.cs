using dao_library.Interfaces;
using entity_library;
using entity_library.Enums;

namespace dao_library.Mock;

// Fábrica concreta 2: todos los DAOs comparten la misma MockDatabase en memoria.
public class MockDAOFactory : IDAOFactory
{
    private readonly MockDatabase db;

    public MockDAOFactory(MockDatabase db) => this.db = db;

    public IDAOAlumno CrearDAOAlumno() => new MockDAOAlumno(db);
    public IDAODocente CrearDAODocente() => new MockDAODocente(db);
    public IDAODivision CrearDAODivision() => new MockDAODivision(db);
    public IDAOMateria CrearDAOMateria() => new MockDAOMateria(db);
    public IDAOCalificacion CrearDAOCalificacion() => new MockDAOCalificacion(db);
    public IDAOUsuario CrearDAOUsuario() => new MockDAOUsuario(db);

    // Datos de ejemplo para probar la API sin SQL Server. Se cargan usando la propia fábrica.
    public static MockDatabase CrearConDatosDeEjemplo()
    {
        var db = new MockDatabase();
        var fabrica = new MockDAOFactory(db);

        var division = new Division { Anio = 3, Letra = "A", Turno = Turno.Manana, Cupo = 30 };
        var lucia = new Alumno { Nombre = "Lucía", Apellido = "Fernández", Dni = "47123456", FechaNacimiento = new DateOnly(2010, 4, 12), Legajo = "A-001" };
        var tomas = new Alumno { Nombre = "Tomás", Apellido = "Gómez", Dni = "46222333", FechaNacimiento = new DateOnly(2010, 8, 3), Legajo = "A-002" };
        var jorge = new Docente { Nombre = "Jorge", Apellido = "Benítez", Dni = "30123456", FechaNacimiento = new DateOnly(1980, 9, 1), Titulo = "Profesor de Matemática" };
        var laura = new Docente { Nombre = "Laura", Apellido = "Ríos", Dni = "28111222", FechaNacimiento = new DateOnly(1979, 5, 20), Titulo = "Profesora de Lengua" };
        var matematica = new Materia { Nombre = "Matemática", HorasSemanales = 5 };
        var lengua = new Materia { Nombre = "Lengua", HorasSemanales = 4 };

        fabrica.CrearDAODivision().CrearAsync(division).Wait();
        fabrica.CrearDAOAlumno().CrearAsync(lucia).Wait();
        fabrica.CrearDAOAlumno().CrearAsync(tomas).Wait();
        fabrica.CrearDAODocente().CrearAsync(jorge).Wait();
        fabrica.CrearDAODocente().CrearAsync(laura).Wait();
        // Cada materia nace con al menos un docente.
        fabrica.CrearDAOMateria().CrearAsync(matematica, [jorge.Id]).Wait();
        fabrica.CrearDAOMateria().CrearAsync(lengua, [laura.Id]).Wait();
        fabrica.CrearDAODivision().InscribirAlumnoAsync(division.Id, lucia.Id).Wait();
        fabrica.CrearDAODivision().InscribirAlumnoAsync(division.Id, tomas.Id).Wait();

        var notas = fabrica.CrearDAOCalificacion();
        notas.CrearAsync(new Calificacion { AlumnoId = lucia.Id, MateriaId = matematica.Id, Nota = 8, Fecha = new DateOnly(2026, 6, 10), Tipo = TipoEvaluacion.Escrita }).Wait();
        notas.CrearAsync(new Calificacion { AlumnoId = lucia.Id, MateriaId = lengua.Id, Nota = 9, Fecha = new DateOnly(2026, 6, 12), Tipo = TipoEvaluacion.Oral }).Wait();
        notas.CrearAsync(new Calificacion { AlumnoId = tomas.Id, MateriaId = matematica.Id, Nota = 5, Fecha = new DateOnly(2026, 6, 10), Tipo = TipoEvaluacion.Escrita }).Wait();
        return db;
    }
}
