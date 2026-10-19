using Dominio.Entidades;
using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public class RegistrarProduccion
    {
        private readonly IProduccionRepositorio _repositorio;
        private readonly IProductoRepositorio _productoRepositorio;
        private readonly ISucursalRepositorio _sucursalRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public RegistrarProduccion(
            IProduccionRepositorio repo,
            IProductoRepositorio productoRepo,
            ISucursalRepositorio sucursalRepo,
            IUsuarioRepositorio usuarioRepo)
        {
            _repositorio = repo;
            _productoRepositorio = productoRepo;
            _sucursalRepositorio = sucursalRepo;
            _usuarioRepositorio = usuarioRepo;
        }

        public void Ejecutar(ProduccionCrearDTO dto)
        {
            var producto = ValidadorProduccion.Validar(dto, _productoRepositorio, _sucursalRepositorio, _usuarioRepositorio);

            var nuevaProduccion = new Produccion
            {
                ProductoId = dto.ProductoId,
                SucursalId = dto.SucursalId,
                UsuarioId = dto.UsuarioId,
                Cantidad = dto.Cantidad
            };

            // La receta del producto le dice al repositorio cuánto insumo descontar.
            _repositorio.Agregar(nuevaProduccion, producto.ProductoInsumos.ToList());
        }
    }
}
