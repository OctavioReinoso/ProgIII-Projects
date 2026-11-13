using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAOAlumno : IDAOAlumno
{
    private readonly MockDatabase db;

    public MockDAOAlumno(MockDatabase db) => this.db = db;

    public Task<List<Alumno>> ListarAsync(long? divisionId = null)
    {
        lock (db.Sync)
        {
            return Task.FromResult(db.Alumnos
                .Where(a => divisionId == null || a.DivisionId == divisionId)
                .OrderBy(a => a.Apellido).ThenBy(a => a.Nombre)
                .ToList());
        }
    }

    public Task<Alumno?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(db.Alumnos.FirstOrDefault(a => a.Id == id));
    }

    public Task<Resultado> CrearAsync(Alumno alumno)
    {
        lock (db.Sync)
        {
            if (ExisteDuplicado(alumno, null)) return Task.FromResult(Resultado.Duplicado);
            alumno.Id = db.SiguienteIdPersona();
            alumno.DivisionId = null;
            alumno.Division = null;
            db.Alumnos.Add(alumno);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> ModificarAsync(long id, Alumno datos)
    {
        lock (db.Sync)
        {
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == id);
            if (alumno is null) return Task.FromResult(Resultado.NoEncontrado);
            if (ExisteDuplicado(datos, id)) return Task.FromResult(Resultado.Duplicado);

            alumno.Nombre = datos.Nombre;
            alumno.Apellido = datos.Apellido;
            alumno.Dni = datos.Dni;
            alumno.FechaNacimiento = datos.FechaNacimiento;
            alumno.Legajo = datos.Legajo;
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<bool> EliminarAsync(long id)
    {
        lock (db.Sync)
        {
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == id);
            if (alumno is null) return Task.FromResult(false);

            // Mismo comportamiento que la base: sale de su división y se borran sus notas.
            alumno.Division?.Alumnos.Remove(alumno);
            foreach (var nota in db.Calificaciones.Where(c => c.AlumnoId == id).ToList())
            {
                nota.Materia?.Calificaciones.Remove(nota);
                db.Calificaciones.Remove(nota);
            }
            db.Alumnos.Remove(alumno);
            return Task.FromResult(true);
        }
    }

    public Task<List<PromedioMateria>?> BoletinAsync(long id)
    {
        lock (db.Sync)
        {
            if (db.Alumnos.All(a => a.Id != id)) return Task.FromResult<List<PromedioMateria>?>(null);
            var notas = db.Calificaciones.Where(c => c.AlumnoId == id)
                .Select(c => new NotaDeMateria(c.MateriaId, c.Materia!.Nombre, c.Nota));
            return Task.FromResult<List<PromedioMateria>?>(PromedioMateria.Calcular(notas));
        }
    }

    private bool ExisteDuplicado(Alumno alumno, long? idActual) =>
        db.Personas.Any(p => p.Id != idActual && p.Dni == alumno.Dni) ||
        db.Alumnos.Any(a => a.Id != idActual && string.Equals(a.Legajo, alumno.Legajo, StringComparison.OrdinalIgnoreCase));
}
