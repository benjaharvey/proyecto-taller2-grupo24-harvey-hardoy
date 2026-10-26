using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    public partial class AgregarProcedimientosStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.DescontarStockVenta
                    @SucursalId      INT,
                    @ProductoId      INT,
                    @Cantidad        INT,
                    @Ahora           DATETIME2,
                    @FilasAfectadas  INT OUTPUT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE dbo.StocksSucursal
                       SET Cantidad  = Cantidad - @Cantidad,
                           UpdatedAt = @Ahora
                     WHERE SucursalId = @SucursalId
                       AND ProductoId = @ProductoId
                       AND Cantidad  >= @Cantidad;

                    SET @FilasAfectadas = @@ROWCOUNT;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.ReponerStockVenta
                    @SucursalId      INT,
                    @ProductoId      INT,
                    @Cantidad        INT,
                    @Ahora           DATETIME2,
                    @FilasAfectadas  INT OUTPUT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE dbo.StocksSucursal
                       SET Cantidad  = Cantidad + @Cantidad,
                           UpdatedAt = @Ahora
                     WHERE SucursalId = @SucursalId
                       AND ProductoId = @ProductoId;

                    SET @FilasAfectadas = @@ROWCOUNT;

                    IF @FilasAfectadas = 0
                    BEGIN
                        INSERT INTO dbo.StocksSucursal (SucursalId, ProductoId, Cantidad, CreatedAt)
                        VALUES (@SucursalId, @ProductoId, @Cantidad, @Ahora);

                        SET @FilasAfectadas = 1;
                    END;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.DescontarStockVenta;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.ReponerStockVenta;");
        }
    }
}