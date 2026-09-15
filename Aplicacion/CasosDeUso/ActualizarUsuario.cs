using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;
        public ActualizarUsuario(IUsuarioRepositorio repositorio) => _repositorio = repositorio;

        public void Ejecutar(int id, UsuarioCrearDTO dto)
        {
            var usuario = new Usuario
            {
                Id = id,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaNacimiento = dto.FechaNacimiento,
                Direccion = dto.Direccion,
                RolId = dto.RolId,
                SucursalId = dto.SucursalId
            };
            _repositorio.Actualizar(usuario);
        }
    }
}