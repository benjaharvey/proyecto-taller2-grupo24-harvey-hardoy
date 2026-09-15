using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarRol
    {
        private readonly IRolRepositorio _repositorio;

        public EliminarRol(IRolRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            _repositorio.Eliminar(id);
        }
    }
}