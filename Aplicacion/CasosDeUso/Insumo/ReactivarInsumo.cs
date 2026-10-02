using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarInsumo
    {
        private readonly IInsumoRepositorio _repositorio;

        public ReactivarInsumo(IInsumoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}
