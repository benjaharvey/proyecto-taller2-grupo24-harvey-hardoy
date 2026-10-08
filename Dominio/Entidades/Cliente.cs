namespace Dominio.Entidades
{
    public class Cliente{
        public int Id {get; set;}

        public string Nombre {get; set; } = "";

        public string Apellido {get; set; } = "";

        public string Dni {get; set; } = "";

        public string Email {get; set; } = "";

        public DateTime FechaNacimiento {get; set;}

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int UsuarioId {get; set;}
        public Usuario? Usuario {get; set;}
    }
}