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

            var conMismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if(conMismoNombre != null && conMismoNombre.Id != idExistente)
            {
                throw new InvalidOperationException("Ya existe un producto con ese nombre.");
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