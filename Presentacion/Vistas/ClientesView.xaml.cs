using System.Windows.Controls;

namespace Presentacion.Vistas;

public partial class ClientesView : UserControl
{
    public ClientesView()
    {
        InitializeComponent();

        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<ClientesViewModel>();
        }
    }
}
