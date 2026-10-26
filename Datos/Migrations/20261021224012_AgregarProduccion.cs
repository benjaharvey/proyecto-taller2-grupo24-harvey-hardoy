using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La rama de ventas ya creó esta tabla con la misma estructura
            // (migración AgregarProduccionesYCambiarPrecioProducto). Al mergear
            // ambas ramas quedaron dos migraciones que la crean: esta tiene que
            // poder aplicarse aunque la tabla ya exista.
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[Producciones]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Producciones] (
                        [Id] int NOT NULL IDENTITY,
                        [Cantidad] int NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [DeletedAt] datetime2 NULL,
                        [UsuarioId] int NOT NULL,
                        [ProductoId] int NOT NULL,
                        [SucursalId] int NOT NULL,
                        CONSTRAINT [PK_Producciones] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Producciones_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]),
                        CONSTRAINT [FK_Producciones_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursales] ([Id]),
                        CONSTRAINT [FK_Producciones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id])
                    );

                    CREATE INDEX [IX_Producciones_ProductoId] ON [Producciones] ([ProductoId]);
                    CREATE INDEX [IX_Producciones_SucursalId] ON [Producciones] ([SucursalId]);
                    CREATE INDEX [IX_Producciones_UsuarioId] ON [Producciones] ([UsuarioId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Producciones");
        }
    }
}
