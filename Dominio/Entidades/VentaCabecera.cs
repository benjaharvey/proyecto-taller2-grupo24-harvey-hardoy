using Dominio.Enums;

namespace Dominio.Entidades
{
    public class VentaCabecera
    {
        public int Id { get; set; }

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public MetodoPago? MetodoPago { get; set; }

        public EstadoVenta Estado { get; set; } = EstadoVenta.PENDIENTE;

        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    }
}