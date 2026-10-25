using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ConfirmarVenta
    {
        private readonly IVentaRepositorio _ventaRepositorio;
        private readonly IClienteRepositorio _clienteRepositorio;

        public ConfirmarVenta(
            IVentaRepositorio ventaRepositorio,
            IClienteRepositorio clienteRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _clienteRepositorio = clienteRepositorio;
        }

        public void Ejecutar(int ventaId, int clienteId, MetodoPago metodoPago)
        {
            ValidadorVenta.ValidarParaConfirmar(
                ventaId, clienteId, metodoPago, _ventaRepositorio, _clienteRepositorio);

            _ventaRepositorio.Confirmar(ventaId, clienteId, metodoPago);
        }
    }
}