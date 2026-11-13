using dao_library.Data;
using dao_library.Interfaces;
using entity_library;
using Microsoft.EntityFrameworkCore;

namespace dao_library.EntityFramework;

public class EFDAOUsuario : IDAOUsuario
{
    private readonly EscuelaDbContext db;

    public EFDAOUsuario(EscuelaDbContext db) => this.db = db;

    public Task<Usuario?> BuscarAsync(long id) =>
        db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        return db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.EmailNormalizado == normalizado);
    }

    public async Task<Usuario?> CrearAsync(string email, string passwordHash)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        if (await db.Usuarios.AnyAsync(u => u.EmailNormalizado == normalizado)) return null;

        var usuario = new Usuario { Email = email.Trim(), EmailNormalizado = normalizado, PasswordHash = passwordHash };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(); // si dos registros llegan juntos, el índice único lo frena (409 en el filtro)
        return usuario;
    }

    // El WHERE incluye el hash verificado: si cambió entre la verificación y la escritura, no se toca nada.
    public async Task<bool> ModificarAsync(long id, string email, string hashVerificado, string? hashNuevo)
    {
        var email_ = email.Trim();
        var normalizado = Usuario.NormalizarEmail(email);
        var hash = hashNuevo ?? hashVerificado;
        var filas = await db.Usuarios
            .Where(u => u.Id == id && u.PasswordHash == hashVerificado)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Email, email_)
                .SetProperty(u => u.EmailNormalizado, normalizado)
                .SetProperty(u => u.PasswordHash, hash));
        return filas == 1;
    }

    public async Task<bool> EliminarAsync(long id, string hashVerificado) =>
        await db.Usuarios.Where(u => u.Id == id && u.PasswordHash == hashVerificado).ExecuteDeleteAsync() == 1;
}
