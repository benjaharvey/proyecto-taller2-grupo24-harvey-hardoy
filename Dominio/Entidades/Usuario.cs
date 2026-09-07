namespace Dominio.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }

        public string email { get; set; } = "";
        public string password { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int RolId { get; set; }

        public Rol? Rol { get; set; }
    }
}