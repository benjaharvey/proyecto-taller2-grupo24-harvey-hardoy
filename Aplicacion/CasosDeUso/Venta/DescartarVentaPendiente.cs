using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class DescartarVentaPendiente
    {
        private readonly IVentaRepositorio _repositorio;

        public DescartarVentaPendiente(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int usuarioId)
        {
            var pendiente = _repositorio.ObtenerPendiente(usuarioId);
            if (pendiente == null)
                return;

            _repositorio.Descartar(pendiente.Id);
        }
    }
}