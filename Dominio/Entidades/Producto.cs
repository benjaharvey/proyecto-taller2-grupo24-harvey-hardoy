namespace Dominio.Entidades
{
    public class Producto
    {
        public int Id {get; set; }
        public string Nombre {get; set; } = "";
        /* PREGUNTAR SI GUARDAMOS CON INT O CON FLOAT/DECIMAL */
        public int Precio {get; set; }
        public string? RutaImagen { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int CategoriaId {get; set;}

        public Categoria? Categoria { get; set; }

        public ICollection<ProductoInsumo> ProductoInsumos { get; set; } = new List<ProductoInsumo>();

    }
}
