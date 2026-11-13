namespace patrones_diseno;

// Imprime cada comprobación y corta el programa si alguna falla.
public static class Verificar
{
    public static void Que(bool condicion, string descripcion)
    {
        if (!condicion) throw new InvalidOperationException($"FALLÓ: {descripcion}");
        Console.WriteLine($"  [OK] {descripcion}");
    }

    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {texto} ===");
    }
}
