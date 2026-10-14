using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using System.Net.NetworkInformation;

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
        public DbSet<ProductoInsumo> ProductosInsumos {get; set;}

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

            modelBuilder.Entity<ProductoInsumo>(entity =>
            {
               entity.HasKey(pi => new { pi.InsumoId, pi.ProductoId});

               entity.Property(pi => pi.CantidadNecesaria).HasPrecision(18,3 );

               // Sin borrado en cascada (la app usa borrado lógico).
               entity.HasOne(pi => pi.Insumo)
                     .WithMany(i => i.ProductoInsumos)
                     .HasForeignKey(pi => pi.InsumoId)
                     .OnDelete(DeleteBehavior.Restrict);

               entity.HasOne(pi => pi.Producto)
                     .WithMany(p => p.ProductoInsumos)
                     .HasForeignKey(pi => pi.ProductoId)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            // Sin borrado en cascada (la app usa borrado lógico).
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
