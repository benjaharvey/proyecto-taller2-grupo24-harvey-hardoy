using Aplicacion.DTOs;

namespace Presentacion;

public static class SesionActual
{
    public static UsuarioDTO? UsuarioLogueado { get; set; }

    public static bool EsAdmin => UsuarioLogueado?.NombreRol == "Admin";

    public static bool EsVendedor => UsuarioLogueado?.NombreRol == "Vendedor";

    public static bool EsCocinero => UsuarioLogueado?.NombreRol == "Cocinero";
}
