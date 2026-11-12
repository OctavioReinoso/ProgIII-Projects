using dao_library.Data;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library;

public class DocenteDAO
{
    private readonly EscuelaDbContext db;

    public DocenteDAO(EscuelaDbContext db) => this.db = db;

    public Task<List<Docente>> ListarAsync() =>
        db.Docentes.AsNoTracking().Include(d => d.Materias)
            .OrderBy(d => d.Apellido).ThenBy(d => d.Nombre).ToListAsync();

    public Task<Docente?> BuscarAsync(long id) =>
        db.Docentes.Include(d => d.Materias).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Resultado> CrearAsync(Docente docente)
    {
        if (await DniEnUsoAsync(docente.Dni, null)) return Resultado.Duplicado;
        db.Docentes.Add(docente);
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> ModificarAsync(long id, Docente datos)
    {
        var docente = await db.Docentes.FindAsync(id);
        if (docente is null) return Resultado.NoEncontrado;
        if (await DniEnUsoAsync(datos.Dni, id)) return Resultado.Duplicado;

        docente.Nombre = datos.Nombre;
        docente.Apellido = datos.Apellido;
        docente.Dni = datos.Dni;
        docente.FechaNacimiento = datos.FechaNacimiento;
        docente.Titulo = datos.Titulo;
        docente.Email = datos.Email;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var docente = await db.Docentes.FindAsync(id);
        if (docente is null) return false;
        db.Docentes.Remove(docente);
        await db.SaveChangesAsync();
        return true;
    }

    private Task<bool> DniEnUsoAsync(string dni, long? idActual) =>
        db.Personas.AnyAsync(p => p.Dni == dni && p.Id != idActual);
}
