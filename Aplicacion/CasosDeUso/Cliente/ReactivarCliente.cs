using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarCliente
    {
        private readonly IClienteRepositorio _repositorio;

        public ReactivarCliente(IClienteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}
