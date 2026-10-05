using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarSucursal
    {
        private readonly ISucursalRepositorio _repositorio;

        public ReactivarSucursal(ISucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}
