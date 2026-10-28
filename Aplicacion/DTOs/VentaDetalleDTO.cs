namespace Aplicacion.DTOs
{
    public class VentaDetalleDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnit;
    }
}