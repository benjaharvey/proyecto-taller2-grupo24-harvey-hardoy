using Aplicacion.DTOs;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerResumenVentas
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerResumenVentas(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public ResumenVentasDTO Ejecutar(DateTime desde, DateTime hasta, int? sucursalId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            // "Hasta" es inclusivo para el usuario: se consulta hasta el inicio del día siguiente.
            var hastaExclusivo = hasta.Date.AddDays(1);

            var resumen = _repositorio.ObtenerResumenVentas(desde.Date, hastaExclusivo, sucursalId);

            // Ticket promedio = lo facturado dividido la cantidad de ventas.
            // La variable se declara afuera del if para poder usarla en el return.
            decimal ticketPromedio;

            if (resumen.CantidadVentas > 0)
            {
                ticketPromedio = Math.Round(resumen.Facturado / resumen.CantidadVentas, 2);
            }
            else
            {
                // Sin ventas en el rango no se puede dividir: el promedio es 0.
                ticketPromedio = 0;
            }

            return new ResumenVentasDTO
            {
                Facturado = resumen.Facturado,
                Ganancia = resumen.Ganancia,
                CantidadVentas = resumen.CantidadVentas,
                Unidades = resumen.Unidades,
                TicketPromedio = ticketPromedio
            };
        }
    }
}
