using dao_library;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

// Traduce los resultados de los DAO a respuestas HTTP con mensajes claros.
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class EscuelaControllerBase : ControllerBase
{
    protected IActionResult Responder(Resultado resultado, string entidad) => resultado switch
    {
        Resultado.Ok => NoContent(),
        Resultado.NoEncontrado => Problema(StatusCodes.Status404NotFound, $"{entidad} o un registro relacionado no existe."),
        Resultado.Duplicado => Problema(StatusCodes.Status409Conflict, $"Ya existe {entidad} con esos datos."),
        Resultado.YaAsignado => Problema(StatusCodes.Status409Conflict,
            "El alumno ya está inscripto en otra división. Quitalo de esa división antes de moverlo."),
        Resultado.CupoCompleto => Problema(StatusCodes.Status409Conflict, "La división no tiene cupo disponible."),
        _ => Problema(StatusCodes.Status500InternalServerError, "Resultado desconocido.")
    };

    protected ObjectResult Problema(int status, string detalle) =>
        Problem(statusCode: status, detail: detalle);
}
