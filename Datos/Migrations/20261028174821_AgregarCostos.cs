using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCostos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnit",
                table: "VentasDetalle",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Costo",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VentasDetalle_CostoUnit",
                table: "VentasDetalle",
                sql: "[CostoUnit] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Costo",
                table: "Productos",
                sql: "[Costo] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VentasDetalle_CostoUnit",
                table: "VentasDetalle");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Costo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CostoUnit",
                table: "VentasDetalle");

            migrationBuilder.DropColumn(
                name: "Costo",
                table: "Productos");
        }
    }
}
