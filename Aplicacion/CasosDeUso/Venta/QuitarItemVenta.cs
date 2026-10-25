using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class QuitarItemVenta
    {
        private readonly IVentaRepositorio _repositorio;

        public QuitarItemVenta(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int ventaId, int detalleId)
        {
            var venta = _repositorio.ObtenerPorId(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.PENDIENTE)
                throw new InvalidOperationException(
                    "La venta ya fue confirmada o cancelada y no admite cambios.");

            // El ítem tiene que ser de esta venta, no de otra.
            if (venta.Detalles.All(d => d.Id != detalleId))
                throw new InvalidOperationException("El ítem no pertenece a la venta.");

            _repositorio.QuitarItem(detalleId);
        }
    }
}