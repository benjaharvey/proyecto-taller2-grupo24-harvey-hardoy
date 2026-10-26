using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class AbrirVenta
    {
        private readonly IVentaRepositorio _repositorio;

        public AbrirVenta(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public VentaCabecera Ejecutar(int usuarioId, int sucursalId)
        {
            // si el vendedor ya tiene una venta en curso, se retoma.
            var pendiente = _repositorio.ObtenerPendiente(usuarioId);

            if (pendiente != null)
            {
                // Si cambió de sucursal, el carrito viejo no sirve: los precios
                // y el stock pertenecen a otra sucursal. Se descarta y se abre otra.
                if (pendiente.SucursalId == sucursalId)
                    return pendiente;

                _repositorio.Descartar(pendiente.Id);
            }

            var venta = new VentaCabecera
            {
                UsuarioId = usuarioId,
                SucursalId = sucursalId
            };

            try
            {
                _repositorio.Abrir(venta);
                return venta;
            }
            catch (Exception)
            {
                // por si se abrio la venta en otra terminal pero con el mismo usuario
                var existente = _repositorio.ObtenerPendiente(usuarioId);
                if (existente == null)
                    throw;

                return existente;
            }
        }
    }
}