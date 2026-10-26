using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerProduccionPorProducto
    {
        private readonly IProduccionRepositorio _repositorio;

        public ObtenerProduccionPorProducto(IProduccionRepositorio repo)
        {
            _repositorio = repo;
        }

        public List<ProduccionPorProductoDTO> Ejecutar(
            DateTime desde, DateTime hasta, int? productoId = null, int? sucursalId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            // "Hasta" es inclusivo para el usuario: se consulta hasta el inicio del día siguiente.
            var hastaExclusivo = hasta.Date.AddDays(1);

            return _repositorio
                .ObtenerTotalesPorProducto(desde.Date, hastaExclusivo, productoId, sucursalId)
                .Select(t => new ProduccionPorProductoDTO
                {
                    NombreProducto = t.NombreProducto,
                    Total = t.Total
                })
                .ToList();
        }
    }
}
