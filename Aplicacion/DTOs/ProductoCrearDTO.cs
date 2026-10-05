using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class ProductoCrearDTO
    {
        public string Nombre {get; set; } = "";

        public int Precio {get; set; }

        public int CategoriaId { get; set; }

        public string? RutaImagen { get; set; }
    }
}
