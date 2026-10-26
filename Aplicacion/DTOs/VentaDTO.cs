using Dominio.Enums;

namespace Aplicacion.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public string NumeroComprobante { get; set; } = "";
        public DateTime Fecha { get; set; }

        public int SucursalId { get; set; }
        public string NombreSucursal { get; set; } = "";

        public int? ClienteId { get; set; }
        public string NombreCliente { get; set; } = "";
        public string DniCliente { get; set; } = "";

        public int UsuarioId { get; set; }
        public string NombreVendedor { get; set; } = "";

        public MetodoPago? MetodoPago { get; set; }
        public EstadoVenta Estado { get; set; }

        public int CantidadItems { get; set; }
        public decimal Total { get; set; }

        public List<VentaDetalleDTO> Detalles { get; set; } = new();
    }
}