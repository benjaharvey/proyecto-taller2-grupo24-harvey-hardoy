using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class CancelarVenta
    {
        private readonly IVentaRepositorio _ventaRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IRolRepositorio _rolRepositorio;

        public CancelarVenta(
            IVentaRepositorio ventaRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IRolRepositorio rolRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _rolRepositorio = rolRepositorio;
        }

        public void Ejecutar(int ventaId, int usuarioId)
        {
            ValidadorVenta.ValidarParaCancelar(
                ventaId, usuarioId, _ventaRepositorio, _usuarioRepositorio, _rolRepositorio);

            _ventaRepositorio.Cancelar(ventaId);
        }
    }
}