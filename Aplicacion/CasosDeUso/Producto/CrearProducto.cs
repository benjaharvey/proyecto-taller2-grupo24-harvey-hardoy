using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearProducto
    {
        private readonly IProductoRepositorio _repositorio;
        private readonly IInsumoRepositorio _insumoRepositorio;

        public CrearProducto(IProductoRepositorio repo, IInsumoRepositorio insumoRepo)
        {
            _repositorio = repo;
            _insumoRepositorio = insumoRepo;
        }

        public void Ejecutar(ProductoCrearDTO dto)
        {
            ValidadorProducto.Validar(dto, _repositorio, _insumoRepositorio);

            var NuevoProducto = new Producto
            {
                Nombre = dto.Nombre,
                Precio = dto.Precio,
                Costo = dto.Costo,
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