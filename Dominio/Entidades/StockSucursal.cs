namespace Dominio.Entidades
{
    public class StockSucursal
    {
        public int Id { get; set; }

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public decimal Cantidad { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
