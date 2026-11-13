using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace angularApi.Filtros;

// Red de seguridad: si dos pedidos simultáneos violan un índice único o una FK,
// SQL Server lanza una excepción. En vez de un 500 devolvemos 409 Conflict.
public sealed class ErroresBaseDeDatosFilter : IExceptionFilter
{
    // 2601 y 2627: clave duplicada. 547: violación de FK.
    // 1205: dos transacciones serializables se bloquearon entre sí y SQL Server canceló una.
    private static readonly int[] CodigosConflicto = [2601, 2627, 547, 1205];
    private readonly ILogger<ErroresBaseDeDatosFilter> logger;

    public ErroresBaseDeDatosFilter(ILogger<ErroresBaseDeDatosFilter> logger) => this.logger = logger;

    public void OnException(ExceptionContext context)
    {
        var sql = context.Exception switch
        {
            DbUpdateException { InnerException: SqlException interna } => interna,
            SqlException directa => directa, // ExecuteUpdate/transacciones la lanzan sin envolver
            _ => null
        };
        if (sql is null || !CodigosConflicto.Contains(sql.Number)) return;

        logger.LogWarning(context.Exception, "Conflicto al guardar en la base de datos.");
        context.Result = new ConflictObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "No se pudieron guardar los cambios.",
            Detail = "Los datos están duplicados, hacen referencia a un registro que ya no existe o chocaron con otra operación simultánea. Volvé a intentarlo."
        });
        context.ExceptionHandled = true;
    }
}
