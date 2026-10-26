using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class VentaPorSucursalDTO
    {
        public string NombreSucursal { get; set; } = string.Empty;
        
        public int CantidadVentas {  get; set; }

        public decimal TotalRecaudado { get; set; }
    }
}
