using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ModificarCantidadItem
    {
        private readonly IVentaRepositorio _ventaRepositorio;
        private readonly IStockSucursalRepositorio _stockRepositorio;

        public ModificarCantidadItem(
            IVentaRepositorio ventaRepositorio,
            IStockSucursalRepositorio stockRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _stockRepositorio = stockRepositorio;
        }

        public void Ejecutar(int ventaId, int detalleId, int cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException(
                    "La cantidad debe ser mayor a cero. Para sacar el producto, quitá el ítem.");

            var venta = _ventaRepositorio.ObtenerPorId(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.PENDIENTE)
                throw new InvalidOperationException(
                    "La venta ya fue confirmada o cancelada y no admite cambios.");

            var detalle = venta.Detalles.FirstOrDefault(d => d.Id == detalleId)
                ?? throw new InvalidOperationException("El ítem no pertenece a la venta.");

            var nombreProducto = detalle.Producto?.Nombre ?? "el producto";

            var enStock = _stockRepositorio
                .ObtenerPorSucursalYProducto(venta.SucursalId, detalle.ProductoId)?.Cantidad ?? 0;

            if (cantidad > enStock)
                throw new InvalidOperationException(
                    $"Stock insuficiente de {nombreProducto}: hay {enStock}.");

            _ventaRepositorio.CambiarCantidad(detalleId, cantidad);
        }
    }
}