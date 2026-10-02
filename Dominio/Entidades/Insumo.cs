namespace Dominio.Entidades
{
    public class Insumo
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Unidad { get; set; } = "";

        public decimal Stock { get; set; }

        public decimal StockMinimo { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
