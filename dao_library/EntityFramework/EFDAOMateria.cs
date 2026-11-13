using System.Data;
using dao_library.Data;
using dao_library.Interfaces;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library.EntityFramework;

public class EFDAOMateria : IDAOMateria
{
    private readonly EscuelaDbContext db;

    public EFDAOMateria(EscuelaDbContext db) => this.db = db;

    public Task<List<Materia>> ListarAsync() =>
        db.Materias.AsNoTracking().Include(m => m.Docentes).OrderBy(m => m.Nombre).ToListAsync();

    public Task<Materia?> BuscarAsync(long id) =>
        db.Materias.Include(m => m.Docentes).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Resultado> CrearAsync(Materia materia, IReadOnlyCollection<long> docenteIds)
    {
        var ids = docenteIds.Distinct().ToList();
        if (ids.Count == 0) return Resultado.MinimoRequerido;

        // Serializable: nadie puede borrar a uno de estos docentes mientras se crea la materia.
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var docentes = await db.Docentes.Where(d => ids.Contains(d.Id)).ToListAsync();
        if (docentes.Count != ids.Count) return Resultado.NoEncontrado; // no se guarda nada a medias
        if (await NombreEnUsoAsync(materia.Nombre, null)) return Resultado.Duplicado;

        materia.Docentes = docentes;
        db.Materias.Add(materia);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> ModificarAsync(long id, Materia datos)
    {
        var materia = await db.Materias.FindAsync(id);
        if (materia is null) return Resultado.NoEncontrado;
        if (await NombreEnUsoAsync(datos.Nombre, id)) return Resultado.Duplicado;

        materia.Nombre = datos.Nombre;
        materia.HorasSemanales = datos.HorasSemanales;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var materia = await db.Materias.FindAsync(id);
        if (materia is null) return false;
        db.Materias.Remove(materia);
        await db.SaveChangesAsync();
        return true;
    }

    // Muchos a muchos: un docente puede dictar varias materias y una materia tener varios docentes.
    public async Task<Resultado> AsignarDocenteAsync(long id, long docenteId)
    {
        var materia = await BuscarAsync(id);
        var docente = await db.Docentes.FindAsync(docenteId);
        if (materia is null || docente is null) return Resultado.NoEncontrado;
        if (materia.Docentes.All(d => d.Id != docenteId))
        {
            materia.Docentes.Add(docente);
            await db.SaveChangesAsync();
        }
        return Resultado.Ok;
    }

    public async Task<Resultado> QuitarDocenteAsync(long id, long docenteId)
    {
        // Serializable: si dos pedidos quitan a los dos últimos docentes a la vez, uno de los dos falla
        // en lugar de dejar la materia vacía.
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var materia = await BuscarAsync(id);
        if (materia is null || !await db.Docentes.AnyAsync(d => d.Id == docenteId)) return Resultado.NoEncontrado;
        var docente = materia.Docentes.FirstOrDefault(d => d.Id == docenteId);
        if (docente is null) return Resultado.Ok; // ya no estaba: no hay nada que hacer
        if (materia.Docentes.Count <= 1) return Resultado.MinimoRequerido;

        materia.Docentes.Remove(docente);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return Resultado.Ok;
    }

    private Task<bool> NombreEnUsoAsync(string nombre, long? idActual) =>
        db.Materias.AnyAsync(m => m.Nombre == nombre && m.Id != idActual);
}
