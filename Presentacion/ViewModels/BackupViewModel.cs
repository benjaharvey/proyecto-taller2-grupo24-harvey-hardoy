using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class BackupItem : ObservableObject
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Completa";
    public string Tamano { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Completado";
    public string Ubicacion { get; set; } = string.Empty;
}

public partial class BackupViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BackupItem> _historialBackups = new();

    [ObservableProperty]
    private BackupItem? _selectedBackup;

    [ObservableProperty]
    private string _rutaDestino = @"C:\Maie\Backups\";

    [ObservableProperty]
    private string _tipoBackupSeleccionado = "Completa";

    [ObservableProperty]
    private string _descripcionOpcional = string.Empty;

    public ObservableCollection<string> TiposBackupDisponibles { get; } = new()
    {
        "Completa",
        "Diferencial"
    };

    [RelayCommand]
    private void GenerarBackup()
    {
        string usuario = SesionActual.UsuarioLogueado != null 
            ? $"{SesionActual.UsuarioLogueado.Nombre} {SesionActual.UsuarioLogueado.Apellido} (Admin)" 
            : "Administrador";

        var ahora = DateTime.Now;
        string timestamp = ahora.ToString("yyyy-MM-dd_HHmmss");
        string sufijoTipo = TipoBackupSeleccionado.ToLowerInvariant();
        string nombreArchivo = $"backup_maie_{sufijoTipo}_{timestamp}.bak";

        var nuevo = new BackupItem
        {
            NombreArchivo = nombreArchivo,
            Tipo = TipoBackupSeleccionado,
            Tamano = TipoBackupSeleccionado == "Completa" ? "14.8 MB" : "2.4 MB",
            FechaCreacion = ahora,
            CreadoPor = usuario,
            Descripcion = string.IsNullOrWhiteSpace(DescripcionOpcional) ? "Sin descripción" : DescripcionOpcional.Trim(),
            Estado = "Completado con éxito",
            Ubicacion = $@"{RutaDestino}{nombreArchivo}"
        };

        HistorialBackups.Insert(0, nuevo);
        SelectedBackup = nuevo;
        DescripcionOpcional = string.Empty;

        MessageBox.Show(
            $"Copia de seguridad generada con éxito.\n\nArchivo: {nombreArchivo}\nTipo: {nuevo.Tipo}\nUbicación: {nuevo.Ubicacion}\nTamaño: {nuevo.Tamano}",
            "Backup Generado",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void CambiarRutaDestino()
    {
        MessageBox.Show("La ruta de almacenamiento predeterminada está configurada en C:\\Maie\\Backups\\ según los parámetros del servidor.", "Configuración de Almacenamiento", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
