using Aplicacion.DTOs;

namespace Aplicacion.DTOs
{
    public class BackupResultadoGeneracionDTO
    {
        public BackupDTO Backup { get; set; } = new BackupDTO();
        public string Advertencia { get; set; } = string.Empty;
    }
}