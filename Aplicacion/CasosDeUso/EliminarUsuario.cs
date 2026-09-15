using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;

        public EliminarUsuario(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Eliminar(id);
        }
    }
}