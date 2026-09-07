namespace Dominio.Entidades
{
    public class Rol
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}