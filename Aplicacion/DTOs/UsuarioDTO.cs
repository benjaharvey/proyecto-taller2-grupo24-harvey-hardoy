namespace Aplicacion.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public string Dni { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = "";
        public int RolId { get; set; }
        public string NombreRol { get; set; } = "";
        public int SucursalId { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? DeletedAt { get; set; }
        public string NombreSucursal { get; set; } = "";
    }
}