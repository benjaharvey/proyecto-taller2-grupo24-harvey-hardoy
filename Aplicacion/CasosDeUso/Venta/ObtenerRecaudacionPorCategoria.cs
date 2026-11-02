using Aplicacion.DTOs;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerRecaudacionPorCategoria
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerRecaudacionPorCategoria(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<RecaudacionPorCategoriaDTO> Ejecutar(
            DateTime desde, DateTime hasta, int? sucursalId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            var hastaExclusivo = hasta.Date.AddDays(1);

            var porCategoria = _repositorio
                .ObtenerRecaudacionPorCategoria(desde.Date, hastaExclusivo, sucursalId);

            var total = porCategoria.Sum(t => t.TotalRecaudado);

            // Las categorías no se rellenan en cero (a diferencia de los métodos de pago):
            // el conjunto no es fijo y una categoría sin ventas no aporta al reparto.
            return porCategoria
                .Select(t => new RecaudacionPorCategoriaDTO
                {
                    NombreCategoria = t.NombreCategoria,
                    Unidades = t.Unidades,
                    TotalRecaudado = t.TotalRecaudado,
                    Participacion = total > 0 ? Math.Round((double)(t.TotalRecaudado * 100 / total), 1) : 0,
                    PrecioPromedio = t.Unidades > 0 ? Math.Round(t.TotalRecaudado / t.Unidades, 2) : 0
                })
                .ToList();
        }
    }
}
