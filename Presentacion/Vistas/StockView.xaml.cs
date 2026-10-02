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
        }
    }
}
