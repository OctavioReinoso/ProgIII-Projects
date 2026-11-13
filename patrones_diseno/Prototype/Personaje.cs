namespace patrones_diseno.Prototype;

public interface IPrototype<T>
{
    T Clonar();
}

public class Personaje : IPrototype<Personaje>
{
    public string Nombre { get; set; } = string.Empty;
    public int Vida { get; set; }
    public int Nivel { get; set; }
    public List<string> Habilidades { get; set; } = new();

    // Copia profunda: MemberwiseClone copia los valores (string e int no dan problema),
    // pero la lista es un objeto aparte, así que hay que crear una lista nueva.
    // Si no, el clon y el original compartirían la misma lista de habilidades.
    public Personaje Clonar()
    {
        var copia = (Personaje)MemberwiseClone();
        copia.Habilidades = new List<string>(Habilidades);
        return copia;
    }

    public override string ToString() =>
        $"{Nombre,-18} Vida {Vida,4}  Nivel {Nivel,2}  Habilidades: {string.Join(", ", Habilidades)}";
}
