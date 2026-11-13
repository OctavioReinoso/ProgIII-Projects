namespace patrones_diseno.Builder;

// Arma el pedido paso a paso. Cada método devuelve "this" para poder encadenar llamadas.
public class PedidoBuilder
{
    private string? cliente;
    private readonly List<Producto> productos = new();
    private TipoEnvio envio = TipoEnvio.Estandar; // valor por defecto
    private decimal porcentajeDescuento;          // opcional

    public PedidoBuilder ParaCliente(string nombre)
    {
        cliente = nombre;
        return this;
    }

    public PedidoBuilder AgregarProducto(string nombre, decimal precio, int cantidad = 1)
    {
        if (precio <= 0) throw new ArgumentException("El precio debe ser mayor a cero.", nameof(precio));
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
        productos.Add(new Producto(nombre, precio, cantidad));
        return this;
    }

    public PedidoBuilder ConEnvioEstandar()
    {
        envio = TipoEnvio.Estandar;
        return this;
    }

    public PedidoBuilder ConEnvioExpress()
    {
        envio = TipoEnvio.Express;
        return this;
    }

    public PedidoBuilder ConDescuento(decimal porcentaje)
    {
        if (porcentaje is < 0 or > 50) throw new ArgumentException("El descuento debe estar entre 0% y 50%.", nameof(porcentaje));
        porcentajeDescuento = porcentaje;
        return this;
    }

    // Valida todo junto y recién ahí crea el Pedido. Nunca existe un pedido a medio armar.
    public Pedido Build()
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new InvalidOperationException("El pedido necesita un cliente.");
        if (productos.Count == 0)
            throw new InvalidOperationException("El pedido necesita al menos un producto.");

        // Se pasa una copia: si se sigue usando el builder, el pedido ya creado no cambia.
        return new Pedido(cliente, productos.ToList(), envio, porcentajeDescuento);
    }
}
