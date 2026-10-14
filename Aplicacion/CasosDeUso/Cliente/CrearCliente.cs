using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class CrearCliente
    {
        private readonly IClienteRepositorio _repositorio;

        public CrearCliente(IClienteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(ClienteDTO dto)
        {
            ValidadorCliente.Validar(dto, _repositorio);

            var nuevoCliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento,
                UsuarioId = dto.UsuarioId
            };
            _repositorio.Agregar(nuevoCliente);
        }
    }
}