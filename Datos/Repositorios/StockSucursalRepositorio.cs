using Microsoft.EntityFrameworkCore;
using Dominio.Consultas;
using Dominio.Entidades;
using Dominio.Enums;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class StockSucursalRepositorio : IStockSucursalRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public StockSucursalRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<StockSucursal> ObtenerPorSucursal(int sucursalId, bool soloActivos = true)
        {
            using var context = _contextFactory.CreateDbContext();
            return Query(context, soloActivos)
                .Where(s => s.SucursalId == sucursalId)
                .OrderBy(s => s.Producto!.Nombre)
                .ToList();
        }

        public List<StockSucursal> ObtenerTodas(bool soloActivos = true)
        {
            using var context = _contextFactory.CreateDbContext();
            return Query(context, soloActivos)
                .OrderBy(s => s.Sucursal!.Nombre)
                .ThenBy(s => s.Producto!.Nombre)
                .ToList();
        }

        public StockSucursal? ObtenerPorSucursalYProducto(int sucursalId, int productoId)
        {
            using var context = _contextFactory.CreateDbContext();
            return Query(context, soloActivos: true)
                .FirstOrDefault(s => s.SucursalId == sucursalId && s.ProductoId == productoId);
        }

        public StockSucursal Ajustar(int sucursalId, int productoId, int cantidadResultante)
        {
            if (cantidadResultante < 0)
                throw new InvalidOperationException("La cantidad resultante no puede ser negativa.");

            using var context = _contextFactory.CreateDbContext();
            var stock = context.StocksSucursal
                .FirstOrDefault(s => s.SucursalId == sucursalId && s.ProductoId == productoId);

            // Si no existe registro para el par, el ajuste lo crea con la cantidad
            // ingresada (recuento inicial); la cantidad anterior en ese caso es 0.
            if (stock == null)
            {
                stock = new StockSucursal
                {
                    SucursalId = sucursalId,
                    ProductoId = productoId,
                    Cantidad = cantidadResultante,
                    CreatedAt = DateTime.Now
                };
                context.StocksSucursal.Add(stock);
            }
            else
            {
                stock.Cantidad = cantidadResultante;
                stock.UpdatedAt = DateTime.Now;
            }

            context.SaveChanges();
            return stock;
        }

        public void Incrementar(int sucursalId, int productoId, int cantidad)
        {
            if (cantidad < 0)
                throw new InvalidOperationException("La cantidad a incrementar no puede ser negativa.");

            using var context = _contextFactory.CreateDbContext();
            var stock = context.StocksSucursal
                .FirstOrDefault(s => s.SucursalId == sucursalId && s.ProductoId == productoId);

            if (stock == null)
            {
                context.StocksSucursal.Add(new StockSucursal
                {
                    SucursalId = sucursalId,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                stock.Cantidad += cantidad;
                stock.UpdatedAt = DateTime.Now;
            }

            context.SaveChanges();
        }

        public List<StockSinVentaItem> ObtenerConStockSinVentas(
            DateTime desde, DateTime hastaExclusivo, int? sucursalId, int? categoriaId)
        {
            using var context = _contextFactory.CreateDbContext();

            // 1. Filtramos stock que no sea 0 ni producto/sucursal que esten eliminados
            var stockQuery = context.StocksSucursal
                .Where(s => s.Cantidad > 0
                         && s.Producto!.DeletedAt == null
                         && s.Sucursal!.DeletedAt == null);

            if (sucursalId.HasValue)
                stockQuery = stockQuery.Where(s => s.SucursalId == sucursalId.Value);

            if (categoriaId.HasValue)
                stockQuery = stockQuery.Where(s => s.Producto!.CategoriaId == categoriaId.Value);

            // 2. Traemos el stock que esta por (producto, sucursal)
            var stock = stockQuery
                .Select(s => new
                {
                    s.ProductoId,
                    Nombre = s.Producto!.Nombre,
                    Categoria = s.Producto!.Categoria!.Nombre,
                    Precio = s.Producto!.Precio,
                    s.Cantidad
                })
                .ToList()
                .GroupBy(x => new { x.ProductoId, x.Nombre, x.Categoria, x.Precio })
                .Select(g => new StockSinVentaItem
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    NombreCategoria = g.Key.Categoria,
                    Precio = g.Key.Precio,
                    Stock = g.Sum(x => x.Cantidad)
                })
                .ToList();

            if (stock.Count == 0)
                return stock;

            var ids = stock.Select(s => s.ProductoId).ToList();

            // 3. Tramoes las ventas del período abiertas por sucursal: sirven para descartar los que
            // sí rotaron y para saber si el producto se vende en otra sucursal.
            var ventasPeriodo = context.VentasDetalle
                .Where(d => ids.Contains(d.ProductoId)
                         && d.Venta!.Estado == EstadoVenta.CONFIRMADA
                         && d.Venta.CreatedAt >= desde
                         && d.Venta.CreatedAt < hastaExclusivo)
                .GroupBy(d => new { d.ProductoId, d.Venta!.SucursalId })
                .Select(g => new
                {
                    g.Key.ProductoId,
                    g.Key.SucursalId,
                    Unidades = g.Sum(d => d.Cantidad)
                })
                .ToList()
                .ToLookup(v => v.ProductoId);

            // 4. Última venta sin filtro de fecha
            var ultimaVenta = context.VentasDetalle
                .Where(d => ids.Contains(d.ProductoId) && d.Venta!.Estado == EstadoVenta.CONFIRMADA)
                .GroupBy(d => d.ProductoId)
                .Select(g => new { ProductoId = g.Key, Ultima = g.Max(d => d.Venta!.CreatedAt) })
                .ToList()
                .ToDictionary(x => x.ProductoId, x => (DateTime?)x.Ultima);

            var resultado = new List<StockSinVentaItem>();

            foreach (var s in stock)
            {
                // Filtramos los productos (de todas las sucursales) que tuvieron ventas en el periodo
                var ventas = ventasPeriodo[s.ProductoId];

                // Sumamos las unidades que tuvieron ventas en el periodo
                var vendidas = sucursalId.HasValue
                    ? ventas.Where(v => v.SucursalId == sucursalId.Value).Sum(v => v.Unidades)
                    : ventas.Sum(v => v.Unidades);

                // Si es > 0 no lo utiliza
                if (vendidas > 0)
                    continue;

                // Este es el objeto de lectura que devuelve el repositorio para que el caso de uso cree el DTO
                resultado.Add(new StockSinVentaItem
                {
                    ProductoId = s.ProductoId,
                    NombreProducto = s.NombreProducto,
                    NombreCategoria = s.NombreCategoria,
                    Precio = s.Precio,
                    Stock = s.Stock,
                    UltimaVenta = ultimaVenta.TryGetValue(s.ProductoId, out var ultima) ? ultima : null,
                    // Con todas las sucursales no hay "otras", el dato solo tiene sentido con una elegida.
                    VendidasOtrasSucursales = sucursalId.HasValue
                        ? ventas.Where(v => v.SucursalId != sucursalId.Value).Sum(v => v.Unidades)
                        : 0
                });
            }

            // Lo que más plata tiene quieta, primero.
            return resultado
                .OrderByDescending(s => s.Stock * s.Precio)
                .ToList();
        }

        private static IQueryable<StockSucursal> Query(AppDbContext context, bool soloActivos)
        {
            var query = context.StocksSucursal
                .Include(s => s.Producto!)
                    .ThenInclude(p => p.Categoria)
                .Include(s => s.Sucursal)
                .AsQueryable();

            if (soloActivos)
                query = query.Where(s =>
                    s.Producto!.DeletedAt == null &&
                    s.Sucursal!.DeletedAt == null);

            return query;
        }
    }
}
