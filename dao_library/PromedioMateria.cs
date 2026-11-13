namespace dao_library;

public record NotaDeMateria(long MateriaId, string Materia, decimal Nota);

public record PromedioMateria(long MateriaId, string Materia, int CantidadNotas, decimal Promedio, bool Aprobada)
{
    public const decimal NotaAprobacion = 6m;

    // Lo usan las dos familias de DAO (EF y Mock), así el boletín se calcula igual en ambas.
    public static List<PromedioMateria> Calcular(IEnumerable<NotaDeMateria> notas) => notas
        .GroupBy(n => new { n.MateriaId, n.Materia })
        .Select(g =>
        {
            var promedio = Math.Round(g.Average(n => n.Nota), 2);
            return new PromedioMateria(g.Key.MateriaId, g.Key.Materia, g.Count(), promedio, promedio >= NotaAprobacion);
        })
        .OrderBy(p => p.Materia)
        .ToList();
}
