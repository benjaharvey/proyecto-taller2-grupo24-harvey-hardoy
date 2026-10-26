using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VentasCabecera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    MetodoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasCabecera", x => x.Id);
                    table.CheckConstraint("CK_VentasCabecera_Completitud", "[Estado] = 'PENDIENTE' OR ([ClienteId] IS NOT NULL AND [MetodoPago] IS NOT NULL)");
                    table.CheckConstraint("CK_VentasCabecera_Estado", "[Estado] IN ('PENDIENTE', 'CONFIRMADA', 'CANCELADA')");
                    table.CheckConstraint("CK_VentasCabecera_MetodoPago", "[MetodoPago] IS NULL OR [MetodoPago] IN ('EFECTIVO', 'TARJETA', 'TRANSFERENCIA')");
                    table.CheckConstraint("CK_VentasCabecera_Total", "[Total] >= 0");
                    table.ForeignKey(
                        name: "FK_VentasCabecera_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasCabecera_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasCabecera_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaCabeceraId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasDetalle", x => x.Id);
                    table.CheckConstraint("CK_VentasDetalle_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_VentasDetalle_PrecioUnit", "[PrecioUnit] > 0");
                    table.ForeignKey(
                        name: "FK_VentasDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasDetalle_VentasCabecera_VentaCabeceraId",
                        column: x => x.VentaCabeceraId,
                        principalTable: "VentasCabecera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentasCabecera_ClienteId",
                table: "VentasCabecera",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasCabecera_SucursalId_CreatedAt",
                table: "VentasCabecera",
                columns: new[] { "SucursalId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VentasCabecera_UsuarioId",
                table: "VentasCabecera",
                column: "UsuarioId",
                unique: true,
                filter: "[Estado] = N'PENDIENTE'");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalle_ProductoId",
                table: "VentasDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalle_VentaCabeceraId",
                table: "VentasDetalle",
                column: "VentaCabeceraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentasDetalle");

            migrationBuilder.DropTable(
                name: "VentasCabecera");
        }
    }
}
