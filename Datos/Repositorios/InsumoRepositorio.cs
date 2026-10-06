using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class InsumoRepositorio : IInsumoRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public InsumoRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Insumo> ObtenerTodos(bool incluirEliminados = false)
        {
            using var context = _contextFactory.CreateDbContext();
            if (!incluirEliminados)
            {
                return context.Insumos.Where(i => i.DeletedAt == null).ToList();
            }

            return context.Insumos.ToList();
        }

        public Insumo? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Insumos.FirstOrDefault(i => i.Id == id && i.DeletedAt == null);
        }

        public Insumo? ObtenerPorNombre(string nombre)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Insumos.FirstOrDefault(i => i.Nombre.ToLower() == nombre.ToLower() && i.DeletedAt == null);
        }

        public bool EstaEnRecetaActiva(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ProductosInsumos
                .Any(pi => pi.InsumoId == id && pi.Producto != null && pi.Producto.DeletedAt == null);
        }

        public void Agregar(Insumo insumo)
        {
            using var context = _contextFactory.CreateDbContext();
            insumo.CreatedAt = DateTime.Now;
            context.Insumos.Add(insumo);
            context.SaveChanges();
        }

        public void Actualizar(Insumo insumo)
        {
            using var context = _contextFactory.CreateDbContext();
            var existente = context.Insumos.FirstOrDefault(i => i.Id == insumo.Id && i.DeletedAt == null)
                ?? throw new InvalidOperationException("El insumo que intentás modificar ya no existe. Recargá la lista.");

            existente.Nombre = insumo.Nombre;
            existente.Unidad = insumo.Unidad;
            existente.Stock = insumo.Stock;
            existente.StockMinimo = insumo.StockMinimo;
            existente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var insumo = context.Insumos.FirstOrDefault(i => i.Id == id && i.DeletedAt == null)
                ?? throw new InvalidOperationException("El insumo que intentás eliminar ya no existe. Recargá la lista.");

            insumo.DeletedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Reactivar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var insumo = context.Insumos.FirstOrDefault(i => i.Id == id && i.DeletedAt != null)
                ?? throw new InvalidOperationException("El insumo no existe o no se encuentra eliminado. Recargá la lista.");

            insumo.DeletedAt = null;
            insumo.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}
