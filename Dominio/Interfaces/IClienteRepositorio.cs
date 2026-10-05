namespace Dominio.Interfaces
{
    public interface IClienteRepositorio
    {
        List<Cliente> ObtenerTodos();

        Cliente? ObtenerPorId(int id);
        Cliente? ObtenerPorEmail(string email);
        Cliente? ObtenerPorDni(string dni);

        void Agregar(Cliente cliente);
        void Actualizar(Cliente cliente);
        void Eliminar(int id);
        void Reactivar(int id);
    }
}