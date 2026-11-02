namespace Dominio.Consultas
{
    public class StockSinVentaItem
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public string NombreCategoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        // Última venta histórica del producto, sin importar la fecha del reporte.
        public DateTime? UltimaVenta { get; set; }

        // Unidades vendidas en el período en las otras sucursales (0 si no se filtró por sucursal).
        public int VendidasOtrasSucursales { get; set; }
    }
}
