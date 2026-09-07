using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IRolRepositorio
    {
        List<Rol> ObtenerTodos();
        Rol? ObtenerPorId(int id);
        void Agregar(Rol rol);
        void Actualizar(Rol rol);
        void Eliminar(int id);
    }
}