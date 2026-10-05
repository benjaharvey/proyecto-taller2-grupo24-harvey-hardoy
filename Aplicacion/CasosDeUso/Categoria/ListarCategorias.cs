using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarCategorias
    {
        private readonly ICategoriaRepositorio _repositorio;

        public ListarCategorias(ICategoriaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<CategoriaDTO> Ejecutar()
        {
            var categorias = _repositorio.ObtenerTodos();
            var resultado = new List<CategoriaDTO>();

            foreach (var categoria in categorias)
            {
                resultado.Add(new CategoriaDTO
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre
                });
            }
            return resultado;
        }
    }
}