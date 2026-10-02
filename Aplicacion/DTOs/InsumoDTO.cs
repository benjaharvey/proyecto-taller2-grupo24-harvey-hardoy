namespace Aplicacion.DTOs
{
    public class InsumoDTO
    {
        public int Id { get; set; }
        public string Codigo => $"INS-{Id:D3}";
        public string Nombre { get; set; } = "";
        public string Unidad { get; set; } = "";
        public decimal Stock { get; set; }
        public decimal StockMinimo { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? DeletedAt { get; set; }
        public bool BajoStock => Stock <= StockMinimo;
        public string NivelAlerta => !Activo ? "Inactivo" : Stock == 0 ? "Sin Stock" : Stock <= StockMinimo ? "Bajo Stock" : "Óptimo";
    }
}
