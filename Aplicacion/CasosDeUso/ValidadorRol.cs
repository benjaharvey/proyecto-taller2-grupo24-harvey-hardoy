using System.Text.RegularExpressions;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorRol
    {
        private const int MinNombre = 3;
        private const int MaxNombre = 30;

        /// <summary>Roles de los que depende la autorización: no se pueden renombrar ni eliminar.</summary>
        public static readonly string[] RolesDelSistema = { "Admin", "Cocinero", "Vendedor" };

        private static readonly Regex RegexNombre = new(@"^\p{L}+([ \-]\p{L}+)*$", RegexOptions.Compiled);

        public static bool EsDelSistema(string nombre) =>
            RolesDelSistema.Contains(nombre, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Normaliza (trim) el DTO y valida el nombre. Lanza InvalidOperationException con el motivo.
        /// </summary>
        /// <param name="idExistente">Id del rol que se está editando (null en el alta).</param>
        public static void Validar(RolDTO dto, IRolRepositorio repo, int? idExistente = null)
        {
            dto.Nombre = (dto.Nombre ?? "").Trim();

            if (dto.Nombre.Length == 0)
                throw new InvalidOperationException("Ingresá el nombre del rol.");
            if (dto.Nombre.Length < MinNombre || dto.Nombre.Length > MaxNombre)
                throw new InvalidOperationException($"El nombre del rol debe tener entre {MinNombre} y {MaxNombre} caracteres.");
            if (!RegexNombre.IsMatch(dto.Nombre))
                throw new InvalidOperationException("El nombre del rol solo puede contener letras, espacios y guiones.");

            if (idExistente is int id)
            {
                var actual = repo.ObtenerPorId(id)
                    ?? throw new InvalidOperationException("El rol que intentás modificar ya no existe.");

                // La autorización compara el nombre del rol, así que renombrarlo dejaría sin permisos a sus usuarios.
                if (EsDelSistema(actual.Nombre) && !actual.Nombre.Equals(dto.Nombre, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"\"{actual.Nombre}\" es un rol del sistema y no se puede renombrar.");
            }

            var conMismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if (conMismoNombre != null && conMismoNombre.Id != idExistente)
                throw new InvalidOperationException("Ya existe un rol con ese nombre.");
        }
    }
}
