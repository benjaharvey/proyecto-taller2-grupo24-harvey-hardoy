using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarUsuarios
    {
        private readonly IUsuarioRepositorio _repositorioUsuario;
        private readonly IRolRepositorio _repositorioRol;

        public ListarUsuarios(IUsuarioRepositorio repositorioUsuario, IRolRepositorio repositorioRol)
        {
            _repositorioUsuario = repositorioUsuario;
            _repositorioRol = repositorioRol;
        }

        public List<UsuarioDTO> Ejecutar()
        {
            var resultado = new List<UsuarioDTO>();
            foreach (var usuario in _repositorioUsuario.ObtenerTodos())
            {
                var rol = _repositorioRol.ObtenerPorId(usuario.RolId);
                resultado.Add(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    RolId = usuario.RolId,
                    NombreRol = rol?.Nombre ?? "Sin rol",
                    SucursalId = usuario.SucursalId
                });
            }
            return resultado;
        }
    }
}