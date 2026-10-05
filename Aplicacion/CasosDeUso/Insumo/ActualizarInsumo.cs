using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarInsumo
    {
        private readonly IInsumoRepositorio _repositorio;

        public ActualizarInsumo(IInsumoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(InsumoDTO dto)
        {
            ValidadorInsumo.Validar(dto, _repositorio, dto.Id);

            var insumo = new Insumo
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Unidad = dto.Unidad,
                Stock = dto.Stock,
                StockMinimo = dto.StockMinimo
            };

            _repositorio.Actualizar(insumo);
        }
    }
}
