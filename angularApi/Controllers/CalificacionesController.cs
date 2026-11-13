using angularApi.Dtos;
using dao_library;
using dao_library.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

public class CalificacionesController : EscuelaControllerBase
{
    private readonly IDAOCalificacion calificacionDAO;

    // La fábrica llega por constructor: el controlador no sabe si los datos vienen de EF o del Mock.
    public CalificacionesController(IDAOFactory fabrica) => calificacionDAO = fabrica.CrearDAOCalificacion();

    // GET /Calificaciones?alumnoId=1&materiaId=2 (los dos filtros son opcionales)
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CalificacionResponse>>> Listar([FromQuery] long? alumnoId, [FromQuery] long? materiaId) =>
        (await calificacionDAO.ListarAsync(alumnoId, materiaId)).Select(c => c.AResponse()).ToList();

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CalificacionResponse>> Buscar(long id) =>
        await calificacionDAO.BuscarAsync(id) is { } calificacion ? calificacion.AResponse() : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(CalificacionRequest request)
    {
        var calificacion = request.AEntidad();
        var resultado = await calificacionDAO.CrearAsync(calificacion);
        if (resultado != Resultado.Ok) return Responder(resultado, "El alumno o la materia");
        return CreatedAtAction(nameof(Buscar), new { id = calificacion.Id }, calificacion.AResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, CalificacionRequest request) =>
        Responder(await calificacionDAO.ModificarAsync(id, request.AEntidad()), "La calificación");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id) =>
        await calificacionDAO.EliminarAsync(id) ? NoContent() : NotFound();
}
