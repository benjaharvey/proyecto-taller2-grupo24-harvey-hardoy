using System.Linq;
using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ConsultarStockPorSucursal
    {
        private readonly IStockSucursalRepositorio _repositorio;

        public ConsultarStockPorSucursal(IStockSucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public ConsultaStockDTO Ejecutar(int? sucursalId = null, int? categoriaId = null)
        {
            var stocks = sucursalId.HasValue
                ? _repositorio.ObtenerPorSucursal(sucursalId.Value)
                : _repositorio.ObtenerTodas();

            if (categoriaId.HasValue)
                stocks = stocks.Where(s => s.Producto!.CategoriaId == categoriaId.Value).ToList();

            var resultado = new ConsultaStockDTO
            {
                Detalle = stocks.Select(Mapear).ToList()
            };

            // El consolidado (suma de todas las sucursales por producto)
            // solo corresponde para el administrador
            if (!sucursalId.HasValue)
            {
                resultado.Consolidado = stocks
                    .GroupBy(s => s.ProductoId)
                    .Select(g =>
                    {
                        var primero = g.First();
                        return new StockSucursalDTO
                        {
                            Id = 0,
                            SucursalId = 0,
                            NombreSucursal = "Consolidado",
                            ProductoId = primero.ProductoId,
                            NombreProducto = primero.Producto!.Nombre,
                            CategoriaId = primero.Producto.CategoriaId,
                            NombreCategoria = primero.Producto.Categoria?.Nombre ?? "",
                            Cantidad = g.Sum(s => s.Cantidad),
                            CreatedAt = g.Min(s => s.CreatedAt),
                            UpdatedAt = g.Max(s => s.UpdatedAt)
                        };
                    })
                    .OrderBy(c => c.NombreProducto)
                    .ToList();
            }

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
