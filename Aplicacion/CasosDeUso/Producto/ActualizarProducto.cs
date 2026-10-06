using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarProducto
    {
        private readonly IProductoRepositorio _repositorio;

        public ActualizarProducto(IProductoRepositorio repo)
        {
           _repositorio = repo;
        }

        public void Ejecutar(int id, ProductoCrearDTO dto)
        {
            ValidadorProducto.Validar(dto, _repositorio, id);

            var ActualizarProducto = new Producto
            {
                Id = id,
                Nombre = dto.Nombre,
                Precio = dto.Precio,
                CategoriaId = dto.CategoriaId,
                RutaImagen = dto.RutaImagen,
                ProductoInsumos = dto.Receta.Select(r => new ProductoInsumo
                {
                    ProductoId = id,
                    InsumoId = r.InsumoId,
                    CantidadNecesaria = r.CantidadNecesaria
                }).ToList()
            };
            _repositorio.Actualizar(ActualizarProducto);
        }
    }
}