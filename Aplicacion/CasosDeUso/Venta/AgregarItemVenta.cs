using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class AgregarItemVenta
    {
        private readonly IVentaRepositorio _ventaRepositorio;
        private readonly IProductoRepositorio _productoRepositorio;
        private readonly IStockSucursalRepositorio _stockRepositorio;

        public AgregarItemVenta(
            IVentaRepositorio ventaRepositorio,
            IProductoRepositorio productoRepositorio,
            IStockSucursalRepositorio stockRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _productoRepositorio = productoRepositorio;
            _stockRepositorio = stockRepositorio;
        }

        public void Ejecutar(int ventaId, int productoId, int cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var venta = _ventaRepositorio.ObtenerPorId(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.PENDIENTE)
                throw new InvalidOperationException(
                    "La venta ya fue confirmada o cancelada y no admite cambios.");

            // ObtenerPorId excluye los dados de baja
            var producto = _productoRepositorio.ObtenerPorId(productoId)
                ?? throw new InvalidOperationException(
                    "El producto no existe o está dado de baja. Recargá el catálogo.");

            // se valida lo que ya hay más lo nuevo.
            var enCarrito = venta.Detalles
                .Where(d => d.ProductoId == productoId)
                .Sum(d => d.Cantidad);

            var enStock = _stockRepositorio
                .ObtenerPorSucursalYProducto(venta.SucursalId, productoId)?.Cantidad ?? 0;

            if (enCarrito + cantidad > enStock)
                throw new InvalidOperationException(
                    $"Stock insuficiente de {producto.Nombre}: hay {enStock} y el carrito quedaría con {enCarrito + cantidad}.");

            // El precio sale de la base y queda congelado en el detalle.
            _ventaRepositorio.AgregarItem(ventaId, productoId, cantidad, producto.Precio);
        }
    }
}