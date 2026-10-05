using System.Text.RegularExpressions;
using Dominio.Interfaces;
using Aplicacion.DTOs;


namespace Aplicacion.CasosDeUso
{
    public static class ValidadorCategoria
    {
        private static readonly Regex RegexNombre = new(@"^\p{L}+([ \-]\p{L}+)*$", RegexOptions.Compiled);

        public static void Validar(CategoriaDTO dto, ICategoriaRepositorio repo, int? idExistente = null)
        {
            dto.Nombre = (dto.Nombre ?? "").Trim();

            if(dto.Nombre.Length == 0)
            {
                throw new InvalidOperationException("Ingrese el nombre de la categoria");
            }

            if(dto.Nombre.Length < 3 || dto.Nombre.Length > 30)
            {
                throw new InvalidOperationException("El nombre de la categoria debe tener entre 3 y 30 caracteres");
            }

            if (!RegexNombre.IsMatch(dto.Nombre))
            {
                throw new InvalidOperationException("El nombre de la categoria solo puede contener letras, espacios y guiones");
            }

            var mismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if(mismoNombre != null && mismoNombre.Id != idExistente)
            {
                throw new InvalidOperationException("Ya existe una categoria con ese mismo nombre");
            }
        }

    }
}