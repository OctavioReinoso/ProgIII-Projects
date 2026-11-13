using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAOCalificacion : IDAOCalificacion
{
    private readonly MockDatabase db;

    public MockDAOCalificacion(MockDatabase db) => this.db = db;

    public Task<List<Calificacion>> ListarAsync(long? alumnoId = null, long? materiaId = null)
    {
        lock (db.Sync)
        {
            return Task.FromResult(db.Calificaciones
                .Where(c => alumnoId == null || c.AlumnoId == alumnoId)
                .Where(c => materiaId == null || c.MateriaId == materiaId)
                .OrderByDescending(c => c.Fecha).ThenBy(c => c.Id)
                .ToList());
        }
    }

    public Task<Calificacion?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(db.Calificaciones.FirstOrDefault(c => c.Id == id));
    }

    public Task<Resultado> CrearAsync(Calificacion calificacion)
    {
        lock (db.Sync)
        {
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == calificacion.AlumnoId);
            var materia = db.Materias.FirstOrDefault(m => m.Id == calificacion.MateriaId);
            if (alumno is null || materia is null) return Task.FromResult(Resultado.NoEncontrado);

            calificacion.Id = db.SiguienteIdCalificacion();
            Vincular(calificacion, alumno, materia);
            db.Calificaciones.Add(calificacion);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> ModificarAsync(long id, Calificacion datos)
    {
        lock (db.Sync)
        {
            var calificacion = db.Calificaciones.FirstOrDefault(c => c.Id == id);
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == datos.AlumnoId);
            var materia = db.Materias.FirstOrDefault(m => m.Id == datos.MateriaId);
            if (calificacion is null || alumno is null || materia is null) return Task.FromResult(Resultado.NoEncontrado);

            Desvincular(calificacion);
            calificacion.Nota = datos.Nota;
            calificacion.Fecha = datos.Fecha;
            calificacion.Tipo = datos.Tipo;
            calificacion.Observacion = datos.Observacion;
            Vincular(calificacion, alumno, materia);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<bool> EliminarAsync(long id)
    {
        lock (db.Sync)
        {
            var calificacion = db.Calificaciones.FirstOrDefault(c => c.Id == id);
            if (calificacion is null) return Task.FromResult(false);
            Desvincular(calificacion);
            db.Calificaciones.Remove(calificacion);
            return Task.FromResult(true);
        }
    }

    private static void Vincular(Calificacion c, Alumno alumno, Materia materia)
    {
        c.AlumnoId = alumno.Id;
        c.Alumno = alumno;
        c.MateriaId = materia.Id;
        c.Materia = materia;
        alumno.Calificaciones.Add(c);
        materia.Calificaciones.Add(c);
    }

    private static void Desvincular(Calificacion c)
    {
        c.Alumno?.Calificaciones.Remove(c);
        c.Materia?.Calificaciones.Remove(c);
    }
}
