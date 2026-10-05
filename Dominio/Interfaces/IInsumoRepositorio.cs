using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IInsumoRepositorio
    {
        List<Insumo> ObtenerTodos(bool incluirEliminados = false);

        Insumo? ObtenerPorId(int id);
        Insumo? ObtenerPorNombre(string nombre);

        bool EstaEnRecetaActiva(int id);

        void Agregar(Insumo insumo);
        void Actualizar(Insumo insumo);
        void Eliminar(int id);
        void Reactivar(int id);
    }
}
