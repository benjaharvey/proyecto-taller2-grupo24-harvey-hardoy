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

        public IniciarSesion(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public UsuarioDTO Ejecutar(string email, string contraseñaPlana)
        {
            var usuario = _repositorio.ObtenerPorEmail(email);

            if (usuario == null)
            {
                throw new InvalidOperationException("El usuario no existe.");
            }

            bool passwordValida = BCrypt.Net.BCrypt.Verify(contraseñaPlana, usuario.Password);

            if (passwordValida == false)
            {
                throw new InvalidOperationException("Contraseña incorrecta");
            }

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
                SucursalId = usuario.SucursalId
            };
        }
    }
}