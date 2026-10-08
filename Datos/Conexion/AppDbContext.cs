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
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Insumo> Insumos { get; set; }

        public DbSet<Producto> Productos {get; set;}
        public DbSet<Categoria> Categorias {get; set;} 

        public DbSet<Cliente> Clientes {get; set;}

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Insumo>(entity =>
            {
                entity.Property(e => e.Stock).HasPrecision(18, 3);
                entity.Property(e => e.StockMinimo).HasPrecision(18, 3);
            });
        }
    }
}
