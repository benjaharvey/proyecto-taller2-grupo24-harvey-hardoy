using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.CasosDeUso;
using Datos.Conexion;
using Datos.Repositorios;
using Dominio.Interfaces;

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
        services.AddTransient<ListarUsuarios>();

        services.AddTransient<RolWindow>();
        services.AddTransient<UsuarioWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MenuPrincipal>();
        services.AddTransient<VistaLogin>();
        services.AddTransient<AltaUsuario>();

        Services = services.BuildServiceProvider();

        Services.GetRequiredService<MenuPrincipal>().Show();
    }
}
