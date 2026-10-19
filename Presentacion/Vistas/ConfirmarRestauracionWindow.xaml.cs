using System;
using System.Windows;
using System.Windows.Controls;
using Aplicacion.DTOs;

namespace Presentacion.Vistas;

public partial class ConfirmarRestauracionWindow : Window
{
    private const string PalabraConfirmacion = "RESTAURAR";

    public ConfirmarRestauracionWindow(BackupDTO backup)
    {
        InitializeComponent();

        ArchivoValor.Text = backup.NombreArchivo;
        FechaValor.Text = backup.FechaBackup == DateTime.MinValue
            ? "Desconocida"
            : backup.FechaBackup.ToString("dd/MM/yyyy HH:mm");
        TamanoValor.Text = string.IsNullOrWhiteSpace(backup.TamanoLegible) ? "Desconocido" : backup.TamanoLegible;
        CreadoPorValor.Text = string.IsNullOrWhiteSpace(backup.CreadoPor) ? "Desconocido" : backup.CreadoPor;
        RutaValor.Text = backup.RutaCompleta;
    }

    private void Continuar_Click(object sender, RoutedEventArgs e)
    {
        PasoAdvertenciaPanel.Visibility = Visibility.Collapsed;
        PasoConfirmacionPanel.Visibility = Visibility.Visible;
        ConfirmacionTextBox.Focus();
    }

    private void Volver_Click(object sender, RoutedEventArgs e)
    {
        PasoConfirmacionPanel.Visibility = Visibility.Collapsed;
        PasoAdvertenciaPanel.Visibility = Visibility.Visible;
        ConfirmacionTextBox.Clear();
    }

    private void ConfirmacionTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RestaurarButton.IsEnabled = string.Equals(ConfirmacionTextBox.Text, PalabraConfirmacion, StringComparison.Ordinal);
    }

    private void Restaurar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
