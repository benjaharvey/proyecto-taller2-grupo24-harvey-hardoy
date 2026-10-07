using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarUsuario
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IRolRepositorio _repositorioRol;

        public EliminarUsuario(IUsuarioRepositorio repositorio, IRolRepositorio repositorioRol)
        {
            _repositorio = repositorio;
            _repositorioRol = repositorioRol;
        }

        public void Ejecutar(int id, int idUsuarioEnSesion)
        {
            if (id == idUsuarioEnSesion)
            {
                throw new InvalidOperationException("No podés dar de baja tu propio usuario.");
            }

            var usuario = _repositorio.ObtenerPorId(id)
                ?? throw new InvalidOperationException("El usuario que intentás eliminar no existe o ya fue dado de baja.");

            var rol = _repositorioRol.ObtenerPorId(usuario.RolId);
            bool esAdmin = rol != null && (rol.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                           rol.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

            if (esAdmin && _repositorio.ContarPorRol(usuario.RolId) <= 1)
            {
                throw new InvalidOperationException("No se puede dar de baja al único Administrador activo del sistema.");
            }

            _repositorio.Eliminar(id);
        }
    }
}