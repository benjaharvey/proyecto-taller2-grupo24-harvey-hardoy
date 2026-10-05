using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface ISucursalRepositorio
    {
        List<Sucursal> ObtenerTodos(bool incluirEliminadas = false);

        Sucursal? ObtenerPorId(int id);
        Sucursal? ObtenerPorNombre(string nombre);

        void Agregar(Sucursal sucursal);
        void Actualizar(Sucursal sucursal);
        void Eliminar(int id);
        void Reactivar(int id);
    }
}