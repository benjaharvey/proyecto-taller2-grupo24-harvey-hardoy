using Aplicacion.DTOs;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerProductosMenosVendidos
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerProductosMenosVendidos(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<ProductoMasVendidoDTO> Ejecutar(
            DateTime desde, DateTime hasta, int? sucursalId = null, int top = 10)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            if (top <= 0)
            {
                throw new InvalidOperationException("La cantidad de productos del ranking debe ser mayor a cero.");
            }

            // "Hasta" es inclusivo para el usuario: se consulta hasta el inicio del día siguiente.
            var hastaExclusivo = hasta.Date.AddDays(1);

            return _repositorio
                .ObtenerProductosMenosVendidos(desde.Date, hastaExclusivo, sucursalId, top)
                .Select(t => new ProductoMasVendidoDTO
                {
                    NombreProducto = t.NombreProducto,
                    Unidades = t.Unidades,
                    Recaudado = t.Recaudado
                })
                .ToList();
        }
    }
}