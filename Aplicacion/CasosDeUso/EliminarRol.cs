using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class EliminarRol
    {
        private readonly IRolRepositorio _repositorio;
        private readonly IUsuarioRepositorio _repositorioUsuario;

        public EliminarRol(IRolRepositorio repositorio, IUsuarioRepositorio repositorioUsuario)
        {
            _repositorio = repositorio;
            _repositorioUsuario = repositorioUsuario;
        }

        public void Ejecutar(int id)
        {
            var rol = _repositorio.ObtenerPorId(id)
                ?? throw new InvalidOperationException("El rol que intentás eliminar ya no existe.");

            if (ValidadorRol.EsDelSistema(rol.Nombre))
                throw new InvalidOperationException($"\"{rol.Nombre}\" es un rol del sistema y no se puede eliminar.");

            // Sin esta guarda los usuarios quedarían apuntando a un rol borrado y se quedarían sin permisos.
            var asignados = _repositorioUsuario.ContarPorRol(id);
            if (asignados > 0)
                throw new InvalidOperationException(
                    $"No se puede eliminar el rol \"{rol.Nombre}\": tiene {asignados} usuario(s) asignado(s). Reasignalos a otro rol primero.");

            _repositorio.Eliminar(id);
        }
    }
}
