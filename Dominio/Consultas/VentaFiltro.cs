using Dominio.Enums;

namespace Dominio.Consultas
{
    public class VentaFiltro
    {
        public int? SucursalId { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? ClienteTexto { get; set; }
        public int? UsuarioId { get; set; }
        public MetodoPago? MetodoPago { get; set; }
    }
}