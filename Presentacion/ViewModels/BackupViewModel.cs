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
using Presentacion.Vistas;

namespace Presentacion.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly GenerarBackup _generarBackup;
    private readonly ListarBackups _listarBackups;
    private readonly RestaurarBackup _restaurarBackup;

    [ObservableProperty]
    private ObservableCollection<BackupDTO> _historialBackups = new();

    [ObservableProperty]
    private BackupDTO? _backupSeleccionado;

    [ObservableProperty]
    private string _rutaDestinoLocal;

    [ObservableProperty]
    private bool _estaGenerando;

    [ObservableProperty]
    private bool _estaRestaurando;

    [ObservableProperty]
    private bool _hayBackups;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    private bool _mensajeEsError;

    public bool OneDriveDisponible { get; }

    public string EstadoOneDrive { get; }

    public BackupViewModel(GenerarBackup generarBackup, ListarBackups listarBackups, RestaurarBackup restaurarBackup)
    {
        _generarBackup = generarBackup;
        _listarBackups = listarBackups;
        _restaurarBackup = restaurarBackup;

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
    private async Task RestaurarBackupSeleccionadoAsync()
    {
        if (BackupSeleccionado is null)
            return;

        var confirmacion = new ConfirmarRestauracionWindow(BackupSeleccionado);

        if (Application.Current?.MainWindow is { } ventanaPrincipal)
            confirmacion.Owner = ventanaPrincipal;

        if (confirmacion.ShowDialog() != true)
            return;

        try
        {
            EstaRestaurando = true;
            MensajeEsError = false;
            MensajeEstado = "Restaurando la base de datos...";

            await _restaurarBackup.Ejecutar(BackupSeleccionado.RutaCompleta);

            MensajeEstado = "Restauración completada. Reiniciá la aplicación para reconectar con la base restaurada.";

            MessageBox.Show(
                "La base de datos se restauró correctamente.\n\n" +
                "Reiniciá la aplicación para reconectar con la base restaurada.",
                "Restauración completada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await CargarBackupsAsync();
        }
        catch (Exception ex)
        {
            MensajeEsError = true;
            MensajeEstado = $"No se pudo restaurar la base de datos: {ex.Message}";

            MessageBox.Show(
                "No se pudo restaurar la base de datos.\n\n" + ex.Message,
                "Error al restaurar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            EstaRestaurando = false;
        }
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
        => !EstaGenerando && !EstaRestaurando && !string.IsNullOrWhiteSpace(RutaDestinoLocal);

    private bool PuedeRestaurar()
        => !EstaGenerando && !EstaRestaurando && BackupSeleccionado is not null;

    partial void OnBackupSeleccionadoChanged(BackupDTO? value)
        => RestaurarBackupSeleccionadoCommand.NotifyCanExecuteChanged();

    partial void OnEstaGenerandoChanged(bool value)
    {
        GenerarBackupAhoraCommand.NotifyCanExecuteChanged();
        RestaurarBackupSeleccionadoCommand.NotifyCanExecuteChanged();
    }

    partial void OnEstaRestaurandoChanged(bool value)
    {
        GenerarBackupAhoraCommand.NotifyCanExecuteChanged();
        RestaurarBackupSeleccionadoCommand.NotifyCanExecuteChanged();
    }

    partial void OnRutaDestinoLocalChanged(string value)
        => GenerarBackupAhoraCommand.NotifyCanExecuteChanged();
}
