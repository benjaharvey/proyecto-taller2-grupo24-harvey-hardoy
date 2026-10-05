using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.CasosDeUso;
using Datos.Conexion;
using Datos.Repositorios;
using Datos.Seeder;
using Dominio.Interfaces;
using Presentacion.ViewModels;

namespace Presentacion;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(AppDbContext.ConnectionString));

        services.AddScoped<IRolRepositorio, RolRepositorio>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddTransient<CrearRol>();
        services.AddTransient<ActualizarRol>();
        services.AddTransient<EliminarRol>();
        services.AddTransient<ListarRoles>();

        services.AddTransient<CrearUsuario>();
        services.AddTransient<ActualizarUsuario>();
        services.AddTransient<EliminarUsuario>();
        services.AddTransient<ReactivarUsuario>();
        services.AddTransient<ListarUsuarios>();

        services.AddTransient<CrearProducto>();
        services.AddTransient<ActualizarProducto>();
        services.AddTransient<EliminarProducto>();
        services.AddTransient<ListarProductos>();
        services.AddTransient<ReactivarProducto>();

        services.AddTransient<CrearCategoria>();
        services.AddTransient<ActualizarCategoria>();
        services.AddTransient<EliminarCategoria>();
        services.AddTransient<ListarCategorias>();
        services.AddTransient<ReactivarCategoria>();

        services.AddTransient<MainWindow>();
        services.AddTransient<VistaLogin>();
        services.AddTransient<IniciarSesion>();
        services.AddTransient<UsuarioViewModel>();


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
