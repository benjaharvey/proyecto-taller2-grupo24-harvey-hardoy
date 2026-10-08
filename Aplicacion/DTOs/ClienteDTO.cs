namespace Aplicacion.DTOs
{
    public class ClienteDTO
    {
        public int Id {get; set;}
        public int UsuarioId {get; set;}

        public string Nombre {get; set;} = "";
        public string Apellido {get; set;} = "";
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public string Dni { get; set; } = "";
        public string Email { get; set; } = "";
        public string NombreRegistradoPor { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }
        public DateTime CreatedAt {get; set;}
        public DateTime? DeletedAt {get; set;}

        public bool Activo { get; set; } = true;
    }
}