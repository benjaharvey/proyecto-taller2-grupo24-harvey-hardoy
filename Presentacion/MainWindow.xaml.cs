using System.Windows;
using Presentacion.ViewModels;
using Wpf.Ui.Controls;

namespace Presentacion;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        AplicarPermisosPorRol();
        ActualizarInfoSesion();

        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentSection))
                {
                    SincronizarRadioButtons(vm.CurrentSection);
                }
            };
            SincronizarRadioButtons(vm.CurrentSection);
        }
    }

    private void AplicarPermisosPorRol()
    {
        string? rol = SesionActual.UsuarioLogueado?.NombreRol;

        if (rol == "Admin")
        {
            RbVentas.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
        }
        else if (rol == "Cocinero")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            RbVentas.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbAltaUsuario.Visibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
            {
                vm.CurrentSection = "Cocina";
            }
        }
        else if (rol == "Vendedor")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            RbStock.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbAltaUsuario.Visibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
            {
                vm.CurrentSection = "Ventas";
            }
        }
        else
        {
            RbAltaUsuario.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbVentas.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
        }
    }

    private void ActualizarInfoSesion()
    {
        var usuario = SesionActual.UsuarioLogueado;
        if (usuario != null)
        {
            TxtInfoSesion.Text = $"{usuario.Nombre} {usuario.Apellido} ({usuario.NombreRol})";
        }
    }

    private void SincronizarRadioButtons(string section)
    {
        RbProductos.IsChecked = section == "Productos";
        RbVentas.IsChecked = section == "Ventas";
        RbStock.IsChecked = section == "Stock";
        RbCocina.IsChecked = section == "Cocina";
        RbReportes.IsChecked = section == "Reportes";
        RbAltaUsuario.IsChecked = section == "AltaUsuario";
    }
}
