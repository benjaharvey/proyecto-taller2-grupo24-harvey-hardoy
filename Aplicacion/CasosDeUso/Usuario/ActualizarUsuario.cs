using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IRolRepositorio _rolRepositorio;
        private readonly ISucursalRepositorio _sucursalRepositorio;

        public ActualizarUsuario(IUsuarioRepositorio repositorio, IRolRepositorio rolRepo, ISucursalRepositorio sucursalRepo)
        {
            _repositorio = repositorio;
            _rolRepositorio = rolRepo;
            _sucursalRepositorio = sucursalRepo;
        }

        public void Ejecutar(int id, UsuarioCrearDTO dto)
        {
            ValidadorUsuario.Validar(dto, _repositorio, _rolRepositorio, _sucursalRepositorio, id);

            var usuario = new Usuario
            {
                Id = id,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                Password = string.IsNullOrEmpty(dto.Password) ? "" : BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FechaNacimiento = dto.FechaNacimiento,
                Direccion = dto.Direccion,
                RolId = dto.RolId,
                SucursalId = dto.SucursalId
            };
            _repositorio.Actualizar(usuario);
        }
    }
}