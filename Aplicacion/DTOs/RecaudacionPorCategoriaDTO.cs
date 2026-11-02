namespace Aplicacion.DTOs
{
    public class RecaudacionPorCategoriaDTO
    {
        public string NombreCategoria { get; set; } = string.Empty;

        public int Unidades { get; set; }

        public decimal TotalRecaudado { get; set; }

        // Porcentaje sobre el total del período (0-100).
        public double Participacion { get; set; }

        // TotalRecaudado / Unidades: precio promedio de venta real de la categoría.
        public decimal PrecioPromedio { get; set; }
    }
}
