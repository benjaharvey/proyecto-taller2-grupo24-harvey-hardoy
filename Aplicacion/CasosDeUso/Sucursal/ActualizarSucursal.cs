using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarSucursal
    {
        private readonly ISucursalRepositorio _repositorio;

        public ActualizarSucursal(ISucursalRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(SucursalDTO dto)
        {
            ValidadorSucursal.Validar(dto, _repositorio, dto.Id);

            var sucursal = new Sucursal
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Ubicacion = dto.Ubicacion,
                Estado = dto.Estado
            };

            _repositorio.Actualizar(sucursal);
        }
    }
}
