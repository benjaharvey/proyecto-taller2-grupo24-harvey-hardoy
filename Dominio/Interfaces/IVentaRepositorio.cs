using Dominio.Consultas;
using Dominio.Entidades;
using Dominio.Enums;

namespace Dominio.Interfaces
{
    public interface IVentaRepositorio
    {
        // Venta en curso
        int Abrir(VentaCabecera venta);
        VentaCabecera? ObtenerPendiente(int usuarioId);
        VentaCabecera? ObtenerPorId(int ventaId);

        // Cambios del carrito
        void AgregarItem(int ventaId, int productoId, int cantidad, decimal precioUnit);
        void CambiarCantidad(int detalleId, int cantidad);
        void QuitarItem(int detalleId);

        // Cambios de estado
        void Confirmar(int ventaId, int clienteId, MetodoPago metodoPago);
        void Cancelar(int ventaId);
        void Descartar(int ventaId);

        List<VentaCabecera> ObtenerHistorial(VentaFiltro filtro);
    }
}