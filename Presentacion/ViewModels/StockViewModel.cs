using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class StockProductoItem
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; } = 15;
    public string NivelAlerta => StockActual == 0 ? "Crítico (Sin Stock)" : StockActual <= StockMinimo ? "Bajo Stock" : "Óptimo";
}

public partial class StockViewModel : ObservableObject
{
    [ObservableProperty]
    private string _tabActivo = "Productos"; // "Productos" o "Insumos"

    [ObservableProperty]
    private string _filtroSucursal = "Todas (Consolidado)";

    [ObservableProperty]
    private string _filtroCategoria = "Todas";

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // Productos Stock (se conectará a casos de uso de stock de productos a futuro)
    [ObservableProperty]
    private ObservableCollection<StockProductoItem> _stockProductos = new();

    // Dialogs / Modals de Productos (RF-13)
    [ObservableProperty]
    private bool _mostrarDialogoAjusteProducto;

    [ObservableProperty]
    private StockProductoItem? _productoAjustando;

    [ObservableProperty]
    private int _nuevoStockProducto;

    [ObservableProperty]
    private string _motivoAjusteProducto = "Recuento físico";

    public ObservableCollection<string> Sucursales { get; } = new()
    {
        "Todas (Consolidado)", "Sucursal Centro", "Sucursal Norte", "Sucursal Sur"
    };

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Todas", "Alfajores", "Conitos", "Tabletas", "Tortas"
    };

    public ObservableCollection<string> MotivosAjuste { get; } = new()
    {
        "Recuento físico", "Merma por vencimiento", "Rotura o daño en exhibición", "Devolución"
    };

    public InsumoViewModel InsumosVM { get; }

    public StockViewModel(InsumoViewModel insumosVM)
    {
        InsumosVM = insumosVM;
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        InsumosVM.BusquedaTexto = value;
    }

    [RelayCommand]
    private void CambiarTab(string tab)
    {
        TabActivo = tab;
    }

    [RelayCommand]
    private void AbrirAjusteProducto(StockProductoItem? prod)
    {
        if (prod == null) return;
        ProductoAjustando = prod;
        NuevoStockProducto = prod.StockActual;
        MotivoAjusteProducto = "Recuento físico";
        MostrarDialogoAjusteProducto = true;
    }

    [RelayCommand]
    private void GuardarAjusteProducto()
    {
        if (ProductoAjustando != null && NuevoStockProducto >= 0)
        {
            ProductoAjustando.StockActual = NuevoStockProducto;
            var index = StockProductos.IndexOf(ProductoAjustando);
            if (index >= 0)
            {
                StockProductos[index] = ProductoAjustando;
            }
        }
        MostrarDialogoAjusteProducto = false;
    }

    [RelayCommand]
    private void CerrarAjusteProducto()
    {
        MostrarDialogoAjusteProducto = false;
    }
}
