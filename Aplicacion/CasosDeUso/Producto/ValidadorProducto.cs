using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorProducto
    {
        public static void Validar(ProductoCrearDTO dto, IProductoRepositorio repo, int? idExistente = null)
        {
            dto.Nombre = (dto.Nombre ?? "").Trim();

            ValidarNombre(dto.Nombre);
            ValidarPrecio(dto.Precio);
            ValidarCategoriaId(dto.CategoriaId);
            ValidarReceta(dto.Receta);

            var conMismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if(conMismoNombre != null && conMismoNombre.Id != idExistente)
            {
                throw new InvalidOperationException("Ya existe un producto con ese nombre.");
            }
        }

        private static void ValidarReceta(List<ProductoInsumoDTO>? receta)
        {
            if (receta == null || receta.Count == 0) return;

            foreach (var item in receta)
            {
                if (item.InsumoId <= 0)
                {
                    throw new InvalidOperationException("Cada insumo de la receta debe ser válido.");
                }

                if (item.CantidadNecesaria <= 0)
                {
                    throw new InvalidOperationException("La cantidad necesaria de cada insumo debe ser mayor a cero.");
                }
            }

            var insumosDuplicados = receta
                .GroupBy(i => i.InsumoId)
                .Any(g => g.Count() > 1);

            if (insumosDuplicados)
            {
                throw new InvalidOperationException("No se pueden repetir insumos en la misma receta.");
            }
        }

        private static void ValidarNombre(string valor)
        {
            if(valor.Length == 0)
            {
                throw new InvalidOperationException("Ingresá el nombre del producto.");
            }

            if(valor.Length < 2 || valor.Length > 40)
            {
                throw new InvalidOperationException("El nombre del producto debe tener entre 5 y 40 caracteres");
            }
        }

        private static void ValidarPrecio(int valor)
        {
            if(valor <= 0)
            {
                throw new InvalidOperationException("El precio no puede ser 0 o negativo. Reingrese");
            }
        }

        private static void ValidarCategoriaId(int valor)
        {
            if(valor <= 0)
            {
                throw new InvalidOperationException("Ingrese una categoria valida");
            }
        }
    }
}