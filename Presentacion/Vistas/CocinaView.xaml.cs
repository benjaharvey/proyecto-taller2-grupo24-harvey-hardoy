using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.Vistas;

public partial class CocinaView : UserControl
{
    public CocinaView()
    {
        InitializeComponent();

        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<CocinaViewModel>();
        }
    }
}
