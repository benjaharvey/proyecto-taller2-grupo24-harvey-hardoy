using Aplicacion.DTOs;
using Aplicacion.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class GenerarBackup
    {
        private readonly IBackupServicio _servicio;

        public GenerarBackup(IBackupServicio servicio)
        {
            _servicio = servicio;
        }

        public async Task<BackupResultadoGeneracionDTO> Ejecutar(string carpetaLocal)
        {
            var resultado = new BackupResultadoGeneracionDTO();

            string rutaLocal = await _servicio.GenerarBackupAsync(carpetaLocal);

            resultado.Backup = await _servicio.LeerMetadatosAsync(rutaLocal);
            resultado.Backup.UbicacionLocal = rutaLocal;

            string? oneDrive = Environment.GetEnvironmentVariable("OneDrive");
            if (string.IsNullOrWhiteSpace(oneDrive))
            {
                resultado.Advertencia = "OneDrive no está disponible en este equipo.";
                return resultado;
            }

            try
            {
                string carpetaNube = Path.Combine(oneDrive, "BackupsMaie");
                Directory.CreateDirectory(carpetaNube);
                string destino = Path.Combine(carpetaNube, resultado.Backup.NombreArchivo);
                File.Copy(rutaLocal, destino, overwrite: true);
                resultado.Backup.UbicacionNube = destino;
            }
            catch (Exception ex)
            {
                resultado.Advertencia = $"No se pudo copiar a OneDrive: {ex.Message}";
            }

            return resultado;
        }
    }
}