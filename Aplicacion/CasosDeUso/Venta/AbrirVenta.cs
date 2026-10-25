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
                return pendiente;

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