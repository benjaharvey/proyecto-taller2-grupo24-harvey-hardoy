using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarProducto
    {
        private readonly IProductoRepositorio _repositorio;

        public EliminarProducto(IProductoRepositorio repo)
        {
           _repositorio = repo;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Eliminar(id);
        }
    }
}