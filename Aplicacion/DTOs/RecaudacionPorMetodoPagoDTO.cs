using Dominio.Enums;

namespace Aplicacion.DTOs
{
    public class RecaudacionPorMetodoPagoDTO
    {
        public MetodoPago Metodo { get; set; }

        // Nombre ya formateado para mostrar ("Efectivo", "Tarjeta", "Transferencia")
        public string Nombre { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal TotalRecaudado { get; set; }

        // Porcentaje sobre el total del período
        public double Participacion { get; set; }

        // TotalRecaudado / CantidadVentas: cuánto deja cada ticket según cómo pagó
        public decimal TicketPromedio { get; set; }
    }
}
