using System.Collections.Generic;

namespace Aplicacion.DTOs
{
    public class ConsultaStockDTO
    {
        public List<StockSucursalDTO> Detalle { get; set; } = new();

        public List<StockSucursalDTO> Consolidado { get; set; } = new();
    }
}
