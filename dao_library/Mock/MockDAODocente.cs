using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAODocente : IDAODocente
{
    private readonly MockDatabase db;

    public MockDAODocente(MockDatabase db) => this.db = db;

    public Task<List<Docente>> ListarAsync()
    {
        lock (db.Sync)
            return Task.FromResult(db.Docentes.OrderBy(d => d.Apellido).ThenBy(d => d.Nombre).ToList());
    }

    public Task<Docente?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(db.Docentes.FirstOrDefault(d => d.Id == id));
    }

    public Task<Resultado> CrearAsync(Docente docente)
    {
        lock (db.Sync)
        {
            if (DniEnUso(docente.Dni, null)) return Task.FromResult(Resultado.Duplicado);
            docente.Id = db.SiguienteIdPersona();
            db.Docentes.Add(docente);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> ModificarAsync(long id, Docente datos)
    {
        lock (db.Sync)
        {
            var docente = db.Docentes.FirstOrDefault(d => d.Id == id);
            if (docente is null) return Task.FromResult(Resultado.NoEncontrado);
            if (DniEnUso(datos.Dni, id)) return Task.FromResult(Resultado.Duplicado);

            docente.Nombre = datos.Nombre;
            docente.Apellido = datos.Apellido;
            docente.Dni = datos.Dni;
            docente.FechaNacimiento = datos.FechaNacimiento;
            docente.Titulo = datos.Titulo;
            docente.Email = datos.Email;
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<bool> EliminarAsync(long id)
    {
        lock (db.Sync)
        {
            var docente = db.Docentes.FirstOrDefault(d => d.Id == id);
            if (docente is null) return Task.FromResult(false);
            foreach (var materia in docente.Materias) materia.Docentes.Remove(docente);
            db.Docentes.Remove(docente);
            return Task.FromResult(true);
        }
    }

    private bool DniEnUso(string dni, long? idActual) => db.Personas.Any(p => p.Dni == dni && p.Id != idActual);
}
