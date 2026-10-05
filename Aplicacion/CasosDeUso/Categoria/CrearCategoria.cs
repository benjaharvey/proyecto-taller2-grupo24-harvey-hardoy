using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearCategoria
    {
        private readonly ICategoriaRepositorio _repositorio;

        public CrearCategoria(ICategoriaRepositorio repo)
        {
            _repositorio = repo;
        }

        public void Ejecutar(CategoriaDTO dto)
        {
            ValidadorCategoria.Validar(dto, _repositorio);

            var NuevaCategoria = new Categoria
            {
                Nombre = dto.Nombre,
            };
            _repositorio.Agregar(NuevaCategoria);
        }
    }
}