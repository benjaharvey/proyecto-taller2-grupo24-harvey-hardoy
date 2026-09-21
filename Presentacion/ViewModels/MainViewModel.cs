using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentSection = "Productos";

    [RelayCommand]
    private void Navigate(string section)
    {
        if (TienePermisoParaSeccion(section))
        {
            CurrentSection = section;
        }
    }

    public static bool TienePermisoParaSeccion(string section)
    {
        string? rol = SesionActual.UsuarioLogueado?.NombreRol;

        return rol switch
        {
            "Admin" => section is not "Ventas" and not "Cocina" and not "Clientes",
            "Cocinero" => section is "Cocina" or "Stock",
            "Vendedor" => section is "Ventas" or "Clientes",
            _ => false
        };
    }
}
