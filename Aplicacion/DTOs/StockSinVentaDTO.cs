namespace Aplicacion.DTOs
{
    public class StockSinVentaDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public string NombreCategoria { get; set; } = "";
        public int Stock { get; set; }
        public decimal Precio { get; set; }

        // Stock * Precio: plata inmovilizada valuada a precio de venta.
        public decimal Valorizado { get; set; }

        public DateTime? UltimaVenta { get; set; }

        // Null cuando el producto nunca se vendió.
        public int? DiasSinVender { get; set; }

        public string UltimaVentaTexto { get; set; } = "";

        public int VendidasOtrasSucursales { get; set; }
    }
}
