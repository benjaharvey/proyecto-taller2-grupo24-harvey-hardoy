namespace Aplicacion.DTOs
{
    public class ProductoDTO
    {
        public int Id {get; set;}

        public string Nombre {get; set; } = "";

        public int Precio {get; set; }

        public string? RutaImagen { get; set; }

        public int CategoriaId { get; set; }

        public string NombreCategoria {get; set;} = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

         public List<ProductoInsumoDTO> Receta { get; set; } = new();

    }
}
