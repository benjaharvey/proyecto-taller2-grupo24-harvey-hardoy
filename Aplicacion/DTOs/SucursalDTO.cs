namespace Aplicacion.DTOs
{
    public class SucursalDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public string Estado { get; set; } = "ACTIVA";
    }
}