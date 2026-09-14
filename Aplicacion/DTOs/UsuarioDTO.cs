namespace Aplicacion.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public int RolId { get; set; }
        public string NombreRol { get; set; } = "";
        public int SucursalId { get; set; }
    }
}