using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;

        public ReactivarUsuario(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}
