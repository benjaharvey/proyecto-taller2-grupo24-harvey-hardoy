namespace Dominio.Entidades
{
    public class ProductoInsumo
    {
        public decimal CantidadNecesaria { get; set; }
        public int InsumoId { get; set; }
        public int ProductoId { get; set; }

        public Insumo? Insumo { get; set; }
        public Producto? Producto { get; set; }

    }
}    