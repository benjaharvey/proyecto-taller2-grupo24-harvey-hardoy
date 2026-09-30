using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarRoles
    {
        private readonly IRolRepositorio _repositorio;

        public ListarRoles(IRolRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<RolDTO> Ejecutar()
        {
            var roles = _repositorio.ObtenerTodos();
            var resultado = new List<RolDTO>();

            foreach (var rol in roles)
            {
                resultado.Add(new RolDTO
                {
                    Id = rol.Id,
                    Nombre = rol.Nombre
                });
            }
            return resultado;
        }
    }
}