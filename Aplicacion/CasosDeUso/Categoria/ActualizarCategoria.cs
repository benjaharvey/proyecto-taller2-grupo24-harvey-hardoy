using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarCategoria
    {
        private readonly ICategoriaRepositorio _repositorio;

        public ActualizarCategoria(ICategoriaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id, CategoriaDTO dto)
        {
            ValidadorCategoria.Validar(dto, _repositorio, id);

            _repositorio.Actualizar(new Categoria { Id = id, Nombre = dto.Nombre });
        }
    }
}