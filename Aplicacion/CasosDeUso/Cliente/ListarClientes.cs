using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class ListarClientes
    {
        private readonly IClienteRepositorio _repositorio;

        public ListarClientes(IClienteRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public List<ClienteDTO> Ejecutar()
        {
            var resultado = new List<ClienteDTO>();
            foreach (var cliente in _repositorio.ObtenerTodos())
            {
                resultado.Add(new ClienteDTO
                {
                    Id = cliente.Id,
                    UsuarioId = cliente.UsuarioId,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Dni = cliente.Dni,
                    Email = cliente.Email,
                    // El repositorio hace Include(c => c.Usuario), por eso no hace falta pedir el usuario aparte.
                    NombreRegistradoPor = cliente.Usuario != null
                        ? $"{cliente.Usuario.Nombre} {cliente.Usuario.Apellido}"
                        : "Desconocido",
                    FechaNacimiento = cliente.FechaNacimiento,
                    CreatedAt = cliente.CreatedAt,
                    Activo = cliente.DeletedAt == null,
                    DeletedAt = cliente.DeletedAt
                });
            }
            return resultado;
        }
    }
}
