using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.Vistas;

public partial class StockView : UserControl
{
    public StockView()
    {
        InitializeComponent();

        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<StockViewModel>();
            DataContextChanged += (_, _) => AplicarPermisosPorRol();
            AplicarPermisosPorRol();
        }
    }

    /// <summary>
    /// RF-13: el ajuste manual de stock es exclusivo del administrador,
    /// se oculta la columna de acciones para el vendedor.
    /// </summary>
    private void AplicarPermisosPorRol()
    {
        if (DataContext is StockViewModel vm)
        {
            ColumnaAjustes.Visibility = vm.EsAdmin
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }
    }
}
