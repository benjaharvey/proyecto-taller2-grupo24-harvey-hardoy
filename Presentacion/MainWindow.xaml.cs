using System;
using System.Windows;
using Aplicacion.CasosDeUso;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;
using Wpf.Ui.Controls;

namespace Presentacion;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private bool _cerrandoSesionVoluntariamente = false;

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

    private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        var confirmacion = System.Windows.MessageBox.Show(
            this,
            "¿Estás seguro de que deseás cerrar la sesión actual?",
            "Cerrar Sesión",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmacion != System.Windows.MessageBoxResult.Yes) return;

        // RF-17: al cerrar sesión se descarta la venta pendiente (no se conserva).
        try
        {
            App.Services.GetRequiredService<DescartarVentaPendiente>()
                .Ejecutar(SesionActual.UsuarioLogueado!.Id);
        }
        catch
        {
            // Un fallo al descartar no puede impedir cerrar sesión.
        }

        _cerrandoSesionVoluntariamente = true;
        SesionActual.UsuarioLogueado = null;

        var login = App.Services.GetRequiredService<VistaLogin>();
        login.Show();
        this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_cerrandoSesionVoluntariamente)
        {
            Application.Current.Shutdown();
        }
    }

    private void AplicarPermisosPorRol()
    {
        string? rol = SesionActual.UsuarioLogueado?.NombreRol;

        if (rol == "Admin")
        {
            RbVentas.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
            RbClientes.Visibility = Visibility.Collapsed;
            RbHistorial.Visibility = Visibility.Visible;
            RbBackup.Visibility = Visibility.Visible;
            RbSucursales.Visibility = Visibility.Visible;
        }
        else if (rol == "Cocinero")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            RbVentas.Visibility = Visibility.Collapsed;
            RbHistorial.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbUsuario.Visibility = Visibility.Collapsed;
            RbSucursales.Visibility = Visibility.Collapsed;
            RbClientes.Visibility = Visibility.Collapsed;
            RbBackup.Visibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
            {
                vm.CurrentSection = "Cocina";
            }
        }
        else if (rol == "Vendedor")
        {
            RbProductos.Visibility = Visibility.Collapsed;
            // RF-12: el vendedor consulta el stock de su propia sucursal.
            RbStock.Visibility = Visibility.Visible;
            RbCocina.Visibility = Visibility.Collapsed;
            RbHistorial.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbUsuario.Visibility = Visibility.Collapsed;
            RbSucursales.Visibility = Visibility.Collapsed;
            RbClientes.Visibility = Visibility.Visible;
            RbBackup.Visibility = Visibility.Collapsed;

            if (DataContext is MainViewModel vm)
            {
                vm.CurrentSection = "Ventas";
            }
        }
        else
        {
            RbUsuario.Visibility = Visibility.Collapsed;
            RbSucursales.Visibility = Visibility.Collapsed;
            RbReportes.Visibility = Visibility.Collapsed;
            RbVentas.Visibility = Visibility.Collapsed;
            RbCocina.Visibility = Visibility.Collapsed;
            RbClientes.Visibility = Visibility.Collapsed;
            RbHistorial.Visibility = Visibility.Collapsed;
            RbBackup.Visibility = Visibility.Collapsed;
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
        RbHistorial.IsChecked = section == "Historial";
        RbClientes.IsChecked = section == "Clientes";
        RbStock.IsChecked = section == "Stock";
        RbCocina.IsChecked = section == "Cocina";
        RbReportes.IsChecked = section == "Reportes";
        RbUsuario.IsChecked = section == "Usuario";
        RbSucursales.IsChecked = section == "Sucursales";
        RbBackup.IsChecked = section == "Backup";
    }
}
