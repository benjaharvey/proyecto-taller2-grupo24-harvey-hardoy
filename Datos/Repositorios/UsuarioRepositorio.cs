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
            var existente = context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id && u.DeletedAt == null);
            if (existente != null)
            {
                existente.Email = usuario.Email;
                existente.Password = usuario.Password;
                existente.RolId = usuario.RolId;
                existente.SucursalId = usuario.SucursalId;
                existente.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == id && u.DeletedAt == null);
            if (usuario != null)
            {
                usuario.DeletedAt = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
