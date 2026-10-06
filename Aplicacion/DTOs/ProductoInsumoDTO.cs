namespace Aplicacion.DTOs
{
    public class ProductoInsumoDTO
    {
        public int InsumoId { get; set; }
        public decimal CantidadNecesaria { get; set; }
        public string NombreInsumo { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;

    }
}