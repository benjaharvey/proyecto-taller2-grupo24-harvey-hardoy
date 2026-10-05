using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarCategoria
    {
        private readonly ICategoriaRepositorio _repositorio;
        private readonly IProductoRepositorio _repositorioProducto;

        public EliminarCategoria(ICategoriaRepositorio repositorio, IProductoRepositorio productoRepositorio)
        {
            _repositorio = repositorio;
            _repositorioProducto = productoRepositorio;
        }

        public void Ejecutar(int id)
        {
            var categoria = _repositorio.ObtenerPorId(id)
                ?? throw new InvalidOperationException("La categoría que intentás eliminar ya no existe.");

            var asignados = _repositorioProducto.ContarPorCategoria(id);
            
            if (asignados > 0)
            {
                    throw new InvalidOperationException(
                    $"No se puede eliminar la categoría \"{categoria.Nombre}\": tiene {asignados} producto(s) asignado(s). Reasignalos a otra categoría primero.");
            }
            _repositorio.Eliminar(id);
        }
    }
}
