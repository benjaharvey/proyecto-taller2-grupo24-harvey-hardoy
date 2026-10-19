namespace Aplicacion.DTOs
{
    public class ProduccionDTO
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        public int SucursalId { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
    }
}
