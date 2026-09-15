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

public class InsumoGlobalItem
{
    public int InsumoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public string UnidadMedida { get; set; } = "kg";
    public decimal StockMinimo { get; set; } = 10;
    public string NivelAlerta => StockActual <= StockMinimo ? "Bajo Stock" : "Óptimo";
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

    // Productos Stock
    [ObservableProperty]
    private ObservableCollection<StockProductoItem> _stockProductos = new();

    // Insumos Globales
    [ObservableProperty]
    private ObservableCollection<InsumoGlobalItem> _insumosGlobales = new();

    // Dialogs / Modals
    [ObservableProperty]
    private bool _mostrarDialogoAjusteProducto;

    [ObservableProperty]
    private StockProductoItem? _productoAjustando;

    [ObservableProperty]
    private int _nuevoStockProducto;

    [ObservableProperty]
    private string _motivoAjusteProducto = "Recuento físico";

    [ObservableProperty]
    private bool _mostrarDialogoIngresoInsumo;

    [ObservableProperty]
    private InsumoGlobalItem? _insumoSeleccionado;

    [ObservableProperty]
    private decimal _cantidadIngresoInsumo;

    // Modal Nuevo Tipo de Insumo
    [ObservableProperty]
    private bool _mostrarDialogoNuevoInsumo;

    [ObservableProperty]
    private string _nuevoInsumoCodigo = string.Empty;

    [ObservableProperty]
    private string _nuevoInsumoNombre = string.Empty;

    [ObservableProperty]
    private string _nuevoInsumoUnidad = "kg";

    [ObservableProperty]
    private decimal _nuevoInsumoStockInicial = 0;

    [ObservableProperty]
    private decimal _nuevoInsumoStockMinimo = 10;

    public ObservableCollection<string> Sucursales { get; } = new()
    {
        "Todas (Consolidado)", "Sucursal Centro", "Sucursal Norte", "Sucursal Sur"
    };

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Todas", "Alfajores", "Conitos", "Tabletas", "Tortas"
    };

    public ObservableCollection<string> UnidadesMedida { get; } = new()
    {
        "kg", "gr", "lt", "ml", "un."
    };

    public ObservableCollection<string> MotivosAjuste { get; } = new()
    {
        "Recuento físico", "Merma por vencimiento", "Rotura o daño en exhibición", "Devolución"
    };

    public StockViewModel()
    {
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

    [RelayCommand]
    private void AbrirIngresoInsumo(InsumoGlobalItem? insumo)
    {
        InsumoSeleccionado = insumo ?? InsumosGlobales.FirstOrDefault();
        CantidadIngresoInsumo = 10;
        MostrarDialogoIngresoInsumo = true;
    }

    [RelayCommand]
    private void GuardarIngresoInsumo()
    {
        if (InsumoSeleccionado != null && CantidadIngresoInsumo > 0)
        {
            InsumoSeleccionado.StockActual += CantidadIngresoInsumo;
            var index = InsumosGlobales.IndexOf(InsumoSeleccionado);
            if (index >= 0)
            {
                InsumosGlobales[index] = InsumoSeleccionado;
            }
        }
        MostrarDialogoIngresoInsumo = false;
    }

    [RelayCommand]
    private void CerrarIngresoInsumo()
    {
        MostrarDialogoIngresoInsumo = false;
    }

    [RelayCommand]
    private void AbrirNuevoInsumo()
    {
        NuevoInsumoCodigo = $"INS-00{InsumosGlobales.Count + 1}";
        NuevoInsumoNombre = string.Empty;
        NuevoInsumoUnidad = "kg";
        NuevoInsumoStockInicial = 10;
        NuevoInsumoStockMinimo = 10;
        MostrarDialogoNuevoInsumo = true;
    }

    [RelayCommand]
    private void GuardarNuevoInsumo()
    {
        if (!string.IsNullOrWhiteSpace(NuevoInsumoNombre))
        {
            var nuevo = new InsumoGlobalItem
            {
                InsumoId = InsumosGlobales.Count + 1,
                Codigo = NuevoInsumoCodigo,
                Nombre = NuevoInsumoNombre,
                UnidadMedida = NuevoInsumoUnidad,
                StockActual = NuevoInsumoStockInicial,
                StockMinimo = NuevoInsumoStockMinimo
            };
            InsumosGlobales.Add(nuevo);
        }
        MostrarDialogoNuevoInsumo = false;
    }

    [RelayCommand]
    private void CerrarNuevoInsumo()
    {
        MostrarDialogoNuevoInsumo = false;
    }
}
