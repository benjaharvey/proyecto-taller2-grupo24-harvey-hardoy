namespace Dominio.Entidades
{
    public class Sucursal
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Ubicacion { get; set; } = "";

        public string Estado { get; set; } = "ACTIVA";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}