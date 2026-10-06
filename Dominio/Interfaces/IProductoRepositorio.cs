using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IProductoRepositorio
    {
        List<Producto> ObtenerTodos();

        Producto? ObtenerPorId(int id);
        Producto? ObtenerPorNombre(string nombre);

        int ContarPorCategoria(int categoriaId);
        void Agregar(Producto producto);
        void Actualizar(Producto producto);
        void Eliminar(int id);

/* CONSULTAR EN CLASE CON EL PROFESOR SI ES NECESARIO TENER UN REACTIVAR PRODUCTO */
        void Reactivar(int id);
    }
}