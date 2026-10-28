namespace Aplicacion.DTOs
{
    public class ResumenVentasDTO
    {
        // Suma de lo cobrado en las ventas confirmadas.
        public decimal Facturado { get; set; }

        // Lo cobrado menos el costo de producir lo vendido.
        public decimal Ganancia { get; set; }

        public int CantidadVentas { get; set; }

        public int Unidades { get; set; }

        public decimal TicketPromedio {get; set;}
    }
}
