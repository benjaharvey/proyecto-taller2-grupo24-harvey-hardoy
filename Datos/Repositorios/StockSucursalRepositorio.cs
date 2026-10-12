using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
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
