using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarUsuarios
    {
        private readonly IUsuarioRepositorio _repositorioUsuario;
        private readonly IRolRepositorio _repositorioRol;
        private readonly ISucursalRepositorio _repositorioSucursal;

        public ListarUsuarios(IUsuarioRepositorio repositorioUsuario, IRolRepositorio repositorioRol, ISucursalRepositorio repositorioSucursal)
        {
            _repositorioUsuario = repositorioUsuario;
            _repositorioRol = repositorioRol;
            _repositorioSucursal = repositorioSucursal;
        }

        public List<UsuarioDTO> Ejecutar()
        {
            var resultado = new List<UsuarioDTO>();
            foreach (var usuario in _repositorioUsuario.ObtenerTodos())
            {
                var rol = _repositorioRol.ObtenerPorId(usuario.RolId);
                var sucursal = _repositorioSucursal.ObtenerPorId(usuario.SucursalId);
                resultado.Add(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Dni = usuario.Dni,
                    Email = usuario.Email,
                    FechaNacimiento = usuario.FechaNacimiento,
                    Direccion = usuario.Direccion,
                    RolId = usuario.RolId,
                    NombreRol = rol?.Nombre ?? "Sin rol",
                    SucursalId = usuario.SucursalId,
                    NombreSucursal = sucursal?.Nombre ?? "Sin sucursal",
                    Activo = usuario.DeletedAt == null,
                    DeletedAt = usuario.DeletedAt
                });
            }
            return resultado;
        }
    }
}