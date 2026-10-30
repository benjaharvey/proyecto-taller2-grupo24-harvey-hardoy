using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class VentaPorVendedorDTO
    {
        public string NombreVendedor { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal TotalRecaudado { get; set; }
    }
}
