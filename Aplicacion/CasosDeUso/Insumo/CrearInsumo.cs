using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearInsumo
    {
        private readonly IInsumoRepositorio _repositorio;

        public CrearInsumo(IInsumoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(InsumoDTO dto)
        {
            ValidadorInsumo.Validar(dto, _repositorio);

            var insumo = new Insumo
            {
                Nombre = dto.Nombre,
                Unidad = dto.Unidad,
                Stock = dto.Stock,
                StockMinimo = dto.StockMinimo
            };

            _repositorio.Agregar(insumo);
        }
    }
}
