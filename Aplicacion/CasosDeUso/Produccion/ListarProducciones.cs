using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarProducciones
    {
        private readonly IProduccionRepositorio _repositorio;

        public ListarProducciones(IProduccionRepositorio repo)
        {
            _repositorio = repo;
        }

        public List<ProduccionDTO> Ejecutar()
        {
            var producciones = _repositorio.ObtenerTodos();
            var resultado = new List<ProduccionDTO>();

            foreach (var produccion in producciones)
            {
                resultado.Add(new ProduccionDTO
                {
                    Id = produccion.Id,
                    Cantidad = produccion.Cantidad,
                    CreatedAt = produccion.CreatedAt,
                    ProductoId = produccion.ProductoId,
                    NombreProducto = produccion.Producto?.Nombre ?? string.Empty,
                    SucursalId = produccion.SucursalId,
                    NombreSucursal = produccion.Sucursal?.Nombre ?? string.Empty,
                    UsuarioId = produccion.UsuarioId,
                    NombreUsuario = produccion.Usuario != null
                        ? $"{produccion.Usuario.Nombre} {produccion.Usuario.Apellido}"
                        : string.Empty
                });
            }
            return resultado;
        }
    }
}
