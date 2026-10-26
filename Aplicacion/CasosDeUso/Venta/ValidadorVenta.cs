using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorVenta
    {
        public static void ValidarParaConfirmar(
            int ventaId,
            int clienteId,
            MetodoPago metodoPago,
            IVentaRepositorio ventaRepositorio,
            IClienteRepositorio clienteRepositorio)
        {
            if (!Enum.IsDefined(metodoPago))
                throw new InvalidOperationException("El método de pago seleccionado no es válido.");

            var venta = ventaRepositorio.ObtenerPorId(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.PENDIENTE)
                throw new InvalidOperationException("La venta ya fue confirmada o cancelada.");

            if (venta.Detalles.Count == 0)
                throw new InvalidOperationException("No se puede confirmar una venta sin ítems.");

            // ObtenerPorId excluye los clientes dados de baja
            if (clienteRepositorio.ObtenerPorId(clienteId) == null)
                throw new InvalidOperationException(
                    "El cliente no existe o está dado de baja. Recargá la lista de clientes.");
        }


        private const string RolAdmin = "Admin";

        public static void ValidarParaCancelar(
            int ventaId,
            int usuarioId,
            IVentaRepositorio ventaRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IRolRepositorio rolRepositorio)
        {
            var usuario = usuarioRepositorio.ObtenerPorId(usuarioId)
                ?? throw new InvalidOperationException("El usuario no existe o está dado de baja.");

            // El nombre del rol se resuelve con el repositorio
            var rol = rolRepositorio.ObtenerPorId(usuario.RolId);
                if (rol?.Nombre != RolAdmin)
                    throw new InvalidOperationException(
                        "Solo un administrador puede cancelar una venta confirmada.");

            var venta = ventaRepositorio.ObtenerPorId(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.CONFIRMADA)
                throw new InvalidOperationException("Solo se puede cancelar una venta confirmada.");
        }
    }
}