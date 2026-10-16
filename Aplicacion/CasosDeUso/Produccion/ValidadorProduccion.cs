using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorProduccion
    {
        // Devuelve el producto (con su receta) para que el caso de uso no tenga que volver a buscarlo.
        public static Producto Validar(
            ProduccionCrearDTO dto,
            IProductoRepositorio productoRepo,
            ISucursalRepositorio sucursalRepo,
            IUsuarioRepositorio usuarioRepo)
        {
            ValidarCantidad(dto.Cantidad);

            // ObtenerPorId excluye los dados de baja, así que null = no existe o está eliminado.
            var producto = productoRepo.ObtenerPorId(dto.ProductoId);
            if (producto == null)
            {
                throw new InvalidOperationException("El producto no existe o está dado de baja. Recargá la lista de productos.");
            }

            if (sucursalRepo.ObtenerPorId(dto.SucursalId) == null)
            {
                throw new InvalidOperationException("La sucursal no existe o está dada de baja. Recargá la lista de sucursales.");
            }

            if (usuarioRepo.ObtenerPorId(dto.UsuarioId) == null)
            {
                throw new InvalidOperationException("El usuario no existe o está dado de baja.");
            }

            if (producto.ProductoInsumos == null || producto.ProductoInsumos.Count == 0)
            {
                throw new InvalidOperationException("El producto no tiene receta cargada, no se puede registrar la producción.");
            }

            return producto;
        }

        private static void ValidarCantidad(int valor)
        {
            if (valor <= 0)
            {
                throw new InvalidOperationException("La cantidad a producir debe ser mayor a cero.");
            }
        }
    }
}
