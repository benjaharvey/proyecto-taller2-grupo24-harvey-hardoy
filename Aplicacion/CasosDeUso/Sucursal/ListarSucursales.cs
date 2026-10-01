using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarSucursales
    {
        private readonly ISucursalRepositorio _repositorio;

        public ListarSucursales(ISucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<SucursalDTO> Ejecutar(bool incluirEliminadas = false)
        {
            var sucursales = _repositorio.ObtenerTodos(incluirEliminadas);
            var resultado = new List<SucursalDTO>();

            foreach (var s in sucursales)
            {
                resultado.Add(new SucursalDTO
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Ubicacion = s.Ubicacion,
                    Estado = s.Estado
                });
            }

            return resultado;
        }
    }
}
