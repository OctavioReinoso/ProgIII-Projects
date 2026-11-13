namespace patrones_diseno.Prototype;

public static class DemoPrototype
{
    public static void Ejecutar()
    {
        Verificar.Titulo("3. Prototype - Personajes del videojuego");

        // El prototipo se configura una sola vez.
        var guerrero = new Personaje
        {
            Nombre = "Guerrero base",
            Vida = 120,
            Nivel = 1,
            Habilidades = ["Golpe con escudo", "Grito de guerra"],
        };

        var clon1 = guerrero.Clonar();
        var clon2 = guerrero.Clonar();

        // Se modifica sólo el clon 1.
        clon1.Nombre = "Guerrero de fuego";
        clon1.Vida = 150;
        clon1.Habilidades.Add("Espada en llamas");

        clon2.Nombre = "Guerrero de hielo";
        clon2.Nivel = 3;

        Console.WriteLine($"  Prototipo: {guerrero}");
        Console.WriteLine($"  Clon 1:    {clon1}");
        Console.WriteLine($"  Clon 2:    {clon2}");

        Verificar.Que(!ReferenceEquals(guerrero, clon1) && !ReferenceEquals(clon1, clon2), "prototipo, clon 1 y clon 2 son objetos distintos");
        Verificar.Que(!ReferenceEquals(guerrero.Habilidades, clon1.Habilidades) && !ReferenceEquals(clon1.Habilidades, clon2.Habilidades),
            "cada uno tiene su propia lista de habilidades");
        Verificar.Que(guerrero.Vida == 120 && guerrero.Habilidades.Count == 2, "el prototipo no cambió");
        Verificar.Que(clon2.Vida == 120 && !clon2.Habilidades.Contains("Espada en llamas"), "el clon 2 no cambió por modificar el clon 1");

        // Pregunta 1: modificar el prototipo después de clonar no afecta a los clones ya creados.
        guerrero.Vida = 80;
        guerrero.Habilidades.Clear();
        Verificar.Que(clon1.Vida == 150 && clon2.Habilidades.Count == 2, "cambiar el prototipo después no altera los clones existentes");
        var clon3 = guerrero.Clonar();
        Verificar.Que(clon3.Vida == 80 && clon3.Habilidades.Count == 0, "pero los clones nuevos sí salen con los valores nuevos");
    }
}
