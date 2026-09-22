using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UsuarioRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Usuario> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Usuarios.Where(u => u.DeletedAt == null).ToList();
        }

        public Usuario? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Usuarios.FirstOrDefault(u => u.Id == id && u.DeletedAt == null);
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Usuarios.FirstOrDefault(u => u.Email == email && u.DeletedAt == null);
        }

        public Usuario? ObtenerPorDni(string dni)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Usuarios.FirstOrDefault(u => u.Dni == dni && u.DeletedAt == null);
        }

        public int ContarPorRol(int rolId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Usuarios.Count(u => u.RolId == rolId && u.DeletedAt == null);
        }

        public void Agregar(Usuario usuario)
        {
            using var context = _contextFactory.CreateDbContext();
            usuario.CreatedAt = DateTime.Now;
            context.Usuarios.Add(usuario);
            context.SaveChanges();
        }

        public void Actualizar(Usuario usuario)
        {
            using var context = _contextFactory.CreateDbContext();
            var existente = context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id && u.DeletedAt == null)
                ?? throw new InvalidOperationException("El usuario que intentás modificar ya no existe. Recargá la lista.");

            existente.Nombre = usuario.Nombre;
            existente.Apellido = usuario.Apellido;
            existente.Dni = usuario.Dni;
            existente.Email = usuario.Email;
            if (!string.IsNullOrEmpty(usuario.Password))
            {
                existente.Password = usuario.Password;
            }
            existente.FechaNacimiento = usuario.FechaNacimiento;
            existente.Direccion = usuario.Direccion;
            existente.RolId = usuario.RolId;
            existente.SucursalId = usuario.SucursalId;
            existente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == id && u.DeletedAt == null)
                ?? throw new InvalidOperationException("El usuario que intentás eliminar ya no existe. Recargá la lista.");

            usuario.DeletedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}
