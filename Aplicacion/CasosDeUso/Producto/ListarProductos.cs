using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarProductos
    {
        private readonly IProductoRepositorio _repositorio;

        public ListarProductos(IProductoRepositorio repo)
        {
           _repositorio = repo;
        }

        public List<ProductoDTO> Ejecutar(bool incluirEliminados = false)
        {
            var productos = _repositorio.ObtenerTodos(incluirEliminados);
            var resultado = new List<ProductoDTO>();

            foreach (var producto in productos)
            {
                resultado.Add(new ProductoDTO
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Costo = producto.Costo,
                    RutaImagen = producto.RutaImagen,
                    CategoriaId = producto.CategoriaId,
                    NombreCategoria = producto.Categoria?.Nombre ?? "Sin categoría",
                    CreatedAt = producto.CreatedAt,
                    UpdatedAt = producto.UpdatedAt,
                    DeletedAt = producto.DeletedAt,
                    Receta = producto.ProductoInsumos.Select(pi => new ProductoInsumoDTO
                    {
                        InsumoId = pi.InsumoId,
                        NombreInsumo = pi.Insumo?.Nombre ?? string.Empty,
                        Unidad = pi.Insumo?.Unidad ?? string.Empty,
                        CantidadNecesaria = pi.CantidadNecesaria
                    }).ToList()
                });
            }
            return resultado;
        }
    }
}