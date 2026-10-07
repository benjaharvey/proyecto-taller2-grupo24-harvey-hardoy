using System;
using Dominio.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class EliminarSucursal
    {
        private static readonly string[] SucursalesDelSistema = { "Casa Central", "Fábrica" };

        private readonly ISucursalRepositorio _repositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public EliminarSucursal(ISucursalRepositorio repositorio, IUsuarioRepositorio usuarioRepositorio)
        {
            _repositorio = repositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public void Ejecutar(int id)
        {
            var sucursal = _repositorio.ObtenerPorId(id)
                ?? throw new InvalidOperationException("La sucursal que intentás eliminar no existe.");

            if (EsDelSistema(sucursal.Nombre))
            {
                throw new InvalidOperationException(
                    $"La sucursal \"{sucursal.Nombre}\" es estructural del sistema y no se puede eliminar.");
            }

            if (_usuarioRepositorio.ContarPorSucursal(id) > 0)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar la sucursal porque tiene usuarios activos asignados. Reasigná a los empleados primero.");
            }

            _repositorio.Eliminar(id);
        }

        public static bool EsDelSistema(string nombre)
        {
            foreach (var s in SucursalesDelSistema)
            {
                if (s.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
