using Aplicacion.DTOs;
using Dominio.Enums;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerRecaudacionPorMetodoPago
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerRecaudacionPorMetodoPago(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<RecaudacionPorMetodoPagoDTO> Ejecutar(
            DateTime desde, DateTime hasta, int? sucursalId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            // se consulta hasta el inicio del día siguiente.
            var hastaExclusivo = hasta.Date.AddDays(1);

            var porMetodo = _repositorio
                .ObtenerRecaudacionPorMetodoPago(desde.Date, hastaExclusivo, sucursalId)
                .ToDictionary(t => t.Metodo, t => (Cantidad: t.CantidadVentas, Monto: t.TotalRecaudado));

            var total = porMetodo.Values.Sum(v => v.Monto);

            // Se devuelven siempre los tres métodos, aunque alguno no tenga ventas.
            return Enum.GetValues<MetodoPago>()
                .Select(metodo =>
                {
                    var cantidad = 0;
                    var monto = 0m;

                    if (porMetodo.TryGetValue(metodo, out var dato))
                    {
                        cantidad = dato.Cantidad;
                        monto = dato.Monto;
                    }

                    return new RecaudacionPorMetodoPagoDTO
                    {
                        Metodo = metodo,
                        Nombre = NombreVisible(metodo),
                        CantidadVentas = cantidad,
                        TotalRecaudado = monto,
                        Participacion = total > 0 ? Math.Round((double)(monto * 100 / total), 1) : 0,
                        TicketPromedio = cantidad > 0 ? Math.Round(monto / cantidad, 2) : 0
                    };
                })
                .ToList();
        }

        private static string NombreVisible(MetodoPago metodo) => metodo switch
        {
            MetodoPago.EFECTIVO => "Efectivo",
            MetodoPago.TARJETA => "Tarjeta",
            MetodoPago.TRANSFERENCIA => "Transferencia",
            _ => metodo.ToString()
        };
    }
}
