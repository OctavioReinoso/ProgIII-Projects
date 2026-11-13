using dao_library.Data;
using dao_library.Interfaces;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library.EntityFramework;

public class EFDAODivision : IDAODivision
{
    private readonly EscuelaDbContext db;

    public EFDAODivision(EscuelaDbContext db) => this.db = db;

    public Task<List<Division>> ListarAsync() =>
        db.Divisiones.AsNoTracking().Include(d => d.Alumnos)
            .OrderBy(d => d.Anio).ThenBy(d => d.Letra).ToListAsync();

    public Task<Division?> BuscarAsync(long id) =>
        db.Divisiones.Include(d => d.Alumnos).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Resultado> CrearAsync(Division division)
    {
        if (await ExisteAsync(division, null)) return Resultado.Duplicado;
        db.Divisiones.Add(division);
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> ModificarAsync(long id, Division datos)
    {
        var division = await BuscarAsync(id);
        if (division is null) return Resultado.NoEncontrado;
        if (await ExisteAsync(datos, id)) return Resultado.Duplicado;
        if (datos.Cupo < division.Alumnos.Count) return Resultado.CupoCompleto;

        division.Anio = datos.Anio;
        division.Letra = datos.Letra;
        division.Turno = datos.Turno;
        division.Cupo = datos.Cupo;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var division = await BuscarAsync(id);
        if (division is null) return false;
        db.Divisiones.Remove(division); // los alumnos quedan con DivisionId = null
        await db.SaveChangesAsync();
        return true;
    }

    // Un alumno puede estar en una sola división y la división no puede superar su cupo.
    public async Task<Resultado> InscribirAlumnoAsync(long id, long alumnoId)
    {
        var division = await BuscarAsync(id);
        var alumno = await db.Alumnos.FindAsync(alumnoId);
        if (division is null || alumno is null) return Resultado.NoEncontrado;
        if (alumno.DivisionId == id) return Resultado.Ok;
        if (alumno.DivisionId is not null) return Resultado.YaAsignado;
        if (division.Alumnos.Count >= division.Cupo) return Resultado.CupoCompleto;

        alumno.DivisionId = id;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> QuitarAlumnoAsync(long id, long alumnoId)
    {
        var division = await db.Divisiones.FindAsync(id);
        var alumno = await db.Alumnos.FindAsync(alumnoId);
        if (division is null || alumno is null) return Resultado.NoEncontrado;
        if (alumno.DivisionId == id)
        {
            alumno.DivisionId = null;
            await db.SaveChangesAsync();
        }
        return Resultado.Ok;
    }

    private Task<bool> ExisteAsync(Division division, long? idActual) =>
        db.Divisiones.AnyAsync(d => d.Id != idActual && d.Anio == division.Anio &&
            d.Letra == division.Letra && d.Turno == division.Turno);
}
