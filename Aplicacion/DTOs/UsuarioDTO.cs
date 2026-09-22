namespace Aplicacion.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Dni { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = "";
        public int RolId { get; set; }
        public string NombreRol { get; set; } = "";
        public int SucursalId { get; set; }
        public string NombreSucursal => NombreRol switch
        {
            "Cocinero" => "Fábrica / Central",
            "Admin" => "Todas / Central",
            _ => SucursalId switch
            {
                1 => "Sucursal Centro",
                2 => "Sucursal Norte",
                3 => "Sucursal Sur",
                _ => $"Sucursal {SucursalId}"
            }
        };
    }
}