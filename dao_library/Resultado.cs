namespace dao_library;

// Resultado de las operaciones que pueden fallar por reglas de negocio.
public enum Resultado
{
    Ok,
    NoEncontrado,
    Duplicado,
    YaAsignado,
    CupoCompleto,
    MinimoRequerido
}
