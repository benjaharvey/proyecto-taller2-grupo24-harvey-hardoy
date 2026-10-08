using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class EliminarCliente
    {
        private readonly IClienteRepositorio _repositorio;

        public EliminarCliente(IClienteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Eliminar(id);
        }
    }
}
