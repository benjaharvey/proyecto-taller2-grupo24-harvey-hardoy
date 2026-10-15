using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Microsoft.Data.SqlClient;
using System.IO;

namespace Datos.Conexion
{
    public class BackupSqlServicio : IBackupServicio
    {
        private readonly string _connectionString;

        public BackupSqlServicio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<string> GenerarBackupAsync(string rutaDestino)
        {
            string nombreArchivo = $"backup_maie_completa_{DateTime.Now:yyyy-MM-dd_HHmmss}.bak";
            string rutaCompleta = Path.Combine(rutaDestino, nombreArchivo);

            Directory.CreateDirectory(rutaDestino);

            string sql = @"BACKUP DATABASE [ProyectoCRUD]
                   TO DISK = @ruta
                   WITH FORMAT, INIT, NAME = @nombre;";

            await using var conexion = new SqlConnection(_connectionString);
            await using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@ruta", rutaCompleta);
            comando.Parameters.AddWithValue("@nombre", $"Backup completo {DateTime.Now:yyyy-MM-dd HH:mm}");

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();

            return rutaCompleta;
        }

        public async Task<IEnumerable<BackupDTO>> ListarBackupsAsync(string carpeta)
        {
            if (!Directory.Exists(carpeta))
                return Enumerable.Empty<BackupDTO>();

            var backups = new List<BackupDTO>();

            foreach (string ruta in Directory.EnumerateFiles(carpeta, "*.bak"))
            {
                try
                {
                    backups.Add(await LeerMetadatosAsync(ruta));
                }
                catch (Exception)
                {
                    backups.Add(new BackupDTO
                    {
                        NombreArchivo = Path.GetFileName(ruta),
                        RutaCompleta = ruta,
                        TamanoBytes = new FileInfo(ruta).Length,
                        TamanoLegible = FormatearTamano(new FileInfo(ruta).Length),
                    });
                }
            }

            return backups.OrderByDescending(b => b.FechaBackup);
        }

        public async Task<BackupDTO> LeerMetadatosAsync(string rutaBak)
        {
            string sql = "RESTORE HEADERONLY FROM DISK = @ruta;";

            await using var conexion = new SqlConnection(_connectionString);
            await using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@ruta", rutaBak);

            await conexion.OpenAsync();
            await using var reader = await comando.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new InvalidOperationException(
                    $"El archivo '{rutaBak}' no contiene un backup válido de SQL Server.");

            return new BackupDTO
            {
                NombreArchivo = Path.GetFileName(rutaBak),
                RutaCompleta = rutaBak,
                TamanoBytes = new FileInfo(rutaBak).Length,
                TamanoLegible = FormatearTamano(new FileInfo(rutaBak).Length),
                FechaBackup = reader["BackupStartDate"] as DateTime? ?? DateTime.MinValue,
                CreadoPor = reader["SQLUserName"]?.ToString() ?? "desconocido",
            };
        }

        public Task RestaurarBackupAsync(string rutaBak)
            => throw new NotImplementedException("Se implementa en el commit de restore.");
        
        private static string FormatearTamano(long bytes) =>
            bytes >= 1024L * 1024L * 1024L ? $"{bytes / (double)(1024L * 1024L * 1024L):0.0} GB"
            : bytes >= 1024L * 1024L ? $"{bytes / (double)(1024L * 1024L):0.0} MB"
            : $"{bytes / 1024.0:0.0} KB";
    }
}


