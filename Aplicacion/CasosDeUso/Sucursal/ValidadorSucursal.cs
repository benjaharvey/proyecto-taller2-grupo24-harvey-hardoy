using Dominio.Interfaces;
using Aplicacion.DTOs;

namespace Aplicacion.CasosDeUso
{
    public static class ValidadorSucursal
    {
        private const int MinNombre = 3;
        private const int MaxNombre = 30;
        private const int MinUbicacion = 10;
        private const int MaxUbicacion = 150;

        public static void Validar(SucursalDTO dto, ISucursalRepositorio repo, int? idExistente = null)
        {
            // Normalizacion de campos
            dto.Nombre = (dto.Nombre ?? "").Trim();
            dto.Ubicacion = (dto.Ubicacion ?? "").Trim();
            dto.Estado = (dto.Estado ?? "ACTIVA").Trim().ToUpper();

            // Validacion de campos de nombre
            if (dto.Nombre.Length == 0)
                throw new InvalidOperationException("Ingresá el nombre de la sucursal.");
            if (dto.Nombre.Length < MinNombre || dto.Nombre.Length > MaxNombre)
                throw new InvalidOperationException($"El nombre de la sucursal debe tener entre {MinNombre} y {MaxNombre} caracteres.");

            // Validacion de campos de ubicacion
            if (dto.Ubicacion.Length == 0)
                throw new InvalidOperationException("Ingresá la ubicacion de la sucursal.");
            if (dto.Ubicacion.Length < MinUbicacion || dto.Ubicacion.Length > MaxUbicacion)
                throw new InvalidOperationException($"La ubicacion de la sucursal debe tener entre {MinUbicacion} y {MaxUbicacion} caracteres.");
            
            // Validacion de campos de estado
            if (dto.Estado != "ACTIVA" && dto.Estado != "INACTIVA")
            throw new InvalidOperationException("El estado debe ser ACTIVA o INACTIVA.");

            if (idExistente is int id)
            {
                var actual = repo.ObtenerPorId(id)
                    ?? throw new InvalidOperationException("La sucursal que intentás modificar ya no existe.");
            }

            var conMismoNombre = repo.ObtenerPorNombre(dto.Nombre);
            if (conMismoNombre != null && conMismoNombre.Id != idExistente)
                throw new InvalidOperationException("Ya existe una sucursal con ese nombre.");
        }
    }
}