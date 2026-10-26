using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.Vistas;

public partial class VentasView : UserControl
{
    public VentasView()
    {
        InitializeComponent();

        // El VM ahora se resuelve por DI (necesita los casos de uso).
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<VentasViewModel>();
        }
    }
}
