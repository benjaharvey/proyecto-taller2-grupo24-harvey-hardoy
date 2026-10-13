using System;
using System.Linq;
using Datos.Conexion;
using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Datos.Seeder
{
    public static class DatabaseSeeder
    {
        public static void Inicializar(AppDbContext context)
        {
            // Aplica migraciones pendientes si la base de datos está configurada con migraciones
            context.Database.Migrate();

            // 1. Roles requeridos
            var rolesRequeridos = new[] { "Admin", "Cocinero", "Vendedor" };
            foreach (var nombreRol in rolesRequeridos)
            {
                // Si ya hay uno activo no se toca nada: buscar sin filtrar DeletedAt podría
                // "restaurar" una fila duplicada y dejar el rol repetido en los combos.
                if (context.Roles.Any(r => r.Nombre == nombreRol && r.DeletedAt == null))
                    continue;

                // No hay ninguno activo. Si existe borrado lógicamente se restaura: si solo se
                // mirara el estado activo, el rol nunca se recrearía (el seeder no lo ve) pero
                // los repositorios lo ignoran, y sus usuarios se quedarían sin permisos.
                var borrado = context.Roles.FirstOrDefault(r => r.Nombre == nombreRol);
                if (borrado != null)
                {
                    borrado.DeletedAt = null;
                    borrado.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.Roles.Add(new Rol
                    {
                        Nombre = nombreRol,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.SaveChanges();

            var adminRol = context.Roles.First(r => r.Nombre == "Admin" && r.DeletedAt == null);
            var cocineroRol = context.Roles.First(r => r.Nombre == "Cocinero" && r.DeletedAt == null);
            var vendedorRol = context.Roles.First(r => r.Nombre == "Vendedor" && r.DeletedAt == null);

            // 2. Sucursales requeridas del sistema
            var sucursalesSeed = new[]
            {
                new { Nombre = "Casa Central", Ubicacion = "San Martin 1120", Estado = "ACTIVA" },
                new { Nombre = "Fábrica", Ubicacion = "Ruta 12 KM 1035", Estado = "ACTIVA" },
                new { Nombre = "Sucursal Centro", Ubicacion = "Junin 1300", Estado = "ACTIVA" }
            };

            foreach (var s in sucursalesSeed)
            {
                if (context.Sucursales.Any(x => x.Nombre == s.Nombre && x.DeletedAt == null))
                    continue;

                var borrada = context.Sucursales.FirstOrDefault(x => x.Nombre == s.Nombre);
                if (borrada != null)
                {
                    borrada.DeletedAt = null;
                    borrada.Ubicacion = s.Ubicacion;
                    borrada.Estado = "ACTIVA";
                    borrada.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.Sucursales.Add(new Sucursal
                    {
                        Nombre = s.Nombre,
                        Ubicacion = s.Ubicacion,
                        Estado = s.Estado,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.SaveChanges();

            var casaCentral = context.Sucursales.First(s => s.Nombre == "Casa Central" && s.DeletedAt == null);
            var fabrica = context.Sucursales.First(s => s.Nombre == "Fábrica" && s.DeletedAt == null);
            var sucursalCentro = context.Sucursales.First(s => s.Nombre == "Sucursal Centro" && s.DeletedAt == null);

            // 3. Usuarios de prueba por rol
            var usuariosSeed = new[]
            {
                new
                {
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Dni = "00000001",
                    Email = "admin@test.com",
                    PasswordPlana = "admin123",
                    RolId = adminRol.Id,
                    Direccion = "Moreno 250",
                    FechaNacimiento = new DateTime(1990, 1, 1),
                    SucursalId = casaCentral.Id
                },
                new
                {
                    Nombre = "Carlos",
                    Apellido = "Cocinero",
                    Dni = "00000002",
                    Email = "cocinero@test.com",
                    PasswordPlana = "cocina123",
                    RolId = cocineroRol.Id,
                    Direccion = "Av. 3 de Abril 1150",
                    FechaNacimiento = new DateTime(1992, 5, 15),
                    SucursalId = fabrica.Id
                },
                new
                {
                    Nombre = "Vanina",
                    Apellido = "Vendedor",
                    Dni = "00000003",
                    Email = "vendedor@test.com",
                    PasswordPlana = "vendedor123",
                    RolId = vendedorRol.Id,
                    Direccion = "Santa Fe 2100",
                    FechaNacimiento = new DateTime(1995, 10, 20),
                    SucursalId = sucursalCentro.Id
                }
            };

            foreach (var u in usuariosSeed)
            {
                if (!context.Usuarios.Any(x => x.Email == u.Email))
                {
                    context.Usuarios.Add(new Usuario
                    {
                        Nombre = u.Nombre,
                        Apellido = u.Apellido,
                        Dni = u.Dni,
                        Email = u.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(u.PasswordPlana),
                        RolId = u.RolId,
                        SucursalId = u.SucursalId,
                        FechaNacimiento = u.FechaNacimiento,
                        Direccion = u.Direccion,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            context.SaveChanges();

            // 4. Insumos iniciales de prueba
            var insumosSeed = new[]
            {
                new { Nombre = "Harina 0000", Unidad = "kg", Stock = 45.0m, StockMinimo = 15.0m },
                new { Nombre = "Dulce de Leche Repostero", Unidad = "kg", Stock = 8.5m, StockMinimo = 12.0m },
                new { Nombre = "Chocolate Semiamargo", Unidad = "kg", Stock = 25.0m, StockMinimo = 10.0m }
            };

            foreach (var ins in insumosSeed)
            {
                if (context.Insumos.Any(x => x.Nombre == ins.Nombre && x.DeletedAt == null))
                    continue;

                var borrado = context.Insumos.FirstOrDefault(x => x.Nombre == ins.Nombre);
                if (borrado != null)
                {
                    borrado.DeletedAt = null;
                    borrado.Unidad = ins.Unidad;
                    borrado.Stock = ins.Stock;
                    borrado.StockMinimo = ins.StockMinimo;
                    borrado.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.Insumos.Add(new Insumo
                    {
                        Nombre = ins.Nombre,
                        Unidad = ins.Unidad,
                        Stock = ins.Stock,
                        StockMinimo = ins.StockMinimo,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            context.SaveChanges();

            // 5. Categorías iniciales de productos
            var categoriasSeed = new[] { "Alfajores", "Conitos", "Tabletas", "Tortas", "Especiales" };

            foreach (var nombreCategoria in categoriasSeed)
            {
                // A diferencia de roles/sucursales/insumos, no se restaura una categoría borrada:
                // si el admin la dio de baja a propósito, reaparecería en cada inicio de la app.
                if (context.Categorias.Any(x => x.Nombre == nombreCategoria))
                    continue;

                context.Categorias.Add(new Categoria
                {
                    Nombre = nombreCategoria,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.SaveChanges();
        }
    }
}
