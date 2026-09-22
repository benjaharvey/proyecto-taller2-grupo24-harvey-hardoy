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
            ValidadorUsuario.Validar(dto, _repositorio);

            var pUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaNacimiento = dto.FechaNacimiento,
                Direccion = dto.Direccion,
                RolId = dto.RolId,
                SucursalId = dto.SucursalId
            };
            _repositorio.Agregar(pUsuario);
        }
    }
}