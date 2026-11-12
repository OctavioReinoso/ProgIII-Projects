using angularApi.Dtos;
using dao_library;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

public class DivisionesController : EscuelaControllerBase
{
    private readonly DivisionDAO divisionDAO;

    public DivisionesController(DivisionDAO divisionDAO) => this.divisionDAO = divisionDAO;

    [HttpGet]
    public async Task<ActionResult<List<DivisionResponse>>> Listar() =>
        (await divisionDAO.ListarAsync()).Select(d => d.AResponse()).ToList();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DivisionResponse>> Buscar(long id) =>
        await divisionDAO.BuscarAsync(id) is { } division ? division.AResponse() : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(DivisionRequest request)
    {
        var division = request.AEntidad();
        var resultado = await divisionDAO.CrearAsync(division);
        if (resultado != Resultado.Ok) return Responder(resultado, "una división");
        return CreatedAtAction(nameof(Buscar), new { id = division.Id }, division.AResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, DivisionRequest request) =>
        Responder(await divisionDAO.ModificarAsync(id, request.AEntidad()), "una división");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id) =>
        await divisionDAO.EliminarAsync(id) ? NoContent() : NotFound();

    // Inscribe al alumno en la división (sin cuerpo).
    [HttpPut("{id:long}/alumnos/{alumnoId:long}")]
    public async Task<IActionResult> InscribirAlumno(long id, long alumnoId) =>
        Responder(await divisionDAO.InscribirAlumnoAsync(id, alumnoId), "La división");

    // Saca al alumno de la división, sin borrarlo.
    [HttpDelete("{id:long}/alumnos/{alumnoId:long}")]
    public async Task<IActionResult> QuitarAlumno(long id, long alumnoId) =>
        Responder(await divisionDAO.QuitarAlumnoAsync(id, alumnoId), "La división");
}
