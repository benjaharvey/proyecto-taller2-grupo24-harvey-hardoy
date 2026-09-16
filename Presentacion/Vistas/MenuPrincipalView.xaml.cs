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
        }

        private void AbrirMainWindowEnSeccion(string seccion)
        {
            var mainWindow = App.Services.GetRequiredService<MainWindow>();
            ((MainViewModel)mainWindow.DataContext).CurrentSection = seccion;
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
