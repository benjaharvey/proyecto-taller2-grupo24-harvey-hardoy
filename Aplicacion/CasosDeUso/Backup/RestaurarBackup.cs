using Aplicacion.Interfaces;

namespace Aplicacion.CasosDeUso
{
    public class RestaurarBackup
    {
        private readonly IBackupServicio _servicio;

        public RestaurarBackup(IBackupServicio servicio)
        {
            _servicio = servicio;
        }

        public async Task Ejecutar(string rutaBak)
        {
            if (string.IsNullOrWhiteSpace(rutaBak) || !File.Exists(rutaBak))
                throw new FileNotFoundException($"El archivo de backup no existe: {rutaBak}");

            await _servicio.RestaurarBackupAsync(rutaBak);
        }
    }
}