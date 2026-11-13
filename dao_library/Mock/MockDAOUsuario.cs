using dao_library.Interfaces;
using entity_library;

namespace dao_library.Mock;

public class MockDAOUsuario : IDAOUsuario
{
    private readonly MockDatabase db;

    public MockDAOUsuario(MockDatabase db) => this.db = db;

    public Task<Usuario?> BuscarAsync(long id)
    {
        lock (db.Sync) return Task.FromResult(Copia(db.Usuarios.FirstOrDefault(u => u.Id == id)));
    }

    public Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        lock (db.Sync) return Task.FromResult(Copia(db.Usuarios.FirstOrDefault(u => u.EmailNormalizado == normalizado)));
    }

    public Task<Usuario?> CrearAsync(string email, string passwordHash)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        lock (db.Sync)
        {
            if (db.Usuarios.Any(u => u.EmailNormalizado == normalizado)) return Task.FromResult<Usuario?>(null);
            var usuario = new Usuario
            {
                Id = db.SiguienteIdUsuario(), Email = email.Trim(), EmailNormalizado = normalizado, PasswordHash = passwordHash
            };
            db.Usuarios.Add(usuario);
            return Task.FromResult(Copia(usuario));
        }
    }

    public Task<bool> ModificarAsync(long id, string email, string hashVerificado, string? hashNuevo)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        lock (db.Sync)
        {
            var usuario = db.Usuarios.FirstOrDefault(u => u.Id == id && u.PasswordHash == hashVerificado);
            if (usuario is null) return Task.FromResult(false);
            if (db.Usuarios.Any(u => u.Id != id && u.EmailNormalizado == normalizado)) return Task.FromResult(false);
            usuario.Email = email.Trim();
            usuario.EmailNormalizado = normalizado;
            usuario.PasswordHash = hashNuevo ?? hashVerificado;
            return Task.FromResult(true);
        }
    }

    public Task<bool> EliminarAsync(long id, string hashVerificado)
    {
        lock (db.Sync)
            return Task.FromResult(db.Usuarios.RemoveAll(u => u.Id == id && u.PasswordHash == hashVerificado) == 1);
    }

    // Se devuelve una copia para que nadie modifique la cuenta guardada sin pasar por el DAO
    // (igual que AsNoTracking en la versión EF).
    private static Usuario? Copia(Usuario? u) => u is null ? null : new Usuario
    {
        Id = u.Id, Email = u.Email, EmailNormalizado = u.EmailNormalizado, PasswordHash = u.PasswordHash
    };
}
