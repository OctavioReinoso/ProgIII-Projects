# Patrones de diseño

| # | Patrón | Lenguaje | Dónde está | Cómo se ejecuta |
|---|---|---|---|---|
| 1 | Singleton | C# | `patrones_diseno/Singleton` | `dotnet run --project patrones_diseno` |
| 2 | Builder | C# | `patrones_diseno/Builder` | `dotnet run --project patrones_diseno` |
| 3 | Prototype | C# | `patrones_diseno/Prototype` | `dotnet run --project patrones_diseno` |
| 4 | Factory Method | TypeScript | `patrones_ts` | `node patrones_ts/main.ts` (Node 22.18 o superior) |
| 5 | Abstract Factory | C# | `dao_library` + `angularApi/Program.cs` | `dotnet test angularApi.Tests` |

El programa `patrones_diseno` imprime cada comprobación con `[OK]` y se detiene si alguna falla.

---

## 1. Singleton: configuración del sistema

- [x] Constructor privado: `private ConfiguracionSistema() { }`.
- [x] Instancia estática privada: `private static readonly ConfiguracionSistema instancia = new();`.
- [x] Propiedad estática pública `Instance`.
- [x] Dos variables (`configA`, `configB`) y dos módulos (`ModuloInscripciones`, `ModuloReportes`) que usan `Instance`.
- [x] `ReferenceEquals(configA, configB)` da `true`.
- [x] Se cambia `NombreAplicacion` desde `configA` y el cambio se ve desde `configB` y desde los dos módulos.

La clase además es `sealed`, para que nadie herede de ella y cree otra instancia desde una subclase. `static readonly` hace que .NET cree la instancia una sola vez y de forma segura entre hilos.

**1. ¿Qué ocurriría si el constructor fuera público?**
Cualquier parte del programa podría hacer `new ConfiguracionSistema()` y tendríamos varias configuraciones distintas. Si un módulo cambia el nombre de la aplicación en su copia, el otro módulo no se entera y cada uno muestra un nombre diferente.

**2. ¿Qué garantía del patrón estaríamos perdiendo?**
La de **instancia única**: que haya un solo objeto y un único punto de acceso global a él. Sin esa garantía ya no compartimos estado, cada uno tiene el suyo.

---

## 2. Builder: pedido de una tienda

- [x] `Pedido` con cliente, productos y tipo de envío (más un descuento opcional).
- [x] `PedidoBuilder` con métodos que devuelven `this`: `ParaCliente`, `AgregarProducto`, `ConEnvioEstandar`, `ConEnvioExpress` y `ConDescuento`.
- [x] Los productos se agregan de a uno con `AgregarProducto`.
- [x] El envío puede ser estándar (es el valor por defecto) o express.
- [x] `Build()` devuelve el pedido final.
- [x] Se crean dos pedidos distintos sin ningún constructor con muchos parámetros.
- [x] `Build()` valida que haya cliente y al menos un producto.

El constructor de `Pedido` es `internal`, así que la única forma de crear un pedido es pasando por el builder. Además, `Build()` le entrega una copia de la lista: si después se sigue usando el builder, los pedidos ya creados no cambian.

**1. ¿Qué responsabilidad tiene `PedidoBuilder` que no tiene `Pedido`?**
Saber *cómo se arma* un pedido. Guarda el estado intermedio mientras se van agregando datos, aplica los valores por defecto (envío estándar, sin descuento) y valida todo antes de crearlo. `Pedido` sólo representa el resultado: sus datos y el cálculo del total.

**2. ¿Por qué los métodos devuelven `this`?**
Para poder encadenar las llamadas (interfaz fluida): `new PedidoBuilder().ParaCliente("Ana").AgregarProducto(...).ConEnvioExpress().Build()`. Se lee como una oración y no hay que repetir la variable en cada línea.

**3. ¿Qué ventaja tiene `Build()` frente a devolver el objeto directamente después de cada paso?**
Separa la construcción del resultado. Mientras se arma, el pedido no existe; recién en `Build()` se valida todo junto y se crea un objeto completo e inmutable. Si cada paso devolviera un `Pedido`, podrían circular pedidos a medio armar, por ejemplo sin cliente o sin productos.

**4. ¿Qué ocurre si intentamos construir un pedido sin productos?**
`Build()` lanza `InvalidOperationException("El pedido necesita al menos un producto.")` y el pedido no se crea. La demo lo comprueba.

**5. ¿Qué patrón aplicarías si además necesitaras garantizar una instancia compartida de un servicio?**
**Singleton**, como en el ejercicio 1. En la API se hace lo mismo con el contenedor de dependencias: `MockDatabase` se registra con `AddSingleton`.

---

## 3. Prototype: personajes de un videojuego

- [x] `Personaje` implementa `IPrototype<Personaje>`.
- [x] Método `Clonar()`.
- [x] La lista de habilidades se copia correctamente (copia profunda): `copia.Habilidades = new List<string>(Habilidades)`.
- [x] Dos clones creados a partir del mismo prototipo ("Guerrero base").
- [x] Al clon 1 se le cambian la vida y las habilidades, y al clon 2 el nombre y el nivel.
- [x] `ReferenceEquals` muestra que los tres objetos y sus tres listas son distintos.

`MemberwiseClone()` hace una copia superficial: copia bien `Nombre`, `Vida` y `Nivel`, pero la lista quedaría compartida. Por eso se crea una lista nueva.

**1. ¿Qué ocurriría si modificáramos el prototipo después de crear los clones?**
Los clones que ya existen **no cambian**, porque son copias independientes (la demo lo comprueba). Los clones que se creen después sí salen con los valores nuevos del prototipo. Con una copia superficial sería distinto: si al prototipo se le borraran habilidades, también desaparecerían de los clones, porque compartirían la misma lista.

**2. ¿Qué ventajas ofrece Prototype frente a configurar cada personaje desde cero?**
- Se configura un personaje base una sola vez y las variantes salen de copiarlo y cambiar sólo lo distinto.
- Hay menos código repetido y menos riesgo de olvidarse un valor.
- Sirve cuando crear el objeto es costoso, por ejemplo si hubiera que cargar datos o recursos.
- Quien clona no necesita conocer la clase concreta ni cómo se construye: sólo llama a `Clonar()`.

---

## 4. Factory Method: exportador de informes académicos (TypeScript)

- [x] Interfaz `Exportador` con `exportar()` (`exportadores.ts`).
- [x] `ExportadorPDF`, `ExportadorCSV` y `ExportadorExcel`.
- [x] Clase abstracta `CreadorExportador` (`creadores.ts`).
- [x] Factory Method `crearExportador()`.
- [x] Un creador concreto por formato: `CreadorPDF`, `CreadorCSV` y `CreadorExcel`.
- [x] Operación común `generarInforme()`: arma el nombre del archivo, valida y exporta sin saber qué exportador usa.
- [x] Nuevo formato JSON en un archivo aparte (`formato-json.ts`), sin modificar los archivos existentes.

**1. ¿Por qué la clase cliente no crea directamente un `ExportadorPDF`?**
Porque quedaría atada a esa clase concreta: para cambiar de formato o agregar uno nuevo habría que modificar el cliente. Así, el cliente depende sólo de `CreadorExportador` y `Exportador`, que son abstracciones, y recibe el creador que corresponda.

**2. ¿Cuál es el Factory Method en la solución?**
`crearExportador()`, declarado `abstract` en `CreadorExportador` e implementado en cada creador concreto. Cada subclase decide qué exportador devuelve.

**3. ¿Qué diferencia existe entre el producto y el creador?**
- El **producto** (`Exportador` y sus implementaciones) es el que hace el trabajo: sabe escribir el informe en un formato.
- El **creador** (`CreadorExportador` y sus subclases) decide *qué* producto se crea y contiene la lógica común (`generarInforme`) que usa ese producto sin conocer su clase.

**4. ¿Qué clases necesitamos agregar para incorporar un exportador JSON?**
Dos: `ExportadorJSON` (el producto) y `CreadorJSON` (el creador). Ya están en `formato-json.ts`. No hubo que modificar ninguna clase existente, sólo usar el nuevo creador.

**5. ¿Qué ventaja ofrece este diseño frente a un `switch` centralizado?**
Con un `switch`, cada formato nuevo obliga a modificar el mismo método: viola el principio abierto/cerrado, el método crece sin control y es fácil romper los otros formatos. Además, si el `switch` se repite en varios lugares, hay que acordarse de cambiarlos todos. Con Factory Method cada formato vive en sus propias clases y se agrega sin tocar el código que ya funciona.

---

## 5. Abstract Factory: acceso a datos

Se aplicó sobre la API de la escuela. La familia tiene un DAO por entidad, incluido el de las cuentas de usuario del login:

| Consigna | Implementación |
|---|---|
| `DAOUser` | `IDAOUsuario`, más `IDAOAlumno`, `IDAODocente`, `IDAODivision`, `IDAOMateria` e `IDAOCalificacion` (`dao_library/Interfaces/IDAOs.cs`) |
| `DAOFactory` | `IDAOFactory` (`dao_library/Interfaces/IDAOFactory.cs`) |
| `EFDAOFactory` | `dao_library/EntityFramework/EFDAOFactory.cs` |
| `MockDAOFactory` | `dao_library/Mock/MockDAOFactory.cs` |
| `EFDAOUser` | `EFDAOUsuario`, más un EFDAO por cada entidad |
| `MockDAOUser` | `MockDAOUsuario`, más un MockDAO por cada entidad, sobre `MockDatabase` |

- [x] Interfaz `IDAOUsuario` (el `DAOUser`) y las de las demás entidades.
- [x] Interfaz `IDAOFactory` con un método `Crear...` por cada DAO.
- [x] `EFDAOFactory` y `MockDAOFactory` implementan `IDAOFactory`.
- [x] `EFDAOUsuario` y `MockDAOUsuario` (y el resto de los `EFDAO*` y `MockDAO*`) implementan las interfaces.
- [x] La fábrica se registra en `Program.cs` según la configuración `AccesoDatos` (`EF` o `Mock`), y los controladores la reciben por constructor.
- [x] Las dos familias se prueban sin modificar `IDAOFactory`: todas las pruebas de la API (incluidas las de login) corren dos veces, una con EF y otra con Mock (`angularApi.Tests/PruebasPorFamilia.cs`).

Para cambiar de base sin tocar código:

```powershell
dotnet run --project angularApi -- --AccesoDatos=Mock   # en memoria, con datos de ejemplo
dotnet run --project angularApi                          # EF + SQL Server (valor de appsettings.json)
```

Las dos familias aplican las mismas reglas: DNI, legajo y email únicos, cupo de la división, un alumno en una sola división, al menos un docente por materia, y cascadas al borrar. En EF las cascadas las hace la base; en Mock se hacen a mano, actualizando los dos lados de cada relación. El cálculo del boletín (`PromedioMateria.Calcular`) es compartido por ambas.

**1. ¿Por qué `DAOFactory` no debería crear directamente `EFDAOUser`?**
Porque `IDAOFactory` es la abstracción. Si creara `EFDAOAlumno`, quedaría atada a Entity Framework y no podría existir otra familia. La interfaz sólo declara *qué* se crea; cada fábrica concreta decide *cómo*. Gracias a eso la API depende de la interfaz y no de EF.

**2. ¿Qué pasaría si la fábrica `EFDAOFactory` devolviera un exportador de notas PDF y uno de asistencias Excel?**
Se rompería el sentido del patrón. Una Abstract Factory crea una **familia de productos relacionados** que trabajan juntos, en este caso DAOs sobre el mismo origen de datos.
- Los exportadores no tienen nada que ver con el acceso a datos: la fábrica pasaría a tener dos responsabilidades.
- Toda clase que use la fábrica dependería de cosas que no usa.
- Cada familia nueva (Mock, SQL) estaría obligada a implementar también exportadores.

Los exportadores tienen su propia jerarquía (el Factory Method del ejercicio 4). Algo parecido pasaría si una fábrica mezclara familias, por ejemplo devolviendo un DAO de EF y otro Mock: unos datos irían a SQL Server y otros a memoria, y las relaciones entre ellos dejarían de ser coherentes.

**3. ¿Qué clases habría que modificar si además incorporamos acceso por SQL directo?**
Casi ninguna: el cambio es agregar, no modificar.
- **Nuevas:** `SqlDAOFactory : IDAOFactory` y una clase por DAO (`SqlDAOUsuario`, `SqlDAOAlumno`, `SqlDAODocente`, `SqlDAODivision`, `SqlDAOMateria` y `SqlDAOCalificacion`) usando ADO.NET (`SqlConnection`/`SqlCommand`).
- **A modificar:** sólo `Program.cs`, para agregar la opción `AccesoDatos = "Sql"`.
- **Sin cambios:** las interfaces, los controladores (incluido el login) y las familias EF y Mock.

**4. ¿Qué ventaja obtenemos al pasar la fábrica por el constructor?**
- El controlador no sabe ni le importa qué familia usa, así que está desacoplado.
- El cambio de familia se hace en un solo lugar (`Program.cs`).
- Las dependencias quedan explícitas.
- Es fácil de probar: en los tests se usa la misma API con EF y con Mock sin tocar los controladores.
- El contenedor de dependencias maneja el ciclo de vida de cada cosa: `DbContext` por pedido (scoped) y `MockDatabase` única (singleton).
