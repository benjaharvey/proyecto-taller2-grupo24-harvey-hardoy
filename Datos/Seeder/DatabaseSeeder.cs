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
                // La búsqueda incluye los borrados lógicamente: si no, el rol eliminado no se recrea
                // (el seeder lo ve) pero los repositorios lo ignoran, y sus usuarios quedan sin permisos.
                var existente = context.Roles.FirstOrDefault(r => r.Nombre == nombreRol);
                if (existente == null)
                {
                    context.Roles.Add(new Rol
                    {
                        Nombre = nombreRol,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (existente.DeletedAt != null)
                {
                    existente.DeletedAt = null;
                    existente.UpdatedAt = DateTime.UtcNow;
                }
            }
            context.SaveChanges();

            var adminRol = context.Roles.First(r => r.Nombre == "Admin" && r.DeletedAt == null);
            var cocineroRol = context.Roles.First(r => r.Nombre == "Cocinero" && r.DeletedAt == null);
            var vendedorRol = context.Roles.First(r => r.Nombre == "Vendedor" && r.DeletedAt == null);

            // 2. Usuarios de prueba por rol
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
                    Direccion = "Av. Principal 100",
                    FechaNacimiento = new DateTime(1990, 1, 1),
                    SucursalId = 1
                },
                new
                {
                    Nombre = "Carlos",
                    Apellido = "Cocinero",
                    Dni = "00000002",
                    Email = "cocinero@test.com",
                    PasswordPlana = "cocina123",
                    RolId = cocineroRol.Id,
                    Direccion = "Av. Siempre Viva 742",
                    FechaNacimiento = new DateTime(1992, 5, 15),
                    SucursalId = 1
                },
                new
                {
                    Nombre = "Vanina",
                    Apellido = "Vendedor",
                    Dni = "00000003",
                    Email = "vendedor@test.com",
                    PasswordPlana = "vendedor123",
                    RolId = vendedorRol.Id,
                    Direccion = "Calle Comercial 456",
                    FechaNacimiento = new DateTime(1995, 10, 20),
                    SucursalId = 1
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
        }
    }
}
