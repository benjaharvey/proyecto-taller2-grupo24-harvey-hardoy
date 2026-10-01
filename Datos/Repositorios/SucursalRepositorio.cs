using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class SucursalRepositorio : ISucursalRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SucursalRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Sucursal> ObtenerTodos(bool incluirEliminadas)
        {
            using var context = _contextFactory.CreateDbContext();
            if (!incluirEliminadas)
            {
                return context.Sucursales.Where(r => r.DeletedAt == null).ToList();
            }
            else
            {
                return context.Sucursales.ToList();
            }
        }

        public Sucursal? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Sucursales.FirstOrDefault(r => r.Id == id && r.DeletedAt == null);
        }

        public Sucursal? ObtenerPorNombre(string nombre)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Sucursales.FirstOrDefault(r => r.Nombre == nombre && r.DeletedAt == null);
        }

        public void Agregar(Sucursal sucursal)
        {
            using var context = _contextFactory.CreateDbContext();
            sucursal.CreatedAt = DateTime.Now;
            context.Sucursales.Add(sucursal);
            context.SaveChanges();
        }

        public void Actualizar(Sucursal sucursal)
        {
            using var context = _contextFactory.CreateDbContext();
            var existente = context.Sucursales.FirstOrDefault(r => r.Id == sucursal.Id && r.DeletedAt == null)
                ?? throw new InvalidOperationException("La sucursal que intentás modificar ya no existe. Recargá la lista.");

            existente.Nombre = sucursal.Nombre;
            existente.Ubicacion = sucursal.Ubicacion;
            existente.Estado = sucursal.Estado;
            existente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var sucursal = context.Sucursales.FirstOrDefault(r => r.Id == id && r.DeletedAt == null)
                ?? throw new InvalidOperationException("La sucursal que intentás eliminar ya no existe. Recargá la lista.");

            sucursal.DeletedAt = DateTime.Now;
            sucursal.Estado = "INACTIVA";
            context.SaveChanges();
        }

        public void Reactivar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var sucursal = context.Sucursales.FirstOrDefault(r => r.Id == id && r.DeletedAt != null)
                ?? throw new InvalidOperationException("La sucursal no existe o no se encuentra eliminada. Recargá la lista.");

            sucursal.DeletedAt = null;
            sucursal.Estado = "ACTIVA";
            sucursal.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}
