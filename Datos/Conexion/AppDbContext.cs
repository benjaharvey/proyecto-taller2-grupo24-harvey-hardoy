using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;

namespace Datos.Conexion
{
    public class AppDbContext : DbContext
    {
        public const string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=ProyectoCRUD;Trusted_Connection=True;TrustServerCertificate=True;";

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
