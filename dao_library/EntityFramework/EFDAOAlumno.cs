using dao_library.Data;
using dao_library.Interfaces;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library.EntityFramework;

public class EFDAOAlumno : IDAOAlumno
{
    private readonly EscuelaDbContext db;

    public EFDAOAlumno(EscuelaDbContext db) => this.db = db;

    public Task<List<Alumno>> ListarAsync(long? divisionId = null)
    {
        var query = db.Alumnos.AsNoTracking().Include(a => a.Division).AsQueryable();
        if (divisionId.HasValue) query = query.Where(a => a.DivisionId == divisionId);
        return query.OrderBy(a => a.Apellido).ThenBy(a => a.Nombre).ToListAsync();
    }

    public Task<Alumno?> BuscarAsync(long id) =>
        db.Alumnos.Include(a => a.Division).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Resultado> CrearAsync(Alumno alumno)
    {
        if (await ExisteDuplicadoAsync(alumno, null)) return Resultado.Duplicado;
        alumno.DivisionId = null; // la división se asigna con PUT /Divisiones/{id}/alumnos/{alumnoId}
        db.Alumnos.Add(alumno);
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<Resultado> ModificarAsync(long id, Alumno datos)
    {
        var alumno = await db.Alumnos.FindAsync(id);
        if (alumno is null) return Resultado.NoEncontrado;
        if (await ExisteDuplicadoAsync(datos, id)) return Resultado.Duplicado;

        alumno.Nombre = datos.Nombre;
        alumno.Apellido = datos.Apellido;
        alumno.Dni = datos.Dni;
        alumno.FechaNacimiento = datos.FechaNacimiento;
        alumno.Legajo = datos.Legajo;
        await db.SaveChangesAsync();
        return Resultado.Ok;
    }

    public async Task<bool> EliminarAsync(long id)
    {
        var alumno = await db.Alumnos.FindAsync(id);
        if (alumno is null) return false;
        db.Alumnos.Remove(alumno);
        await db.SaveChangesAsync();
        return true;
    }

    // Promedio por materia de todas las notas del alumno. Devuelve null si el alumno no existe.
    public async Task<List<PromedioMateria>?> BoletinAsync(long id)
    {
        if (!await db.Alumnos.AnyAsync(a => a.Id == id)) return null;

        var notas = await db.Calificaciones.AsNoTracking()
            .Where(c => c.AlumnoId == id)
            .Select(c => new NotaDeMateria(c.MateriaId, c.Materia!.Nombre, c.Nota))
            .ToListAsync();

        return PromedioMateria.Calcular(notas);
    }

    // El DNI no se puede repetir entre ninguna persona; el legajo, entre alumnos.
    private async Task<bool> ExisteDuplicadoAsync(Alumno alumno, long? idActual) =>
        await db.Personas.AnyAsync(p => p.Id != idActual && p.Dni == alumno.Dni) ||
        await db.Alumnos.AnyAsync(a => a.Id != idActual && a.Legajo == alumno.Legajo);
}
