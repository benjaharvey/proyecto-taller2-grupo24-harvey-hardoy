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
    private decimal _totalIngresosVentas = 0;

    [ObservableProperty]
    private int _totalOperacionesVenta = 0;

    [ObservableProperty]
    private int _totalUnidadesVendidas = 0;

    [ObservableProperty]
    private int _totalUnidadesElaboradas = 0;

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
    }

    [RelayCommand]
    private void ExportarReporte()
    {
        // Mock export action
    }
}
