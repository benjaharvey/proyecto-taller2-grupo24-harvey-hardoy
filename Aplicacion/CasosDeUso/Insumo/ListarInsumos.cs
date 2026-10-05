using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarInsumos
    {
        private readonly IInsumoRepositorio _repositorio;

        public ListarInsumos(IInsumoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<InsumoDTO> Ejecutar(bool incluirEliminados = false)
        {
            var insumos = _repositorio.ObtenerTodos(incluirEliminados);
            var resultado = new List<InsumoDTO>();

            foreach (var insumo in insumos)
            {
                resultado.Add(new InsumoDTO
                {
                    Id = insumo.Id,
                    Nombre = insumo.Nombre,
                    Unidad = insumo.Unidad,
                    Stock = insumo.Stock,
                    StockMinimo = insumo.StockMinimo,
                    Activo = insumo.DeletedAt == null,
                    DeletedAt = insumo.DeletedAt
                });
            }

            return resultado;
        }
    }
}
