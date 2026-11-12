using dao_library.Data;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library;

public class MateriaDAO
{
    private readonly EscuelaDbContext db;

    public MateriaDAO(EscuelaDbContext db) => this.db = db;

    public Task<List<Materia>> ListarAsync() =>
        db.Materias.AsNoTracking().Include(m => m.Docentes).OrderBy(m => m.Nombre).ToListAsync();

    public Task<Materia?> BuscarAsync(long id) =>
        db.Materias.Include(m => m.Docentes).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Resultado> CrearAsync(Materia materia)
    {
        if (await NombreEnUsoAsync(materia.Nombre, null)) return Resultado.Duplicado;
        db.Materias.Add(materia);
        await db.SaveChangesAsync();
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
        var materia = await BuscarAsync(id);
        if (materia is null || !await db.Docentes.AnyAsync(d => d.Id == docenteId)) return Resultado.NoEncontrado;
        var docente = materia.Docentes.FirstOrDefault(d => d.Id == docenteId);
        if (docente is not null)
        {
            materia.Docentes.Remove(docente);
            await db.SaveChangesAsync();
        }
        return Resultado.Ok;
    }

    private Task<bool> NombreEnUsoAsync(string nombre, long? idActual) =>
        db.Materias.AnyAsync(m => m.Nombre == nombre && m.Id != idActual);
}
