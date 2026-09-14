using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IRolRepositorio
    {
        List<Rol> ObtenerTodos();

        Rol? ObtenerPorId(int id);
        Rol? ObtenerPorNombre(string nombre);

        void Agregar(Rol rol);
        void Actualizar(Rol rol);
        void Eliminar(int id);
    }
}