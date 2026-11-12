using angularApi.Dtos;
using dao_library;
using Microsoft.AspNetCore.Mvc;

namespace angularApi.Controllers;

public class DocentesController : EscuelaControllerBase
{
    private readonly DocenteDAO docenteDAO;

    public DocentesController(DocenteDAO docenteDAO) => this.docenteDAO = docenteDAO;

    [HttpGet]
    public async Task<ActionResult<List<DocenteResponse>>> Listar() =>
        (await docenteDAO.ListarAsync()).Select(d => d.AResponse()).ToList();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DocenteResponse>> Buscar(long id) =>
        await docenteDAO.BuscarAsync(id) is { } docente ? docente.AResponse() : NotFound();

    [HttpPost]
    public async Task<IActionResult> Crear(DocenteRequest request)
    {
        var docente = request.AEntidad();
        var resultado = await docenteDAO.CrearAsync(docente);
        if (resultado != Resultado.Ok) return Responder(resultado, "una persona");
        return CreatedAtAction(nameof(Buscar), new { id = docente.Id }, docente.AResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Modificar(long id, DocenteRequest request) =>
        Responder(await docenteDAO.ModificarAsync(id, request.AEntidad()), "una persona");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(long id) =>
        await docenteDAO.EliminarAsync(id) ? NoContent() : NotFound();
}
