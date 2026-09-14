using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;

        public CrearUsuario(IUsuarioRepositorio repo)
        {
            _repositorio = repo;
        }

        public void Ejecutar(UsuarioCrearDTO dto)
        {
            var pUsuario = new Usuario
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                SucursalId = dto.SucursalId
            };
            _repositorio.Agregar(pUsuario);
        }
    }
}