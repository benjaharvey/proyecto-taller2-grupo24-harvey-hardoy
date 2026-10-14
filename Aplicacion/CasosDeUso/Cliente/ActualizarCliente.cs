using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ActualizarCliente
    {
        private readonly IClienteRepositorio _repositorio;

        public ActualizarCliente(IClienteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id, ClienteDTO dto)
        {
            ValidadorCliente.Validar(dto, _repositorio, id);

            var cliente = new Cliente
            {
                Id = id,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento
            };
            _repositorio.Actualizar(cliente);
        }
    }
}