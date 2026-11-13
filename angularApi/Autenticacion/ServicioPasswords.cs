using Microsoft.Extensions.Options;

namespace angularApi.Autenticacion;

public sealed class ServicioPasswords
{
    private readonly int costo;
    // Hash de relleno: si el email no existe igual se verifica contra algo, así el login tarda lo mismo
    // y no se puede adivinar qué emails están registrados midiendo el tiempo de respuesta.
    private readonly string hashDeRelleno;

    public ServicioPasswords(IOptions<JwtOpciones> opciones)
    {
        costo = opciones.Value.CostoBCrypt;
        hashDeRelleno = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), costo);
    }

    public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password, costo);

    public bool Verificar(string password, string? hash) => BCrypt.Net.BCrypt.Verify(password, hash ?? hashDeRelleno);
}
