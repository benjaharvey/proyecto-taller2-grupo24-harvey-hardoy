using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearRol
    {
        private readonly IRolRepositorio _repositorio;

        public CrearRol(IRolRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(RolDTO dto)
        {
            ValidadorRol.Validar(dto, _repositorio);

            var rol = new Rol
            {
                Nombre = dto.Nombre
            };
            _repositorio.Agregar(rol);
        }
    }
}
