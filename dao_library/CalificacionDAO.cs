using dao_library.Data;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library;

public class CalificacionDAO
{
    private readonly EscuelaDbContext db;

    public CalificacionDAO(EscuelaDbContext db) => this.db = db;

    public Task<List<Calificacion>> ListarAsync(long? alumnoId = null, long? materiaId = null)
    {
        var query = ConDatos().AsNoTracking();
        if (alumnoId.HasValue) query = query.Where(c => c.AlumnoId == alumnoId);
        if (materiaId.HasValue) query = query.Where(c => c.MateriaId == materiaId);
        return query.OrderByDescending(c => c.Fecha).ThenBy(c => c.Id).ToListAsync();
    }

    public Task<Calificacion?> BuscarAsync(long id) => ConDatos().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Resultado> CrearAsync(Calificacion calificacion)
    {
        if (!await ExistenAlumnoYMateriaAsync(calificacion)) return Resultado.NoEncontrado;
        db.Calificaciones.Add(calificacion);
        await db.SaveChangesAsync();
        await db.Entry(calificacion).Reference(c => c.Alumno).LoadAsync();
        await db.Entry(calificacion).Reference(c => c.Materia).LoadAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> ModificarAsync(long id, Calificacion datos)
    {
        var calificacion = await db.Calificaciones.FindAsync(id);
        if (calificacion is null || !await ExistenAlumnoYMateriaAsync(datos)) return Resultado.NoEncontrado;

        calificacion.Nota = datos.Nota;
        calificacion.Fecha = datos.Fecha;
        calificacion.Tipo = datos.Tipo;
        calificacion.Observacion = datos.Observacion;
        calificacion.AlumnoId = datos.AlumnoId;
        calificacion.MateriaId = datos.MateriaId;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var calificacion = await db.Calificaciones.FindAsync(id);
        if (calificacion is null) return false;
        db.Calificaciones.Remove(calificacion);
        await db.SaveChangesAsync();
        return true;
    }

    private IQueryable<Calificacion> ConDatos() =>
        db.Calificaciones.Include(c => c.Alumno).Include(c => c.Materia);

    private async Task<bool> ExistenAlumnoYMateriaAsync(Calificacion c) =>
        await db.Alumnos.AnyAsync(a => a.Id == c.AlumnoId) &&
        await db.Materias.AnyAsync(m => m.Id == c.MateriaId);
}
