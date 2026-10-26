using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.CasosDeUso;
using Aplicacion.Servicios;
using Datos.Conexion;
using Datos.Repositorios;
using Datos.Seeder;
using Dominio.Interfaces;
using Presentacion.ViewModels;
using QuestPDF.Infrastructure;

namespace Presentacion;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // WPF interpreta los bindings con cultura en-US por defecto. Forzamos la cultura
        // del sistema para que los decimales se parseen y formateen con coma (es-AR).
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        base.OnStartup(e);

        // QuestPDF exige declarar la licencia antes de generar el primer documento.
        // Community: gratuita para uso no comercial / facturación anual menor a 1M USD.
        QuestPDF.Settings.License = LicenseType.Community;

        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(AppDbContext.ConnectionString));

        services.AddScoped<IRolRepositorio, RolRepositorio>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<ISucursalRepositorio, SucursalRepositorio>();
        services.AddScoped<IInsumoRepositorio, InsumoRepositorio>();
        services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
        services.AddScoped<IStockSucursalRepositorio, StockSucursalRepositorio>();
        services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
        services.AddScoped<IProduccionRepositorio, ProduccionRepositorio>();
        services.AddScoped<IVentaRepositorio, VentaRepositorio>();

        services.AddTransient<IComprobantePdfServicio, ComprobantePdfServicio>();

        services.AddTransient<CrearRol>();
        services.AddTransient<ActualizarRol>();
        services.AddTransient<EliminarRol>();
        services.AddTransient<ListarRoles>();

        services.AddTransient<CrearUsuario>();
        services.AddTransient<ActualizarUsuario>();
        services.AddTransient<EliminarUsuario>();
        services.AddTransient<ReactivarUsuario>();
        services.AddTransient<ListarUsuarios>();

        services.AddTransient<CrearSucursal>();
        services.AddTransient<ActualizarSucursal>();
        services.AddTransient<EliminarSucursal>();
        services.AddTransient<ListarSucursales>();
        services.AddTransient<ReactivarSucursal>();

        services.AddTransient<CrearInsumo>();
        services.AddTransient<ActualizarInsumo>();
        services.AddTransient<EliminarInsumo>();
        services.AddTransient<ReactivarInsumo>();
        services.AddTransient<ListarInsumos>();

        services.AddTransient<CrearCliente>();
        services.AddTransient<ActualizarCliente>();
        services.AddTransient<EliminarCliente>();
        services.AddTransient<ReactivarCliente>();
        services.AddTransient<ListarClientes>();

        services.AddTransient<ConsultarStockPorSucursal>();
        services.AddTransient<AjustarStockProducto>();
        services.AddTransient<CrearProducto>();
        services.AddTransient<ActualizarProducto>();
        services.AddTransient<EliminarProducto>();
        services.AddTransient<ReactivarProducto>();
        services.AddTransient<ListarProductos>();
        services.AddTransient<ObtenerProducto>();

        services.AddTransient<ListarCategorias>();

        services.AddTransient<RegistrarProduccion>();
        services.AddTransient<ListarProducciones>();
        services.AddTransient<ObtenerProduccionPorProducto>();
        services.AddTransient<ObtenerProductosMasVendidos>();
        services.AddTransient<ObtenerVentasPorSucursal>();

        services.AddTransient<AbrirVenta>();
        services.AddTransient<AgregarItemVenta>();
        services.AddTransient<ModificarCantidadItem>();
        services.AddTransient<QuitarItemVenta>();
        services.AddTransient<DescartarVentaPendiente>();
        services.AddTransient<CancelarVenta>();
        services.AddTransient<ListarVentas>();
        services.AddTransient<ObtenerVenta>();
        services.AddTransient<ConfirmarVenta>();
        services.AddTransient<GenerarComprobante>();

        services.AddTransient<MainWindow>();
        services.AddTransient<VistaLogin>();
        services.AddTransient<IniciarSesion>();
        services.AddTransient<UsuarioViewModel>();
        services.AddTransient<SucursalViewModel>();
        services.AddTransient<InsumoViewModel>();
        services.AddTransient<StockViewModel>();
        services.AddTransient<ProductosViewModel>();
        services.AddTransient<ClientesViewModel>();
        services.AddTransient<CocinaViewModel>();
        services.AddTransient<ReportesViewModel>();
        services.AddTransient<VentasViewModel>();
        services.AddTransient<HistorialVentasViewModel>();
        Services = services.BuildServiceProvider();

        try
        {
            using var dbContext = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            DatabaseSeeder.Inicializar(dbContext);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo preparar la base de datos.\n\n" +
                "Verificá que el servicio SQL Server (SQLEXPRESS) esté iniciado.\n\n" +
                $"Detalle: {ex.Message}",
                "Error al iniciar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        Services.GetRequiredService<VistaLogin>().Show();
    }
}
