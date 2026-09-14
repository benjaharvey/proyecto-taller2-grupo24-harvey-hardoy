namespace Aplicacion.DTOs
{
    public class UsuarioCrearDTO
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = ""; 
        public int RolId { get; set; }
        public int SucursalId { get; set; }
    }
}