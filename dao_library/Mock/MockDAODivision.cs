using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAODivision : IDAODivision
{
    private readonly MockDatabase db;

    public MockDAODivision(MockDatabase db) => this.db = db;

    public Task<List<Division>> ListarAsync()
    {
        lock (db.Sync)
            return Task.FromResult(db.Divisiones.OrderBy(d => d.Anio).ThenBy(d => d.Letra).ToList());
    }

    public Task<Division?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(db.Divisiones.FirstOrDefault(d => d.Id == id));
    }

    public Task<Resultado> CrearAsync(Division division)
    {
        lock (db.Sync)
        {
            if (Existe(division, null)) return Task.FromResult(Resultado.Duplicado);
            division.Id = db.SiguienteIdDivision();
            db.Divisiones.Add(division);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> ModificarAsync(long id, Division datos)
    {
        lock (db.Sync)
        {
            var division = db.Divisiones.FirstOrDefault(d => d.Id == id);
            if (division is null) return Task.FromResult(Resultado.NoEncontrado);
            if (Existe(datos, id)) return Task.FromResult(Resultado.Duplicado);
            if (datos.Cupo < division.Alumnos.Count) return Task.FromResult(Resultado.CupoCompleto);

            division.Anio = datos.Anio;
            division.Letra = datos.Letra;
            division.Turno = datos.Turno;
            division.Cupo = datos.Cupo;
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<bool> EliminarAsync(long id)
    {
        lock (db.Sync)
        {
            var division = db.Divisiones.FirstOrDefault(d => d.Id == id);
            if (division is null) return Task.FromResult(false);
            // Igual que OnDelete(SetNull) en EF: los alumnos quedan sin división.
            foreach (var alumno in division.Alumnos)
            {
                alumno.DivisionId = null;
                alumno.Division = null;
            }
            db.Divisiones.Remove(division);
            return Task.FromResult(true);
        }
    }

    public Task<Resultado> InscribirAlumnoAsync(long id, long alumnoId)
    {
        lock (db.Sync)
        {
            var division = db.Divisiones.FirstOrDefault(d => d.Id == id);
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == alumnoId);
            if (division is null || alumno is null) return Task.FromResult(Resultado.NoEncontrado);
            if (alumno.DivisionId == id) return Task.FromResult(Resultado.Ok);
            if (alumno.DivisionId is not null) return Task.FromResult(Resultado.YaAsignado);
            if (division.Alumnos.Count >= division.Cupo) return Task.FromResult(Resultado.CupoCompleto);

            alumno.DivisionId = id;
            alumno.Division = division;
            division.Alumnos.Add(alumno);
            return Task.FromResult(Resultado.Ok);
        }
    }

    public Task<Resultado> QuitarAlumnoAsync(long id, long alumnoId)
    {
        lock (db.Sync)
        {
            var division = db.Divisiones.FirstOrDefault(d => d.Id == id);
            var alumno = db.Alumnos.FirstOrDefault(a => a.Id == alumnoId);
            if (division is null || alumno is null) return Task.FromResult(Resultado.NoEncontrado);
            if (alumno.DivisionId == id)
            {
                division.Alumnos.Remove(alumno);
                alumno.DivisionId = null;
                alumno.Division = null;
            }
            return Task.FromResult(Resultado.Ok);
        }
    }

    private bool Existe(Division division, long? idActual) =>
        db.Divisiones.Any(d => d.Id != idActual && d.Anio == division.Anio &&
            d.Letra == division.Letra && d.Turno == division.Turno);
}
