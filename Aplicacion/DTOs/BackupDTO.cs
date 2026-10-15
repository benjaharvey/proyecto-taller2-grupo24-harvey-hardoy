namespace Aplicacion.DTOs
{
    public class BackupDTO
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaCompleta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public string TamanoLegible { get; set; } = string.Empty;
        public DateTime FechaBackup { get; set; }
        public string CreadoPor { get; set; } = string.Empty;
        public string UbicacionLocal { get; set; } = string.Empty;
        public string UbicacionNube { get; set; } = string.Empty;
    }
}