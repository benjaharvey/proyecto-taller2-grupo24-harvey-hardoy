using Aplicacion.DTOs;
using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerVentasPorVendedor
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerVentasPorVendedor(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<VentaPorVendedorDTO> Ejecutar(DateTime desde, DateTime hasta, int? sucursalId = null)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            var hastaExclusivo = hasta.Date.AddDays(1);

            return _repositorio.ObtenerVentasPorVendedor(desde.Date, hastaExclusivo, sucursalId)
            .Select(t => new VentaPorVendedorDTO
            {
                NombreVendedor = t.NombreVendedor,
                CantidadVentas = t.CantidadVentas,
                TotalRecaudado = t.TotalRecaudado
            }).ToList();
        }
    }
}