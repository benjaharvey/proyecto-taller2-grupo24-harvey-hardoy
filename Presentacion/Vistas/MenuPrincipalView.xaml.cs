using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion
{
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
            AplicarPermisosPorRol();
        }

        private void AplicarPermisosPorRol()
        {
            string? rol = SesionActual.UsuarioLogueado?.NombreRol;

            if (rol == "Admin")
            {
                BtnPuntoDeVenta.Visibility = Visibility.Collapsed;
                BtnCocina.Visibility = Visibility.Collapsed;
            }
            else if (rol == "Cocinero")
            {
                BtnRegistrarUsuario.Visibility = Visibility.Collapsed;
                BtnProductos.Visibility = Visibility.Collapsed;
                BtnPuntoDeVenta.Visibility = Visibility.Collapsed;
                BtnReportes.Visibility = Visibility.Collapsed;
                BtnBackup.Visibility = Visibility.Collapsed;
                BtnClientes.Visibility = Visibility.Collapsed;
            }
            else if (rol == "Vendedor")
            {
                BtnRegistrarUsuario.Visibility = Visibility.Collapsed;
                BtnProductos.Visibility = Visibility.Collapsed;
                BtnStockInsumos.Visibility = Visibility.Collapsed;
                BtnCocina.Visibility = Visibility.Collapsed;
                BtnReportes.Visibility = Visibility.Collapsed;
                BtnBackup.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnRegistrarUsuario.Visibility = Visibility.Collapsed;
                BtnBackup.Visibility = Visibility.Collapsed;
                BtnReportes.Visibility = Visibility.Collapsed;
                BtnPuntoDeVenta.Visibility = Visibility.Collapsed;
                BtnCocina.Visibility = Visibility.Collapsed;
            }
        }

        private void AbrirMainWindowEnSeccion(string seccion)
        {
            if (!MainViewModel.TienePermisoParaSeccion(seccion))
            {
                MessageBox.Show("No tenés permisos para acceder a esta sección.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mainWindow = App.Services.GetRequiredService<MainWindow>();
            if (mainWindow.DataContext is MainViewModel vm)
            {
                vm.CurrentSection = seccion;
            }
            mainWindow.Show();
            this.Close();
        }

        private void BtnRegistrarUsuario_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("AltaUsuario");

        private void BtnProductos_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("Productos");

        private void BtnPuntoDeVenta_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("Ventas");

        private void BtnStockInsumos_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("Stock");

        private void BtnCocina_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("Cocina");

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
            => AbrirMainWindowEnSeccion("Reportes");
    }
}
