using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearProducto
    {
        private readonly IProductoRepositorio _repositorio;

        public CrearProducto(IProductoRepositorio repo)
        {
            _repositorio = repo;
        }

        public void Ejecutar(ProductoCrearDTO dto)
        {
            ValidadorProducto.Validar(dto, _repositorio);

            var NuevoProducto = new Producto
            {
                Nombre = dto.Nombre,
                Precio = dto.Precio,
                CategoriaId = dto.CategoriaId,
                RutaImagen = dto.RutaImagen,
                ProductoInsumos = dto.Receta.Select(r => new ProductoInsumo
                {
                    InsumoId = r.InsumoId,
                    CantidadNecesaria = r.CantidadNecesaria
                }).ToList()
            };
            _repositorio.Agregar(NuevoProducto);
        }
    }
}