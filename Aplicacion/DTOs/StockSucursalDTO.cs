using System;
using System.Collections.Generic;

namespace Aplicacion.DTOs
{
    public class StockSucursalDTO
    {
        public int Id { get; set; }

        public int SucursalId { get; set; }
        public string NombreSucursal { get; set; } = "";

        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = "";

        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = "";

        public int Cantidad { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
