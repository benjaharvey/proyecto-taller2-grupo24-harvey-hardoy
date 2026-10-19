namespace Dominio.Entidades
{
    public class Produccion
    {
        public int Id {get; set;}

        public int Cantidad {get; set;}

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int UsuarioId {get; set;}
        public Usuario? Usuario {get; set;}

        public int ProductoId {get; set;}
        public Producto? Producto {get; set;}

        public int SucursalId {get; set;}
        public Sucursal? Sucursal {get; set;}
    }
}