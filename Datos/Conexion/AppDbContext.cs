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

        public DbSet<StockSucursal> StocksSucursal { get; set; }

        public DbSet<Produccion> Producciones {get; set;}

        public DbSet<VentaCabecera> VentasCabecera { get; set; }
        public DbSet<VentaDetalle> VentasDetalle { get; set; }

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

            modelBuilder.Entity<StockSucursal>(entity =>
            {
                entity.HasIndex(s => new { s.SucursalId, s.ProductoId }).IsUnique();

                entity.HasOne(s => s.Sucursal)
                    .WithMany()
                    .HasForeignKey(s => s.SucursalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Producto)
                    .WithMany()
                    .HasForeignKey(s => s.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductoInsumo>(entity =>
            {
               entity.HasKey(pi => new { pi.InsumoId, pi.ProductoId});

               entity.Property(pi => pi.CantidadNecesaria).HasPrecision(18,3 );

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

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Costo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .ToTable(t => t.HasCheckConstraint("CK_Productos_Costo", "[Costo] >= 0"));

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Sucursal)
                .WithMany()
                .HasForeignKey(u => u.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Produccion>(entity =>
            {
                entity.HasOne(p => p.Usuario)
                    .WithMany()
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Producto)
                    .WithMany()
                    .HasForeignKey(p => p.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Sucursal)
                    .WithMany()
                    .HasForeignKey(p => p.SucursalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Produccion es una entidad transaccional: sus relaciones no se borran en cascada.
            modelBuilder.Entity<Produccion>(entity =>
            {
                entity.HasOne(p => p.Producto)
                      .WithMany()
                      .HasForeignKey(p => p.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Usuario)
                      .WithMany()
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Sucursal)
                      .WithMany()
                      .HasForeignKey(p => p.SucursalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VentaCabecera>(entity =>
            {
                entity.Property(v => v.Estado)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(v => v.MetodoPago)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(v => v.Total)
                      .HasPrecision(18, 2);

                entity.HasOne(v => v.Sucursal)
                      .WithMany()
                      .HasForeignKey(v => v.SucursalId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Usuario)
                      .WithMany()
                      .HasForeignKey(v => v.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Cliente)
                      .WithMany()
                      .HasForeignKey(v => v.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Una única venta PENDIENTE por vendedor: índice único filtrado.
                entity.HasIndex(v => v.UsuarioId)
                      .HasFilter("[Estado] = N'PENDIENTE'")
                      .IsUnique();

                // Consulta del historial por sucursal y período.
                entity.HasIndex(v => new { v.SucursalId, v.CreatedAt });

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_VentasCabecera_Estado",
                        "[Estado] IN ('PENDIENTE', 'CONFIRMADA', 'CANCELADA')");

                    t.HasCheckConstraint(
                        "CK_VentasCabecera_MetodoPago",
                        "[MetodoPago] IS NULL OR [MetodoPago] IN ('EFECTIVO', 'TARJETA', 'TRANSFERENCIA')");

                    t.HasCheckConstraint(
                        "CK_VentasCabecera_Completitud",
                        "[Estado] = 'PENDIENTE' OR ([ClienteId] IS NOT NULL AND [MetodoPago] IS NOT NULL)");

                    t.HasCheckConstraint(
                        "CK_VentasCabecera_Total",
                        "[Total] >= 0");
                });
            });

            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.Property(d => d.PrecioUnit)
                      .HasPrecision(18, 2);

                entity.Property(d => d.CostoUnit)
                      .HasPrecision(18, 2);

                entity.HasOne(d => d.Venta)
                      .WithMany(v => v.Detalles)
                      .HasForeignKey(d => d.VentaCabeceraId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_VentasDetalle_Cantidad",
                        "[Cantidad] > 0");

                    t.HasCheckConstraint(
                        "CK_VentasDetalle_PrecioUnit",
                        "[PrecioUnit] > 0");

                    t.HasCheckConstraint(
                        "CK_VentasDetalle_CostoUnit",
                        "[CostoUnit] >= 0");
                });
            });
        }
    }
}
