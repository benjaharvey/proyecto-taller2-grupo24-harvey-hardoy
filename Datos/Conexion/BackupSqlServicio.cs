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
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string nombreBase = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(nombreBase))
                throw new InvalidOperationException("La cadena de conexión no define la base de datos a respaldar.");

            string baseEscapada = nombreBase.Replace("]", "]]");
            string nombreArchivo = $"backup_maie_completa_{DateTime.Now:yyyy-MM-dd_HHmmss}.bak";
            string rutaCompleta = Path.Combine(rutaDestino, nombreArchivo);

            Directory.CreateDirectory(rutaDestino);

            string sql = $@"BACKUP DATABASE [{baseEscapada}]
                   TO DISK = @ruta
                   WITH FORMAT, INIT, NAME = @nombre;";

            await using var conexion = new SqlConnection(_connectionString);
            await conexion.OpenAsync();

            await EjecutarSentenciaAsync(
                conexion,
                sql,
                new SqlParameter("@ruta", rutaCompleta),
                new SqlParameter("@nombre", $"Backup completo {DateTime.Now:yyyy-MM-dd HH:mm}"));

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

        public async Task RestaurarBackupAsync(string rutaBak)
        {
            if (!File.Exists(rutaBak))
                throw new FileNotFoundException($"El archivo de backup no existe: {rutaBak}", rutaBak);

            var builder = new SqlConnectionStringBuilder(_connectionString);
            string nombreBase = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(nombreBase))
                throw new InvalidOperationException("La cadena de conexión no define la base de datos a restaurar.");

            // Todas las sentencias de restore se ejecutan contra master
            builder.InitialCatalog = "master";
            string cadenaMaster = builder.ConnectionString;
            string baseEscapada = nombreBase.Replace("]", "]]");

            // El pool mantiene conexiones abiertas aunque el DbContext se haya liberado.
            SqlConnection.ClearAllPools();

            await using (var conexion = new SqlConnection(cadenaMaster))
            {
                await conexion.OpenAsync();
                await EjecutarSentenciaAsync(
                    conexion,
                    "RESTORE VERIFYONLY FROM DISK = @ruta;",
                    new SqlParameter("@ruta", rutaBak));
            }

            bool quedoEnSingleUser = false;

            try
            {
                await using var conexion = new SqlConnection(cadenaMaster);
                await conexion.OpenAsync();

                await EjecutarSentenciaAsync(
                    conexion,
                    $"ALTER DATABASE [{baseEscapada}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
                quedoEnSingleUser = true;

                await EjecutarSentenciaAsync(
                    conexion,
                    $"RESTORE DATABASE [{baseEscapada}] FROM DISK = @ruta WITH REPLACE, RECOVERY;",
                    new SqlParameter("@ruta", rutaBak));

                await EjecutarSentenciaAsync(
                    conexion,
                    $"ALTER DATABASE [{baseEscapada}] SET MULTI_USER;");
                quedoEnSingleUser = false;
            }
            finally
            {
                // Si el restore falla, la base queda en SINGLE_USER y la app no puede reconectarse.
                if (quedoEnSingleUser)
                {
                    try
                    {
                        await using var conexion = new SqlConnection(cadenaMaster);
                        await conexion.OpenAsync();
                        await EjecutarSentenciaAsync(
                            conexion,
                            $"ALTER DATABASE [{baseEscapada}] SET MULTI_USER;");
                    }
                    catch
                    {
                        // la base quedo en SINGLE_USER y requiere modificacion manual.
                    }
                }
            }
        }

        private static async Task EjecutarSentenciaAsync(SqlConnection conexion, string sql, params SqlParameter[] parametros)
        {
            await using var comando = new SqlCommand(sql, conexion);

            if (parametros.Length > 0)
                comando.Parameters.AddRange(parametros);

            await comando.ExecuteNonQueryAsync();
        }

        private static string FormatearTamano(long bytes) =>
            bytes >= 1024L * 1024L * 1024L ? $"{bytes / (double)(1024L * 1024L * 1024L):0.0} GB"
            : bytes >= 1024L * 1024L ? $"{bytes / (double)(1024L * 1024L):0.0} MB"
            : $"{bytes / 1024.0:0.0} KB";
    }
}


