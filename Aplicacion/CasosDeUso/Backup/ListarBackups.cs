using Aplicacion.DTOs;
using Aplicacion.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class ListarBackups
    {
        private readonly IBackupServicio _servicio;

        public ListarBackups(IBackupServicio servicio)
        {
            _servicio = servicio;
        }

        public Task<IEnumerable<BackupDTO>> Ejecutar(string carpeta)
            => _servicio.ListarBackupsAsync(carpeta);
    }
}