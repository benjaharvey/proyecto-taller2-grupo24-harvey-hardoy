using Aplicacion.DTOs;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerStockSinVentas
    {
        private readonly IStockSucursalRepositorio _repositorio;

        public ObtenerStockSinVentas(IStockSucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<StockSinVentaDTO> Ejecutar(
            DateTime desde, DateTime hasta, int? sucursalId = null, int? categoriaId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            var hastaExclusivo = hasta.Date.AddDays(1);

            var items = _repositorio
                .ObtenerConStockSinVentas(desde.Date, hastaExclusivo, sucursalId, categoriaId);

            return items
                .Select(i => new StockSinVentaDTO
                {
                    ProductoId = i.ProductoId,
                    NombreProducto = i.NombreProducto,
                    NombreCategoria = i.NombreCategoria,
                    Stock = i.Stock,
                    Precio = i.Precio,
                    Valorizado = Math.Round(i.Stock * i.Precio, 2),
                    UltimaVenta = i.UltimaVenta,
                    DiasSinVender = i.UltimaVenta.HasValue
                        ? (DateTime.Today - i.UltimaVenta.Value.Date).Days
                        : null,
                    UltimaVentaTexto = i.UltimaVenta.HasValue
                        ? i.UltimaVenta.Value.ToString("dd/MM/yyyy")
                        : "Nunca vendido",
                    VendidasOtrasSucursales = i.VendidasOtrasSucursales
                })
                .ToList();
        }
    }
}
