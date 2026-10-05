using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorInsumo
    {
        private const int MinNombre = 2;
        private const int MaxNombre = 50;
        private const int MinUnidad = 1;
        private const int MaxUnidad = 20;

        public static void Validar(InsumoDTO dto, IInsumoRepositorio repo, int? idExistente = null)
        {
            dto.Nombre = (dto.Nombre ?? "").Trim();
            dto.Unidad = (dto.Unidad ?? "").Trim();

            if (dto.Nombre.Length == 0)
                throw new InvalidOperationException("Ingresá el nombre del insumo.");
            if (dto.Nombre.Length < MinNombre || dto.Nombre.Length > MaxNombre)
                throw new InvalidOperationException($"El nombre del insumo debe tener entre {MinNombre} y {MaxNombre} caracteres.");

            if (dto.Unidad.Length == 0)
                throw new InvalidOperationException("Ingresá la unidad de medida del insumo.");
            if (dto.Unidad.Length < MinUnidad || dto.Unidad.Length > MaxUnidad)
                throw new InvalidOperationException($"La unidad de medida debe tener entre {MinUnidad} y {MaxUnidad} caracteres.");

            if (dto.Stock < 0)
                throw new InvalidOperationException("El stock no puede ser negativo.");

            dto.Stock = Math.Round(dto.Stock, 3, MidpointRounding.AwayFromZero);

            if (dto.StockMinimo < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");

            dto.StockMinimo = Math.Round(dto.StockMinimo, 3, MidpointRounding.AwayFromZero);

            if (idExistente is int id)
            {
                var actual = repo.ObtenerPorId(id)
                    ?? throw new InvalidOperationException("El insumo que intentás modificar ya no existe.");
            }

            var conMismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if (conMismoNombre != null && conMismoNombre.DeletedAt == null && conMismoNombre.Id != idExistente)
                throw new InvalidOperationException("Ya existe un insumo con ese nombre.");
        }
    }
}
