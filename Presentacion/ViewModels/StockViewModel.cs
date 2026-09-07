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

    [ObservableProperty]
    private string _comprobanteInsumo = string.Empty;

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

    public StockViewModel()
    {
        CargarDatosMock();
    }

    private void CargarDatosMock()
    {
        StockProductos = new ObservableCollection<StockProductoItem>
        {
            new() { ProductoId = 1, Codigo = "ALF-001", Nombre = "Alfajor Clásico DDL", Categoria = "Alfajores", Sucursal = "Sucursal Centro", StockActual = 45, StockMinimo = 20 },
            new() { ProductoId = 1, Codigo = "ALF-001", Nombre = "Alfajor Clásico DDL", Categoria = "Alfajores", Sucursal = "Sucursal Norte", StockActual = 12, StockMinimo = 20 },
            new() { ProductoId = 2, Codigo = "ALF-002", Nombre = "Alfajor Nuez y Choc Blanco", Categoria = "Alfajores", Sucursal = "Sucursal Centro", StockActual = 22, StockMinimo = 15 },
            new() { ProductoId = 2, Codigo = "ALF-002", Nombre = "Alfajor Nuez y Choc Blanco", Categoria = "Alfajores", Sucursal = "Sucursal Sur", StockActual = 5, StockMinimo = 15 },
            new() { ProductoId = 3, Codigo = "CON-001", Nombre = "Conito Dulce de Leche", Categoria = "Conitos", Sucursal = "Sucursal Centro", StockActual = 18, StockMinimo = 15 },
            new() { ProductoId = 4, Codigo = "TAB-001", Nombre = "Tableta Marroc Artesanal", Categoria = "Tabletas", Sucursal = "Sucursal Norte", StockActual = 0, StockMinimo = 10 },
            new() { ProductoId = 5, Codigo = "TOR-001", Nombre = "Mini Torta Rogel", Categoria = "Tortas", Sucursal = "Sucursal Centro", StockActual = 8, StockMinimo = 10 },
            new() { ProductoId = 6, Codigo = "ALF-003", Nombre = "Alfajor Maicena Tradicional", Categoria = "Alfajores", Sucursal = "Sucursal Sur", StockActual = 35, StockMinimo = 15 }
        };

        InsumosGlobales = new ObservableCollection<InsumoGlobalItem>
        {
            new() { InsumoId = 1, Codigo = "INS-001", Nombre = "Dulce de Leche Repostero Especial", StockActual = 185.5m, UnidadMedida = "kg", StockMinimo = 50m },
            new() { InsumoId = 2, Codigo = "INS-002", Nombre = "Harina de Trigo 0000", StockActual = 240.0m, UnidadMedida = "kg", StockMinimo = 80m },
            new() { InsumoId = 3, Codigo = "INS-003", Nombre = "Chocolate Cobertura Semiamargo 70%", StockActual = 64.0m, UnidadMedida = "kg", StockMinimo = 25m },
            new() { InsumoId = 4, Codigo = "INS-004", Nombre = "Chocolate Cobertura Blanco", StockActual = 32.5m, UnidadMedida = "kg", StockMinimo = 20m },
            new() { InsumoId = 5, Codigo = "INS-005", Nombre = "Nueces Mariposa Peladas", StockActual = 8.5m, UnidadMedida = "kg", StockMinimo = 15m },
            new() { InsumoId = 6, Codigo = "INS-006", Nombre = "Manteca de Primera Calidad", StockActual = 45.0m, UnidadMedida = "kg", StockMinimo = 20m },
            new() { InsumoId = 7, Codigo = "INS-007", Nombre = "Pasta de Maní Tostado", StockActual = 19.0m, UnidadMedida = "kg", StockMinimo = 15m },
            new() { InsumoId = 8, Codigo = "INS-008", Nombre = "Coco Rallado Fino", StockActual = 14.0m, UnidadMedida = "kg", StockMinimo = 10m }
        };
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
        ComprobanteInsumo = string.Empty;
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
}
