using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarProducto
    {
        private readonly IProductoRepositorio _repositorio;

        public ReactivarProducto(IProductoRepositorio repo)
        {
           _repositorio = repo;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}