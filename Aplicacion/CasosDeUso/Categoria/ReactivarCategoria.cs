using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ReactivarCategoria
    {
        private readonly ICategoriaRepositorio _repositorio;

        public ReactivarCategoria(ICategoriaRepositorio repo)
        {
           _repositorio = repo;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Reactivar(id);
        }
    }
}