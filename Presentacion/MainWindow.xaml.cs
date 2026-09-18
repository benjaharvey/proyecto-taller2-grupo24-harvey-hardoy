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

        string? rol = SesionActual.UsuarioLogueado?.NombreRol;

        if (rol == "Cocinero")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            RbVentas.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbAltaUsuario.Visibility = Visibility.Collapsed;

            ((MainViewModel)DataContext).CurrentSection = "Cocina";
            RbCocina.IsChecked = true;
        }
        else if (rol == "Vendedor")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            RbStock.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbAltaUsuario.Visibility = Visibility.Collapsed;

            ((MainViewModel)DataContext).CurrentSection = "Ventas";
            RbVentas.IsChecked = true;
        }
        else if (rol != "Admin")
        {
            RbAltaUsuario.Visibility = Visibility.Collapsed;
        }
    }
}
