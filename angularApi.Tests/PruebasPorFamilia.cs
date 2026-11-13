using dao_library.EntityFramework;
using dao_library.Interfaces;
using dao_library.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace angularApi.Tests;

// Cada clase de pruebas es abstracta y se ejecuta una vez por familia de DAOs.
// Las mismas pruebas pasan con EF y con Mock sin tocar IDAOFactory ni los controladores.
public abstract class PruebasApi
{
    protected abstract string AccesoDatos { get; }
    protected ApiDePrueba CrearApi() => new(AccesoDatos);
}

public class ValidacionesEF : ValidacionesTests { protected override string AccesoDatos => "EF"; }
public class ValidacionesMock : ValidacionesTests { protected override string AccesoDatos => "Mock"; }

public class RelacionesEF : RelacionesTests { protected override string AccesoDatos => "EF"; }
public class RelacionesMock : RelacionesTests { protected override string AccesoDatos => "Mock"; }

public class CalificacionesEF : CalificacionesTests { protected override string AccesoDatos => "EF"; }
public class CalificacionesMock : CalificacionesTests { protected override string AccesoDatos => "Mock"; }

public class AutenticacionEF : AutenticacionTests { protected override string AccesoDatos => "EF"; }
public class AutenticacionMock : AutenticacionTests { protected override string AccesoDatos => "Mock"; }

public class MinimoDocentesEF : MinimoDocentesTests { protected override string AccesoDatos => "EF"; }
public class MinimoDocentesMock : MinimoDocentesTests { protected override string AccesoDatos => "Mock"; }

public class FabricaTests
{
    [Theory]
    [InlineData("EF", typeof(EFDAOFactory), typeof(EFDAOAlumno), typeof(EFDAOMateria), typeof(EFDAOUsuario))]
    [InlineData("Mock", typeof(MockDAOFactory), typeof(MockDAOAlumno), typeof(MockDAOMateria), typeof(MockDAOUsuario))]
    public void La_configuracion_elige_una_familia_completa(string acceso, Type fabrica, Type daoAlumno, Type daoMateria, Type daoUsuario)
    {
        using var api = new ApiDePrueba(acceso);
        using var scope = api.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDAOFactory>();

        Assert.IsType(fabrica, factory);
        // Todos los productos salen de la misma familia: nunca se mezcla un DAO de EF con uno Mock.
        Assert.IsType(daoAlumno, factory.CrearDAOAlumno());
        Assert.IsType(daoMateria, factory.CrearDAOMateria());
        Assert.IsType(daoUsuario, factory.CrearDAOUsuario());
    }

    [Fact]
    public async Task Los_datos_de_ejemplo_del_mock_se_cargan_con_la_propia_fabrica()
    {
        var db = MockDAOFactory.CrearConDatosDeEjemplo();
        var fabrica = new MockDAOFactory(db);

        Assert.Equal(2, (await fabrica.CrearDAOAlumno().ListarAsync()).Count);
        var division = Assert.Single(await fabrica.CrearDAODivision().ListarAsync());
        Assert.Equal(2, division.Alumnos.Count);
        var boletin = await fabrica.CrearDAOAlumno().BoletinAsync(db.Alumnos.First(a => a.Legajo == "A-001").Id);
        Assert.Equal(2, boletin!.Count);
        Assert.All(await fabrica.CrearDAOMateria().ListarAsync(), m => Assert.NotEmpty(m.Docentes));
    }
}
