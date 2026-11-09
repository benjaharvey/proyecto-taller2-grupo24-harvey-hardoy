using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Presentacion.Controles;

public partial class BotonAyuda : UserControl
{
    // El PDF se copia junto al .exe al compilar (ver Presentacion.csproj, ítem Content)
    private const string NombreManual = "Manual_de_Usuario_Maie.pdf";

    public BotonAyuda()
    {
        InitializeComponent();
    }

    private void BtnAyuda_Click(object sender, RoutedEventArgs e)
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, NombreManual);

        if (!File.Exists(ruta))
        {
            MessageBox.Show(
                $"No se encontró el manual de usuario.\n\nSe buscó en:\n{ruta}",
                "Ayuda", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // UseShellExecute = true: Windows lo abre con el lector de PDF predeterminado
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo abrir el manual de usuario.\n\nDetalle: {ex.Message}",
                "Ayuda", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
