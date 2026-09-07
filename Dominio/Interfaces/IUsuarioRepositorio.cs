using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IUsuarioRepositorio
    {
        List<Usuario> obtenerTodos();
        Usuario? ObtenerPorId(int id);
        void Agregar(Usuario usuario);
        void Actualizar(Usuario usuario);
        void Eliminar(int id);
    }
}