using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IStockSucursalRepositorio
    {
        List<StockSucursal> ObtenerPorSucursal(int sucursalId, bool soloActivos = true);

        List<StockSucursal> ObtenerTodas(bool soloActivos = true);

        StockSucursal? ObtenerPorSucursalYProducto(int sucursalId, int productoId);

        StockSucursal Ajustar(int sucursalId, int productoId, decimal cantidadResultante);

        void Incrementar(int sucursalId, int productoId, decimal cantidad);
    }
}
