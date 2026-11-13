using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAOMateria : IDAOMateria
{
    private readonly MockDatabase db;

    public MockDAOMateria(MockDatabase db) => this.db = db;

    public Task<List<Materia>> ListarAsync()
    {
        lock (db.Sync) return Task.FromResult(db.Materias.OrderBy(m => m.Nombre).ToList());
    }

    public Task<Materia?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(db.Materias.FirstOrDefault(m => m.Id == id));
    }

    public Task<Resultado> CrearAsync(Materia materia, IReadOnlyCollection<long> docenteIds)
    {
        var ids = docenteIds.Distinct().ToList();
        if (ids.Count == 0) return Task.FromResult(Resultado.MinimoRequerido);
        lock (db.Sync)
        {
            var docentes = db.Docentes.Where(d => ids.Contains(d.Id)).ToList();
            if (docentes.Count != ids.Count) return Task.FromResult(Resultado.NoEncontrado);
            if (NombreEnUso(materia.Nombre, null)) return Task.FromResult(Resultado.Duplicado);

            materia.Id = db.SiguienteIdMateria();
            foreach (var docente in docentes)
            {
                materia.Docentes.Add(docente);
                docente.Materias.Add(materia);
            }
            db.Materias.Add(materia);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> ModificarAsync(long id, Materia datos)
    {
        lock (db.Sync)
        {
            var materia = db.Materias.FirstOrDefault(m => m.Id == id);
            if (materia is null) return Task.FromResult(Resultado.NoEncontrado);
            if (NombreEnUso(datos.Nombre, id)) return Task.FromResult(Resultado.Duplicado);

            materia.Nombre = datos.Nombre;
            materia.HorasSemanales = datos.HorasSemanales;
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<bool> EliminarAsync(long id)
    {
        lock (db.Sync)
        {
            var materia = db.Materias.FirstOrDefault(m => m.Id == id);
            if (materia is null) return Task.FromResult(false);

            foreach (var docente in materia.Docentes) docente.Materias.Remove(materia);
            foreach (var nota in db.Calificaciones.Where(c => c.MateriaId == id).ToList())
            {
                nota.Alumno?.Calificaciones.Remove(nota);
                db.Calificaciones.Remove(nota);
            }
            db.Materias.Remove(materia);
            return Task.FromResult(true);
        }
    }

    public Task<Resultado> AsignarDocenteAsync(long id, long docenteId)
    {
        lock (db.Sync)
        {
            var materia = db.Materias.FirstOrDefault(m => m.Id == id);
            var docente = db.Docentes.FirstOrDefault(d => d.Id == docenteId);
            if (materia is null || docente is null) return Task.FromResult(Resultado.NoEncontrado);
            if (!materia.Docentes.Contains(docente))
            {
                // En memoria no hay tabla intermedia: hay que actualizar los dos lados.
                materia.Docentes.Add(docente);
                docente.Materias.Add(materia);
            }
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> QuitarDocenteAsync(long id, long docenteId)
    {
        lock (db.Sync)
        {
            var materia = db.Materias.FirstOrDefault(m => m.Id == id);
            var docente = db.Docentes.FirstOrDefault(d => d.Id == docenteId);
            if (materia is null || docente is null) return Task.FromResult(Resultado.NoEncontrado);
            if (!materia.Docentes.Contains(docente)) return Task.FromResult(Resultado.Ok);
            if (materia.Docentes.Count <= 1) return Task.FromResult(Resultado.MinimoRequerido);
            materia.Docentes.Remove(docente);
            docente.Materias.Remove(materia);
            return Task.FromResult(Resultado.Ok);
        }
    }

    private bool NombreEnUso(string nombre, long? idActual) =>
        db.Materias.Any(m => m.Id != idActual && string.Equals(m.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
}
