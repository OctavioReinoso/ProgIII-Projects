using patrones_diseno.Builder;
using patrones_diseno.Prototype;
using patrones_diseno.Singleton;

Console.OutputEncoding = System.Text.Encoding.UTF8;

DemoSingleton.Ejecutar();
DemoBuilder.Ejecutar();
DemoPrototype.Ejecutar();

Console.WriteLine();
Console.WriteLine("Todas las comprobaciones pasaron.");
Console.WriteLine("Factory Method (TypeScript): node patrones_ts/main.ts");
Console.WriteLine("Abstract Factory: dao_library/Interfaces, dao_library/EntityFramework y dao_library/Mock");
