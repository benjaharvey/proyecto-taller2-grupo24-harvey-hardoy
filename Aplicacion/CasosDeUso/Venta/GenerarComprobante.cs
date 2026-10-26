using Aplicacion.Servicios;

namespace Aplicacion.CasosDeUso
{
    public class GenerarComprobante
    {
        private readonly ObtenerVenta _obtenerVenta;
        private readonly IComprobantePdfServicio _servicio;

        public GenerarComprobante(ObtenerVenta obtenerVenta, IComprobantePdfServicio servicio)
        {
            _obtenerVenta = obtenerVenta;
            _servicio = servicio;
        }

        public string Ejecutar(int ventaId)
        {
            var venta = _obtenerVenta.Ejecutar(ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            return _servicio.Generar(venta);
        }
    }
}