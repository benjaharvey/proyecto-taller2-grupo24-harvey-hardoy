using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProduccionesYCambiarPrecioProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // La rama de produccion creó la misma tabla en su propia migración
            // (AgregarProduccion). Al mergear, cualquiera de las dos puede correr
            // segunda: se crea solo si no existe.
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

            migrationBuilder.AlterColumn<int>(
                name: "Precio",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
