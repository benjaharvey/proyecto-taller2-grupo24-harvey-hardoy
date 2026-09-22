using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IUsuarioRepositorio
    {
        List<Usuario> ObtenerTodos();

        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorDni(string dni);

        void Agregar(Usuario usuario);
        void Actualizar(Usuario usuario);
        void Eliminar(int id);
    }
}