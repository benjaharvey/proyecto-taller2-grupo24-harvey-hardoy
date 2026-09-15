namespace Aplicacion.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = "";
        public int RolId { get; set; }
        public string NombreRol { get; set; } = "";
        public int SucursalId { get; set; }
    }
}