namespace patrones_diseno.Singleton;

// sealed: nadie puede heredar y crear "otra" configuración por la puerta de atrás.
public sealed class ConfiguracionSistema
{
    // Instancia estática privada: el CLR la crea una sola vez y de forma thread-safe.
    private static readonly ConfiguracionSistema instancia = new();

    // Constructor privado: desde afuera no se puede hacer new ConfiguracionSistema().
    private ConfiguracionSistema() { }

    // Único punto de acceso.
    public static ConfiguracionSistema Instance => instancia;

    public string NombreAplicacion { get; set; } = "Sistema de Gestión Escolar";
}

// Dos "módulos" distintos de la aplicación que leen la misma configuración.
public class ModuloInscripciones
{
    public ConfiguracionSistema Configuracion { get; } = ConfiguracionSistema.Instance;
    public string Encabezado() => $"[{Configuracion.NombreAplicacion}] Inscripciones";
}

public class ModuloReportes
{
    public ConfiguracionSistema Configuracion { get; } = ConfiguracionSistema.Instance;
    public string Encabezado() => $"[{Configuracion.NombreAplicacion}] Reportes";
}
