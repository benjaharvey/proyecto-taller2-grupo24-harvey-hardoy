using Aplicacion.DTOs;
using Aplicacion.Mapeadores;
using Dominio.Consultas;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ListarVentas
    {
        private readonly IVentaRepositorio _repositorio;

        public ListarVentas(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<VentaDTO> Ejecutar(VentaFiltro filtro)
        {
            return _repositorio.ObtenerHistorial(filtro)
                .Select(VentaMapper.ToDTO)
                .ToList();
        }
    }
}