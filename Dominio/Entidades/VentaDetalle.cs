namespace Dominio.Entidades
{
    public class VentaDetalle
    {
        public int Id { get; set; }

        public int VentaCabeceraId { get; set; }
        public VentaCabecera? Venta { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnit { get; set; }

        // Costo de producir una unidad al momento de la venta (se congela, igual que PrecioUnit).
        public decimal CostoUnit { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}