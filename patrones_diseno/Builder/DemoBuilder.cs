namespace patrones_diseno.Builder;

public static class DemoBuilder
{
    public static void Ejecutar()
    {
        Verificar.Titulo("2. Builder - Pedidos de la tienda");

        // Pedido 1: varios productos agregados de a uno, envío express y descuento.
        var pedido1 = new PedidoBuilder()
            .ParaCliente("Ana López")
            .AgregarProducto("Cuaderno universitario", 3500m, 3)
            .AgregarProducto("Calculadora científica", 28000m)
            .AgregarProducto("Lapicera azul", 900m, 5)
            .ConEnvioExpress()
            .ConDescuento(10)
            .Build();
        Console.WriteLine(pedido1);

        // Pedido 2: un solo producto, envío estándar por defecto y sin descuento.
        var pedido2 = new PedidoBuilder()
            .ParaCliente("Martín Sosa")
            .AgregarProducto("Mochila", 45000m)
            .Build();
        Console.WriteLine(pedido2);

        Verificar.Que(pedido1.Productos.Count == 3 && pedido1.Envio == TipoEnvio.Express, "pedido 1: 3 productos con envío express");
        Verificar.Que(pedido1.Total == 42700m, "pedido 1: total = 43.000 - 10% + 4.000 = 42.700");
        Verificar.Que(pedido2.Envio == TipoEnvio.Estandar && pedido2.PorcentajeDescuento == 0, "pedido 2: envío estándar y sin descuento");

        // Validaciones de Build().
        var sinProductos = Falla(() => new PedidoBuilder().ParaCliente("Sin compras").Build());
        Verificar.Que(sinProductos == "El pedido necesita al menos un producto.", $"sin productos -> \"{sinProductos}\"");
        var sinCliente = Falla(() => new PedidoBuilder().AgregarProducto("Regla", 1200m).Build());
        Verificar.Que(sinCliente == "El pedido necesita un cliente.", $"sin cliente -> \"{sinCliente}\"");

        // El pedido ya construido no cambia si se sigue usando el mismo builder.
        var builder = new PedidoBuilder().ParaCliente("Carla").AgregarProducto("Goma", 500m);
        var primero = builder.Build();
        builder.AgregarProducto("Tijera", 2500m);
        Verificar.Que(primero.Productos.Count == 1, "un pedido ya construido no se modifica al seguir usando el builder");
    }

    private static string Falla(Action accion)
    {
        try { accion(); return "(no falló)"; }
        catch (InvalidOperationException ex) { return ex.Message; }
    }
}
