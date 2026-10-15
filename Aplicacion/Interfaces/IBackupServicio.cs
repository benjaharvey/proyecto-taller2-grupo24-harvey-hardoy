using Aplicacion.DTOs;
using System.Threading.Tasks;

namespace Aplicacion.Interfaces
{
    public interface IBackupServicio
    {
        Task<string> GenerarBackupAsync(string rutaDestino);
        Task RestaurarBackupAsync(string rutaBak);
        Task<IEnumerable<BackupDTO>> ListarBackupsAsync(string carpeta);
        Task<BackupDTO> LeerMetadatosAsync(string rutaBak);
    }
}