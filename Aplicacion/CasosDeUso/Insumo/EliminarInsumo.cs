using System;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class EliminarInsumo
    {
        private readonly IInsumoRepositorio _repositorio;

        public EliminarInsumo(IInsumoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public void Ejecutar(int id)
        {
            var insumo = _repositorio.ObtenerPorId(id)
                ?? throw new InvalidOperationException("El insumo que intentás eliminar no existe.");

            if (_repositorio.EstaEnRecetaActiva(id))
            {
                throw new InvalidOperationException($"No se puede eliminar el insumo \"{insumo.Nombre}\" porque forma parte de la receta de uno o más productos activos.");
            }

            _repositorio.Eliminar(id);
        }
    }
}
