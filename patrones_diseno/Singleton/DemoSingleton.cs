namespace patrones_diseno.Singleton;

public static class DemoSingleton
{
    public static void Ejecutar()
    {
        Verificar.Titulo("1. Singleton - ConfiguracionSistema");

        // Dos variables que acceden a Instance.
        var configA = ConfiguracionSistema.Instance;
        var configB = ConfiguracionSistema.Instance;
        Verificar.Que(ReferenceEquals(configA, configB), "configA y configB apuntan al mismo objeto (ReferenceEquals)");

        // Dos módulos distintos también reciben la misma instancia.
        var inscripciones = new ModuloInscripciones();
        var reportes = new ModuloReportes();
        Verificar.Que(ReferenceEquals(inscripciones.Configuracion, reportes.Configuracion),
            "ModuloInscripciones y ModuloReportes comparten la configuración");

        Console.WriteLine($"  Antes:   {inscripciones.Encabezado()} | {reportes.Encabezado()}");

        // Se modifica desde una referencia y el cambio se ve desde todas.
        configA.NombreAplicacion = "Escuela Técnica N° 1";
        Console.WriteLine($"  Después: {inscripciones.Encabezado()} | {reportes.Encabezado()}");
        Verificar.Que(configB.NombreAplicacion == "Escuela Técnica N° 1", "el cambio hecho con configA se ve desde configB");
        Verificar.Que(reportes.Encabezado().Contains("Escuela Técnica N° 1"), "el cambio se ve desde el otro módulo");

        // new ConfiguracionSistema();  <- no compila: el constructor es privado (error CS0122).
    }
}
