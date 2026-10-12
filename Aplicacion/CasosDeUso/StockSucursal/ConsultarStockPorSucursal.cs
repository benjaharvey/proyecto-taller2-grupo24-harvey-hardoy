using System.Collections.Generic;
using System.Linq;
using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ConsultarStockPorSucursal
    {
        private readonly IStockSucursalRepositorio _repositorio;
        private readonly IProductoRepositorio _repositorioProductos;
        private readonly ICategoriaRepositorio _repositorioCategorias;

        public ConsultarStockPorSucursal(
            IStockSucursalRepositorio repositorio,
            IProductoRepositorio repositorioProductos,
            ICategoriaRepositorio repositorioCategorias)
        {
            _repositorio = repositorio;
            _repositorioProductos = repositorioProductos;
            _repositorioCategorias = repositorioCategorias;
        }

        public ConsultaStockDTO Ejecutar(int? sucursalId = null, int? categoriaId = null)
        {
            var productos = _repositorioProductos.ObtenerTodos();
            var categorias = _repositorioCategorias.ObtenerTodos();

            if (categoriaId.HasValue)
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value).ToList();

            // Vista de una sucursal concreta (o del vendedor, que siempre ve la suya):
            // se listan TODOS los productos activos, con 0 para los que no
            // tienen registro de stock en esa sucursal.
            if (sucursalId.HasValue)
            {
                var stocks = _repositorio.ObtenerPorSucursal(sucursalId.Value);

                // En una sucursal hay un único registro por producto (índice único).
                var stocksPorProducto = stocks.ToDictionary(s => s.ProductoId, s => s);

                return new ConsultaStockDTO
                {
                    Detalle = productos
                        .OrderBy(p => p.Nombre)
                        .Select(p =>
                        {
                            var s = stocksPorProducto.GetValueOrDefault(p.Id);
                            return new StockSucursalDTO
                            {
                                Id = s?.Id ?? 0,
                                SucursalId = sucursalId.Value,
                                ProductoId = p.Id,
                                NombreProducto = p.Nombre,
                                CategoriaId = p.CategoriaId,
                                NombreCategoria = categorias.FirstOrDefault(c => c.Id == p.CategoriaId)?.Nombre ?? "",
                                Cantidad = s?.Cantidad ?? 0,
                                CreatedAt = s?.CreatedAt ?? default,
                                UpdatedAt = s?.UpdatedAt
                            };
                        })
                        .ToList()
                };
            }

            // Administrador sin sucursal elegida: detalle real por sucursal
            // (solo los pares que existen) + consolidado de TODOS los productos
            // activos, con 0 para los que no tienen stock en ninguna.
            var stocksTodas = _repositorio.ObtenerTodas();

            var resultado = new ConsultaStockDTO
            {
                Detalle = stocksTodas.Select(Mapear).ToList()
            };

            resultado.Consolidado = productos
                .OrderBy(p => p.Nombre)
                .Select(p => new StockSucursalDTO
                {
                    Id = 0,
                    SucursalId = 0,
                    NombreSucursal = "Consolidado",
                    ProductoId = p.Id,
                    NombreProducto = p.Nombre,
                    CategoriaId = p.CategoriaId,
                    NombreCategoria = categorias.FirstOrDefault(c => c.Id == p.CategoriaId)?.Nombre ?? "",
                    Cantidad = stocksTodas.Where(s => s.ProductoId == p.Id).Sum(s => s.Cantidad),
                    CreatedAt = stocksTodas.Where(s => s.ProductoId == p.Id).Select(s => s.CreatedAt).DefaultIfEmpty().Min(),
                    UpdatedAt = stocksTodas.Where(s => s.ProductoId == p.Id).Select(s => s.UpdatedAt).DefaultIfEmpty().Max()
                })
                .ToList();

            return resultado;
        }

        private static StockSucursalDTO Mapear(StockSucursal stock)
        {
            return new StockSucursalDTO
            {
                Id = stock.Id,
                SucursalId = stock.SucursalId,
                NombreSucursal = stock.Sucursal?.Nombre ?? "",
                ProductoId = stock.ProductoId,
                NombreProducto = stock.Producto?.Nombre ?? "",
                CategoriaId = stock.Producto?.CategoriaId ?? 0,
                NombreCategoria = stock.Producto?.Categoria?.Nombre ?? "",
                Cantidad = stock.Cantidad,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }
    }
}
