using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class ProduccionRepositorio: IProduccionRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProduccionRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Agregar(Produccion p_produccion, List<ProductoInsumo> receta)
        {
            using var context = _contextFactory.CreateDbContext();
            var ahora = DateTime.Now;

            // 1. Traer los insumos de la receta, con el stock ACTUAL de la base
            var insumoIds = receta.Select(r => r.InsumoId).ToList();
            var insumos = context.Insumos
                .Where(i => insumoIds.Contains(i.Id) && i.DeletedAt == null)
                .ToList();

            // 2. Validar TODOS antes de tocar nada
            foreach (var item in receta)
            {
                var insumo = insumos.FirstOrDefault(i => i.Id == item.InsumoId)
                    ?? throw new InvalidOperationException("Uno de los insumos de la receta no existe o está dado de baja.");

                var necesario = item.CantidadNecesaria * p_produccion.Cantidad;

                if (insumo.Stock < necesario)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente de {insumo.Nombre}: se necesitan {necesario} {insumo.Unidad} y hay {insumo.Stock}.");
                }
            }

            // 3. Descontar
            foreach (var item in receta)
            {
                var insumo = insumos.First(i => i.Id == item.InsumoId);
                insumo.Stock -= item.CantidadNecesaria * p_produccion.Cantidad;
                insumo.UpdatedAt = ahora;
            }

            // 4. Sumar al stock del producto en la sucursal (crear la fila si no existe)
            var stock = context.StocksSucursal.FirstOrDefault(s =>
                s.SucursalId == p_produccion.SucursalId && s.ProductoId == p_produccion.ProductoId);

            if (stock == null)
            {
                context.StocksSucursal.Add(new StockSucursal
                {
                    SucursalId = p_produccion.SucursalId,
                    ProductoId = p_produccion.ProductoId,
                    Cantidad = p_produccion.Cantidad,
                    CreatedAt = ahora
                });
            }
            else
            {
                stock.Cantidad += p_produccion.Cantidad;
                stock.UpdatedAt = ahora;
            }

            // 5. Registrar la producción
            p_produccion.CreatedAt = ahora;
            context.Producciones.Add(p_produccion);

            // 6. Un solo SaveChanges: todo o nada
            context.SaveChanges();
        }

        public List<Produccion> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Producciones
                .Include(p => p.Producto)
                .Include(p => p.Usuario)
                .Include(p => p.Sucursal)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }
    }
}