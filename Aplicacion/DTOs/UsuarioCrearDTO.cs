namespace Aplicacion.DTOs
{
    public class UsuarioCrearDTO
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Dni { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = "";
        public int RolId { get; set; }
        public int SucursalId { get; set; }
    }
}