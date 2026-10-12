using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class AjustarStockProducto
    {
        private readonly IStockSucursalRepositorio _repositorio;

        public AjustarStockProducto(IStockSucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public AjusteStockDTO Ejecutar(int sucursalId, int productoId, decimal cantidadResultante)
        {
            if (cantidadResultante < 0)
                throw new InvalidOperationException(
                    "La cantidad resultante no puede ser negativa. Revisá el valor ingresado.");

            var anterior = _repositorio.ObtenerPorSucursalYProducto(sucursalId, productoId);
            var cantidadAnterior = anterior?.Cantidad ?? 0m;

            var stock = _repositorio.Ajustar(sucursalId, productoId, cantidadResultante);

            return new AjusteStockDTO
            {
                CantidadAnterior = cantidadAnterior,
                CantidadResultante = stock.Cantidad
            };
        }
    }
}
