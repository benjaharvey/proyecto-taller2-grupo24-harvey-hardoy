using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarRol
    {
        private readonly IRolRepositorio _repositorio;

        public ActualizarRol(IRolRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id, RolDTO dto)
        {
            _repositorio.Actualizar(new Rol { Id = id, Nombre = dto.Nombre });
        }
    }
}