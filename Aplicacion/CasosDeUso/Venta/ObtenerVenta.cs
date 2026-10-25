using Aplicacion.DTOs;
using Aplicacion.Mapeadores;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerVenta
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerVenta(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public VentaDTO? Ejecutar(int ventaId)
        {
            var venta = _repositorio.ObtenerPorId(ventaId);
            return venta == null ? null : VentaMapper.ToDTO(venta);
        }
    }
}