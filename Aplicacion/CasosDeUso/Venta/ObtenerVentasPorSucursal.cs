using Aplicacion.DTOs;
using Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.CasosDeUso
{
    public class ObtenerVentasPorSucursal
    {
        private readonly IVentaRepositorio _repositorio;

        public ObtenerVentasPorSucursal(IVentaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<VentaPorSucursalDTO> Ejecutar(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
            {
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            var hastaExclusivo = hasta.Date.AddDays(1);

            return _repositorio.ObtenerVentasPorSucursal(desde.Date, hastaExclusivo)
            .Select(t => new VentaPorSucursalDTO
            {
                NombreSucursal = t.NombreSucursal,
                CantidadVentas = t.CantidadVentas,
                TotalRecaudado = t.TotalRecaudado
            }).ToList();
        }
    }
}
