namespace Dominio.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Email { get; set; } = "";
        public string Password { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int RolId { get; set; }
        public int SucursalId { get; set; }

        public Rol? Rol { get; set; }
    }
}