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
        CurrentSection = section;
    }
}
