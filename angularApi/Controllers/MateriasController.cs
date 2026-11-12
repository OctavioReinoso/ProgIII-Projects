using angularApi.Dtos;
using dao_library;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

public class MateriasController : EscuelaControllerBase
{
    private readonly MateriaDAO materiaDAO;

    public MateriasController(MateriaDAO materiaDAO) => this.materiaDAO = materiaDAO;

    [HttpGet]
    public async Task<ActionResult<List<MateriaResponse>>> Listar() =>
        (await materiaDAO.ListarAsync()).Select(m => m.AResponse()).ToList();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MateriaResponse>> Buscar(long id) =>
        await materiaDAO.BuscarAsync(id) is { } materia ? materia.AResponse() : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(MateriaRequest request)
    {
        var materia = request.AEntidad();
        var resultado = await materiaDAO.CrearAsync(materia);
        if (resultado != Resultado.Ok) return Responder(resultado, "una materia");
        return CreatedAtAction(nameof(Buscar), new { id = materia.Id }, materia.AResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, MateriaRequest request) =>
        Responder(await materiaDAO.ModificarAsync(id, request.AEntidad()), "una materia");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id) =>
        await materiaDAO.EliminarAsync(id) ? NoContent() : NotFound();

    [HttpPut("{id:long}/docentes/{docenteId:long}")]
    public async Task<IActionResult> AsignarDocente(long id, long docenteId) =>
        Responder(await materiaDAO.AsignarDocenteAsync(id, docenteId), "La materia");

    [HttpDelete("{id:long}/docentes/{docenteId:long}")]
    public async Task<IActionResult> QuitarDocente(long id, long docenteId) =>
        Responder(await materiaDAO.QuitarDocenteAsync(id, docenteId), "La materia");
}
