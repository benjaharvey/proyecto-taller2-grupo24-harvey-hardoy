using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class IniciarSesion
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IRolRepositorio _repositorioRol;

        public IniciarSesion(IUsuarioRepositorio repositorio, IRolRepositorio repositorioRol)
        {
            _repositorio = repositorio;
            _repositorioRol = repositorioRol;
        }

        public UsuarioDTO Ejecutar(string email, string contraseñaPlana)
        {
            var usuario = _repositorio.ObtenerPorEmail(email);

            if (usuario == null)
            {
                throw new InvalidOperationException("El usuario no existe.");
            }

            bool passwordValida;
            try
            {
                passwordValida = BCrypt.Net.BCrypt.Verify(contraseñaPlana, usuario.Password);
            }
            catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException or FormatException)
            {
                // El valor guardado no es un hash bcrypt válido (por ejemplo, un registro cargado a mano por SQL).
                throw new InvalidOperationException(
                    "La contraseña de este usuario está dañada. Pedile al administrador que la restablezca.");
            }

            if (passwordValida == false)
            {
                throw new InvalidOperationException("Contraseña incorrecta");
            }

            var rol = _repositorioRol.ObtenerPorId(usuario.RolId);

            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Dni = usuario.Dni,
                Email = usuario.Email,
                FechaNacimiento = usuario.FechaNacimiento,
                Direccion = usuario.Direccion,
                RolId = usuario.RolId,
                NombreRol = rol?.Nombre ?? "",
                SucursalId = usuario.SucursalId
            };
        }
    }
}
