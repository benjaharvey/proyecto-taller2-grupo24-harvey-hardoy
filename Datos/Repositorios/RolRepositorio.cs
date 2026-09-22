using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class RolRepositorio : IRolRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RolRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Rol> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.Where(r => r.DeletedAt == null).ToList();
        }

        public Rol? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.FirstOrDefault(r => r.Id == id && r.DeletedAt == null);
        }

        public Rol? ObtenerPorNombre(string nombre)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.FirstOrDefault(r => r.Nombre == nombre && r.DeletedAt == null);
        }

        public void Agregar(Rol rol)
        {
            using var context = _contextFactory.CreateDbContext();
            rol.CreatedAt = DateTime.Now;
            context.Roles.Add(rol);
            context.SaveChanges();
        }

        public void Actualizar(Rol rol)
        {
            using var context = _contextFactory.CreateDbContext();
            var existente = context.Roles.FirstOrDefault(r => r.Id == rol.Id && r.DeletedAt == null)
                ?? throw new InvalidOperationException("El rol que intentás modificar ya no existe. Recargá la lista.");

            existente.Nombre = rol.Nombre;
            existente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var rol = context.Roles.FirstOrDefault(r => r.Id == id && r.DeletedAt == null)
                ?? throw new InvalidOperationException("El rol que intentás eliminar ya no existe. Recargá la lista.");

            rol.DeletedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}
