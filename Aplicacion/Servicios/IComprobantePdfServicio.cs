using Aplicacion.DTOs;

namespace Aplicacion.Servicios
{
    public interface IComprobantePdfServicio
    {
        string Generar(VentaDTO venta);
    }
}