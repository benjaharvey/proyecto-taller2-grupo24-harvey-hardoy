using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class TopProductoReporteItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal Recaudacion { get; set; }
    public double PorcentajeTotal { get; set; } // 0-100 para barra de progreso
}

public class SucursalVentaReporteItem
{
    public string SucursalNombre { get; set; } = string.Empty;
    public int CantidadVentas { get; set; }
    public decimal TotalFacturado { get; set; }
    public double PorcentajeParticipacion { get; set; }
}

public class DetalleConsolidadoReporteItem
{
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int UnidadesElaboradas { get; set; }
    public int UnidadesVendidas { get; set; }
    public int StockDisponible { get; set; }
    public decimal TotalRecaudado { get; set; }
}

public partial class ReportesViewModel : ObservableObject
{
    [ObservableProperty]
    private string _periodoSeleccionado = "Mes Actual (Septiembre 2026)";

    [ObservableProperty]
    private string _sucursalFiltro = "Todas las Sucursales";

    // KPI Cards
    [ObservableProperty]
    private decimal _totalIngresosVentas = 842500.00m;

    [ObservableProperty]
    private int _totalOperacionesVenta = 384;

    [ObservableProperty]
    private int _totalUnidadesVendidas = 1420;

    [ObservableProperty]
    private int _totalUnidadesElaboradas = 1850;

    // Visual lists
    [ObservableProperty]
    private ObservableCollection<TopProductoReporteItem> _topProductos = new();

    [ObservableProperty]
    private ObservableCollection<SucursalVentaReporteItem> _ventasPorSucursal = new();

    [ObservableProperty]
    private ObservableCollection<DetalleConsolidadoReporteItem> _tablaConsolidada = new();

    public ObservableCollection<string> Periodos { get; } = new()
    {
        "Hoy", "Últimos 7 días", "Mes Actual (Septiembre 2026)", "Últimos 30 días", "Año 2026"
    };

    public ObservableCollection<string> Sucursales { get; } = new()
    {
        "Todas las Sucursales", "Sucursal Centro", "Sucursal Norte", "Sucursal Sur"
    };

    public ReportesViewModel()
    {
        CargarDatosMock();
    }

    private void CargarDatosMock()
    {
        TopProductos = new ObservableCollection<TopProductoReporteItem>
        {
            new() { Nombre = "Alfajor Clásico Dulce de Leche", Categoria = "Alfajores", UnidadesVendidas = 680, Recaudacion = 1224000.00m, PorcentajeTotal = 48 },
            new() { Nombre = "Conito Dulce de Leche", Categoria = "Conitos", UnidadesVendidas = 340, Recaudacion = 544000.00m, PorcentajeTotal = 24 },
            new() { Nombre = "Tableta Marroc Artesanal", Categoria = "Tabletas", UnidadesVendidas = 210, Recaudacion = 504000.00m, PorcentajeTotal = 15 },
            new() { Nombre = "Alfajor Nuez y Chocolate Blanco", Categoria = "Alfajores", UnidadesVendidas = 190, Recaudacion = 399000.00m, PorcentajeTotal = 13 }
        };

        VentasPorSucursal = new ObservableCollection<SucursalVentaReporteItem>
        {
            new() { SucursalNombre = "Sucursal Centro", CantidadVentas = 195, TotalFacturado = 412800.00m, PorcentajeParticipacion = 49 },
            new() { SucursalNombre = "Sucursal Norte", CantidadVentas = 114, TotalFacturado = 261100.00m, PorcentajeParticipacion = 31 },
            new() { SucursalNombre = "Sucursal Sur", CantidadVentas = 75, TotalFacturado = 168600.00m, PorcentajeParticipacion = 20 }
        };

        TablaConsolidada = new ObservableCollection<DetalleConsolidadoReporteItem>
        {
            new() { Producto = "Alfajor Clásico Dulce de Leche", Categoria = "Alfajores", UnidadesElaboradas = 850, UnidadesVendidas = 680, StockDisponible = 170, TotalRecaudado = 1224000.00m },
            new() { Producto = "Conito Dulce de Leche", Categoria = "Conitos", UnidadesElaboradas = 420, UnidadesVendidas = 340, StockDisponible = 80, TotalRecaudado = 544000.00m },
            new() { Producto = "Tableta Marroc Artesanal", Categoria = "Tabletas", UnidadesElaboradas = 250, UnidadesVendidas = 210, StockDisponible = 40, TotalRecaudado = 504000.00m },
            new() { Producto = "Alfajor Nuez y Chocolate Blanco", Categoria = "Alfajores", UnidadesElaboradas = 230, UnidadesVendidas = 190, StockDisponible = 40, TotalRecaudado = 399000.00m },
            new() { Producto = "Mini Torta Rogel", Categoria = "Tortas", UnidadesElaboradas = 100, UnidadesVendidas = 85, StockDisponible = 15, TotalRecaudado = 297500.00m }
        };
    }

    [RelayCommand]
    private void ExportarReporte()
    {
        // Mock export action
    }
}
