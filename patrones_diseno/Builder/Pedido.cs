namespace patrones_diseno.Builder;

public enum TipoEnvio
{
    Estandar,
    Express
}

public record Producto(string Nombre, decimal PrecioUnitario, int Cantidad = 1)
{
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

// El producto final. Es inmutable: una vez construido no se le pueden agregar productos.
public class Pedido
{
    public static readonly IReadOnlyDictionary<TipoEnvio, decimal> CostoEnvio = new Dictionary<TipoEnvio, decimal>
    {
        [TipoEnvio.Estandar] = 1500m,
        [TipoEnvio.Express] = 4000m,
    };

    // internal: sólo PedidoBuilder (mismo proyecto) puede crear pedidos.
    internal Pedido(string cliente, IReadOnlyList<Producto> productos, TipoEnvio envio, decimal porcentajeDescuento)
    {
        Cliente = cliente;
        Productos = productos;
        Envio = envio;
        PorcentajeDescuento = porcentajeDescuento;
    }

    public string Cliente { get; }
    public IReadOnlyList<Producto> Productos { get; }
    public TipoEnvio Envio { get; }
    public decimal PorcentajeDescuento { get; }

    public decimal SubtotalProductos => Productos.Sum(p => p.Subtotal);
    public decimal Descuento => Math.Round(SubtotalProductos * PorcentajeDescuento / 100, 2);
    public decimal Total => SubtotalProductos - Descuento + CostoEnvio[Envio];

    public override string ToString()
    {
        var lineas = Productos.Select(p => $"    - {p.Cantidad} x {p.Nombre} (${p.PrecioUnitario:N2}) = ${p.Subtotal:N2}");
        return $"  Pedido de {Cliente}\n{string.Join("\n", lineas)}\n" +
               $"    Envío {Envio}: ${CostoEnvio[Envio]:N2}" +
               (PorcentajeDescuento > 0 ? $" | Descuento {PorcentajeDescuento}%: -${Descuento:N2}" : "") +
               $"\n    TOTAL: ${Total:N2}";
    }
}
