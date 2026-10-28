using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IProduccionRepositorio
    {
        void Agregar(Produccion produccion, List<ProductoInsumo> receta);
        List<Produccion> ObtenerTodos();

        // Total elaborado por producto en el rango [desde, hastaExclusivo). Los filtros null no se aplican.
        List<(string NombreProducto, int Total)> ObtenerTotalesPorProducto(
            DateTime desde, DateTime hastaExclusivo, int? productoId, int? sucursalId);
    }
}