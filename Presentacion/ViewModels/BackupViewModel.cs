using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace Presentacion.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly GenerarBackup _generarBackup;
    private readonly ListarBackups _listarBackups;

    [ObservableProperty]
    private ObservableCollection<BackupDTO> _historialBackups = new();

    [ObservableProperty]
    private BackupDTO? _backupSeleccionado;

    [ObservableProperty]
    private string _rutaDestinoLocal;

    [ObservableProperty]
    private bool _estaGenerando;

    [ObservableProperty]
    private bool _hayBackups;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    private bool _mensajeEsError;

    public bool OneDriveDisponible { get; }

    public string EstadoOneDrive { get; }

    public BackupViewModel(GenerarBackup generarBackup, ListarBackups listarBackups)
    {
        _generarBackup = generarBackup;
        _listarBackups = listarBackups;

        string carpetaPublica = Environment.GetEnvironmentVariable("PUBLIC")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

        RutaDestinoLocal = Path.Combine(carpetaPublica, "Maie", "Backups");

        string? oneDrive = Environment.GetEnvironmentVariable("OneDrive");
        OneDriveDisponible = !string.IsNullOrWhiteSpace(oneDrive);
        EstadoOneDrive = OneDriveDisponible
            ? Path.Combine(oneDrive!, "BackupsMaie")
            : "No disponible: OneDrive no está configurado en este equipo.";

        _ = CargarBackupsAsync();
    }

    [RelayCommand]
    private void SeleccionarCarpeta()
    {
        var dialogo = new OpenFolderDialog
        {
            Title = "Seleccionar carpeta para las copias de seguridad",
            InitialDirectory = Directory.Exists(RutaDestinoLocal)
                ? RutaDestinoLocal
                : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        if (dialogo.ShowDialog() == true)
        {
            RutaDestinoLocal = dialogo.FolderName;
            _ = CargarBackupsAsync();
        }
    }

    [RelayCommand]
    private async Task ActualizarListaAsync()
    {
        await CargarBackupsAsync();
    }

    [RelayCommand(CanExecute = nameof(PuedeGenerarBackup))]
    private async Task GenerarBackupAhoraAsync()
    {
        try
        {
            EstaGenerando = true;
            MensajeEsError = false;
            MensajeEstado = "Generando copia de seguridad...";

            BackupResultadoGeneracionDTO resultado = await _generarBackup.Ejecutar(RutaDestinoLocal);

            await CargarBackupsAsync();

            BackupSeleccionado = HistorialBackups.FirstOrDefault(
                b => string.Equals(b.RutaCompleta, resultado.Backup.RutaCompleta, StringComparison.OrdinalIgnoreCase));

            string mensaje = $"Copia de seguridad generada: {resultado.Backup.NombreArchivo} ({resultado.Backup.TamanoLegible}).";

            if (!string.IsNullOrWhiteSpace(resultado.Backup.UbicacionNube))
                mensaje += " Copia en OneDrive realizada correctamente.";

            if (!string.IsNullOrWhiteSpace(resultado.Advertencia))
                mensaje += $" Advertencia: {resultado.Advertencia}";

            MensajeEstado = mensaje;
        }
        catch (Exception ex)
        {
            MensajeEsError = true;
            MensajeEstado = $"No se pudo generar la copia de seguridad: {ex.Message}";
        }
        finally
        {
            EstaGenerando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeRestaurar))]
    private void AvisarRestauracionManual()
    {
        if (BackupSeleccionado is null)
            return;

        MessageBox.Show(
            "Llamar al 3794-184591 para que el administrador de base de datos realice la tarea" +
            $"\n\nArchivo: {BackupSeleccionado.NombreArchivo}" +
            $"\nUbicación: {BackupSeleccionado.RutaCompleta}",
            "Restauración de copia de seguridad",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private async Task CargarBackupsAsync()
    {
        try
        {
            var backups = await _listarBackups.Ejecutar(RutaDestinoLocal);

            HistorialBackups.Clear();
            foreach (var backup in backups)
                HistorialBackups.Add(backup);

            HayBackups = HistorialBackups.Count > 0;

            if (BackupSeleccionado is not null &&
                !HistorialBackups.Any(b => string.Equals(b.RutaCompleta, BackupSeleccionado.RutaCompleta, StringComparison.OrdinalIgnoreCase)))
            {
                BackupSeleccionado = null;
            }
        }
        catch (Exception ex)
        {
            HayBackups = false;
            MensajeEsError = true;
            MensajeEstado = $"No se pudieron leer las copias de seguridad: {ex.Message}";
        }
    }

    private bool PuedeGenerarBackup()
        => !EstaGenerando && !string.IsNullOrWhiteSpace(RutaDestinoLocal);

    private bool PuedeRestaurar()
        => !EstaGenerando && BackupSeleccionado is not null;

    partial void OnBackupSeleccionadoChanged(BackupDTO? value)
        => AvisarRestauracionManualCommand.NotifyCanExecuteChanged();

    partial void OnEstaGenerandoChanged(bool value)
    {
        GenerarBackupAhoraCommand.NotifyCanExecuteChanged();
        AvisarRestauracionManualCommand.NotifyCanExecuteChanged();
    }

    partial void OnRutaDestinoLocalChanged(string value)
        => GenerarBackupAhoraCommand.NotifyCanExecuteChanged();
}
