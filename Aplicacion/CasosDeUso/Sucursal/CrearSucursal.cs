using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearSucursal
    {
        private readonly ISucursalRepositorio _repositorio;

        public CrearSucursal(ISucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(SucursalDTO dto)
        {
            ValidadorSucursal.Validar(dto, _repositorio);

            var sucursal = new Sucursal
            {
                Nombre = dto.Nombre,
                Ubicacion = dto.Ubicacion,
                Estado = dto.Estado
            };

            _repositorio.Agregar(sucursal);
        }

    }
}