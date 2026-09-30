using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.Vistas;

public partial class UsuarioView : UserControl
{
    private UsuarioViewModel? ViewModel => DataContext as UsuarioViewModel;

    public UsuarioView()
    {
        InitializeComponent();

        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<UsuarioViewModel>();
        }
    }

    private static bool EsSoloDigitos(string? texto) => !string.IsNullOrEmpty(texto) && texto.All(char.IsAsciiDigit);

    private void SoloDigitos_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !EsSoloDigitos(e.Text);
    }

    private void SoloDigitos_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string)) || !EsSoloDigitos((string)e.DataObject.GetData(typeof(string))))
            e.CancelCommand();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.FormPassword = txtPassword.Password;
        }
    }

    private void TxtConfirmarPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.FormConfirmarPassword = txtConfirmarPassword.Password;
        }
    }
}
