using System.Net.Mail;
using System.Text.RegularExpressions;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorCliente
    {
        private const int MaxNombre = 50;
        private const int MaxEmail = 100;
        private const int EdadMaxima = 100;

        private static readonly Regex RegexNombre = new(@"^\p{L}+([ '\-]\p{L}+)*$", RegexOptions.Compiled);
        private static readonly Regex RegexDni = new(@"^\d{7,8}$", RegexOptions.Compiled);

        /// <summary>
        /// Normaliza (trim) el DTO y valida cada campo. Lanza InvalidOperationException con el motivo.
        /// </summary>
        /// <param name="idExistente">Id del cliente que se está editando (null en el alta).</param>
        public static void Validar(ClienteDTO dto, IClienteRepositorio repo, int? idExistente = null)
        {
            dto.Nombre = (dto.Nombre ?? "").Trim();
            dto.Apellido = (dto.Apellido ?? "").Trim();
            dto.Dni = (dto.Dni ?? "").Trim();
            dto.Email = (dto.Email ?? "").Trim();

            ValidarNombre(dto.Nombre, "nombre");
            ValidarNombre(dto.Apellido, "apellido");

            if (!RegexDni.IsMatch(dto.Dni))
                throw new InvalidOperationException("El DNI debe tener 7 u 8 dígitos, sin puntos ni espacios.");

            ValidarEmail(dto.Email);
            ValidarFechaNacimiento(dto.FechaNacimiento);

            // Solo en el alta: al editar, el usuario que registró al cliente no cambia.
            if (idExistente is null && dto.UsuarioId <= 0)
                throw new InvalidOperationException("No se pudo identificar al usuario que registra el cliente.");

            var conMismoEmail = repo.ObtenerPorEmail(dto.Email);
            if (conMismoEmail != null && conMismoEmail.Id != idExistente)
                throw new InvalidOperationException("Ya existe un cliente con ese email.");

            var conMismoDni = repo.ObtenerPorDni(dto.Dni);
            if (conMismoDni != null && conMismoDni.Id != idExistente)
                throw new InvalidOperationException("Ya existe un cliente con ese DNI.");
        }

        private static void ValidarNombre(string valor, string campo)
        {
            if (valor.Length == 0)
                throw new InvalidOperationException($"Ingresá el {campo}.");
            if (valor.Length < 2 || valor.Length > MaxNombre)
                throw new InvalidOperationException($"El {campo} debe tener entre 2 y {MaxNombre} caracteres.");
            if (!RegexNombre.IsMatch(valor))
                throw new InvalidOperationException($"El {campo} solo puede contener letras, espacios, guiones y apóstrofes.");
        }

        private static void ValidarEmail(string email)
        {
            if (email.Length == 0)
                throw new InvalidOperationException("Ingresá el email.");
            if (email.Length > MaxEmail)
                throw new InvalidOperationException($"El email no puede superar los {MaxEmail} caracteres.");

            // MailAddress acepta formas como "Juan <a@b.com>" o "a@b" — se exige que sea solo la dirección y con dominio con punto.
            var valido = MailAddress.TryCreate(email, out var direccion)
                         && direccion.Address == email
                         && direccion.Host.Contains('.')
                         && !direccion.Host.EndsWith('.');
            if (!valido)
                throw new InvalidOperationException("El email no tiene un formato válido (ej: nombre@dominio.com).");
        }

        private static void ValidarFechaNacimiento(DateTime fecha)
        {
            var hoy = DateTime.Today;
            if (fecha.Date > hoy)
                throw new InvalidOperationException("La fecha de nacimiento no puede ser futura.");

            var edad = hoy.Year - fecha.Year;
            if (fecha.Date > hoy.AddYears(-edad)) edad--;

            if (edad > EdadMaxima)
                throw new InvalidOperationException("La fecha de nacimiento no es válida.");
        }
    }
}
