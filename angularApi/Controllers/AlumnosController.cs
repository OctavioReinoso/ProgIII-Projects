using angularApi.Dtos;
using dao_library;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

public class AlumnosController : EscuelaControllerBase
{
    private readonly AlumnoDAO alumnoDAO;

    public AlumnosController(AlumnoDAO alumnoDAO) => this.alumnoDAO = alumnoDAO;

    // GET /Alumnos?divisionId=1
    [HttpGet]
    public async Task<ActionResult<List<AlumnoResponse>>> Listar([FromQuery] long? divisionId) =>
        (await alumnoDAO.ListarAsync(divisionId)).Select(a => a.AResponse()).ToList();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AlumnoResponse>> Buscar(long id) =>
        await alumnoDAO.BuscarAsync(id) is { } alumno ? alumno.AResponse() : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(AlumnoRequest request)
    {
        var alumno = request.AEntidad();
        var resultado = await alumnoDAO.CrearAsync(alumno);
        if (resultado != Resultado.Ok) return Responder(resultado, "un alumno");
        return CreatedAtAction(nameof(Buscar), new { id = alumno.Id }, alumno.AResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, AlumnoRequest request) =>
        Responder(await alumnoDAO.ModificarAsync(id, request.AEntidad()), "un alumno");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id) =>
        await alumnoDAO.EliminarAsync(id) ? NoContent() : NotFound();

    // Promedio por materia y promedio general del alumno.
    [HttpGet("{id:long}/boletin")]
    public async Task<ActionResult<BoletinResponse>> Boletin(long id)
    {
        var alumno = await alumnoDAO.BuscarAsync(id);
        var materias = await alumnoDAO.BoletinAsync(id);
        if (alumno is null || materias is null) return NotFound();

        decimal? general = materias.Count > 0 ? Math.Round(materias.Average(m => m.Promedio), 2) : null;
        return new BoletinResponse(alumno.AResumen(), alumno.Division?.AResumen(), materias, general);
    }
}
