using System.Linq;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerProducto
    {
        private readonly IProductoRepositorio _repositorio;

        public ObtenerProducto(IProductoRepositorio repo)
        {
            _repositorio = repo;
        }

        public ProductoDTO? Ejecutar(int id)
        {
            var producto = _repositorio.ObtenerPorId(id);
            if (producto == null) return null;

            return new ProductoDTO
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
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
            };
        }
    }
}
