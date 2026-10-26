namespace Aplicacion.DTOs
{
    public class ProductoMasVendidoDTO
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int Unidades { get; set; }
        public decimal Recaudado { get; set; }
    }
}
