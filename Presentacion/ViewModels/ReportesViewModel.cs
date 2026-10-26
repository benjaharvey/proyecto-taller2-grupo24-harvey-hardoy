using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

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
    // Filtros de la barra superior: ya gobiernan el ranking de más vendidos.
    // Cada vez que cambian se vuelve a calcular (ver los métodos On...Changed más abajo).
    [ObservableProperty]
    private string _periodoSeleccionado = "Últimos 30 días";

    [ObservableProperty]
    private string _sucursalFiltro = TodasLasSucursales;

    private const string TodasLasSucursales = "Todas las Sucursales";

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
        "Hoy", "Últimos 7 días", "Últimos 30 días", "Últimos 90 días"
    };

    // Se llena en el constructor con las sucursales reales de la base.
    public ObservableCollection<string> Sucursales { get; } = new();

    // ---- Ranking de productos más vendidos (datos reales) ----
    private const int CantidadEnRanking = 10;
    private readonly ObtenerProductosMasVendidos _obtenerProductosMasVendidos;
    private readonly ObtenerVentasPorSucursal _obtenerVentasPorSucursal;
    private readonly List<SucursalDTO> _sucursalesDb = new();

    // ---- Reporte: producción por producto (datos reales) ----
    private readonly ObtenerProduccionPorProducto _obtenerProduccionPorProducto;

    [ObservableProperty]
    private DateTime? _produccionDesde = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime? _produccionHasta = DateTime.Today;

    // El primer elemento de cada lista es la opción "Todos" (Id = 0): equivale a no filtrar.
    [ObservableProperty]
    private ProductoDTO? _produccionProductoFiltro;

    [ObservableProperty]
    private SucursalDTO? _produccionSucursalFiltro;

    [ObservableProperty]
    private ISeries[] _produccionSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _produccionXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private bool _produccionSinDatos;

    public ObservableCollection<ProductoDTO> ProductosFiltro { get; } = new();
    public ObservableCollection<SucursalDTO> SucursalesFiltro { get; } = new();

    // El eje Y arranca en 0 y avanza de a 1 (son unidades enteras).
    public Axis[] ProduccionYAxes { get; } =
    {
        new Axis { MinLimit = 0, MinStep = 1, Name = "Unidades elaboradas" }
    };

    public ReportesViewModel(
        ObtenerProduccionPorProducto obtenerProduccionPorProducto,
        ObtenerProductosMasVendidos obtenerProductosMasVendidos,
        ObtenerVentasPorSucursal obtenerVentasPorSucursal,
        ListarProductos listarProductos,
        ListarSucursales listarSucursales)
    {
        _obtenerProduccionPorProducto = obtenerProduccionPorProducto;
        _obtenerProductosMasVendidos = obtenerProductosMasVendidos;
        _obtenerVentasPorSucursal = obtenerVentasPorSucursal;

        // Combo de la barra superior: "Todas" + el nombre de cada sucursal.
        _sucursalesDb = listarSucursales.Ejecutar();
        Sucursales.Add(TodasLasSucursales);
        foreach (var s in _sucursalesDb)
            Sucursales.Add(s.Nombre);

        ProductosFiltro.Add(new ProductoDTO { Id = 0, Nombre = "Todos los productos" });
        foreach (var p in listarProductos.Ejecutar())
            ProductosFiltro.Add(p);

        SucursalesFiltro.Add(new SucursalDTO { Id = 0, Nombre = "Todas las sucursales" });
        foreach (var s in listarSucursales.Ejecutar())
            SucursalesFiltro.Add(s);

        ProduccionProductoFiltro = ProductosFiltro[0];
        ProduccionSucursalFiltro = SucursalesFiltro[0];

        GenerarReporteProduccion();
        CargarRankingProductos();
        CargarVentasPorSucursal();
    }

    // Se ejecutan solos cuando el usuario cambia el período o la sucursal en la barra superior.
    partial void OnPeriodoSeleccionadoChanged(string value)
    {
        CargarRankingProductos();
        CargarVentasPorSucursal();
    }

    partial void OnSucursalFiltroChanged(string value) => CargarRankingProductos();

    // Traduce el texto del combo de período a un rango de fechas (hasta = hoy).
    private static (DateTime Desde, DateTime Hasta) ObtenerRangoDelPeriodo(string periodo)
    {
        var hoy = DateTime.Today;

        return periodo switch
        {
            "Hoy" => (hoy, hoy),
            "Últimos 7 días" => (hoy.AddDays(-6), hoy),
            "Últimos 90 días" => (hoy.AddDays(-89), hoy),
            _ => (hoy.AddDays(-29), hoy)   // "Últimos 30 días" y cualquier valor inesperado
        };
    }

    private void CargarRankingProductos()
    {
        var (desde, hasta) = ObtenerRangoDelPeriodo(PeriodoSeleccionado);

        // "Todas las Sucursales" (o un nombre que no se encuentre) = sin filtro (null).
        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        List<ProductoMasVendidoDTO> datos;
        try
        {
            datos = _obtenerProductosMasVendidos.Ejecutar(desde, hasta, sucursalId, CantidadEnRanking);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // La barra de cada producto se mide contra el más vendido (el primero = 100%).
        var maximo = datos.Count > 0 ? datos.Max(d => d.Unidades) : 0;

        TopProductos = new ObservableCollection<TopProductoReporteItem>(
            datos.Select(d => new TopProductoReporteItem
            {
                Nombre = d.NombreProducto,
                UnidadesVendidas = d.Unidades,
                Recaudacion = d.Recaudado,
                PorcentajeTotal = maximo > 0 ? d.Unidades * 100.0 / maximo : 0
            }));
    }

    private void CargarVentasPorSucursal()
    {
        var (desde, hasta) = ObtenerRangoDelPeriodo(PeriodoSeleccionado);

        List<VentaPorSucursalDTO> datos;
        try
        {
            datos = _obtenerVentasPorSucursal.Ejecutar(desde, hasta);
        }
        catch(InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var totalGeneral = datos.Sum(d => d.TotalRecaudado);

        VentasPorSucursal = new ObservableCollection<SucursalVentaReporteItem>(
            datos.Select(d => new SucursalVentaReporteItem
            {
                SucursalNombre = d.NombreSucursal,
                CantidadVentas = d.CantidadVentas,
                TotalFacturado = d.TotalRecaudado,
                PorcentajeParticipacion = totalGeneral > 0
                ? Math.Round((double)d.TotalRecaudado * 100 / (double)totalGeneral, 1)
                : 0
            })
        );

    }

    [RelayCommand]
    private void GenerarReporteProduccion()
    {
        // Un DatePicker vacío entrega null: se valida antes de consultar.
        if (ProduccionDesde == null || ProduccionHasta == null)
        {
            MessageBox.Show("Elegí la fecha desde y la fecha hasta.", "Reportes",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Id = 0 (la opción "Todos") se traduce a null = sin filtro.
        int? productoId = ProduccionProductoFiltro is { Id: > 0 } ? ProduccionProductoFiltro.Id : null;
        int? sucursalId = ProduccionSucursalFiltro is { Id: > 0 } ? ProduccionSucursalFiltro.Id : null;

        List<ProduccionPorProductoDTO> datos;
        try
        {
            datos = _obtenerProduccionPorProducto.Ejecutar(
                ProduccionDesde.Value, ProduccionHasta.Value, productoId, sucursalId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ProduccionSinDatos = datos.Count == 0;

        // Se reemplazan los arreglos completos: el [ObservableProperty] avisa a la vista del cambio.
        ProduccionSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Elaborado",
                Values = datos.Select(d => d.Total).ToArray(),
                Fill = new SolidColorPaint(SKColor.Parse("#9A4600"))
            }
        };

        // Las etiquetas del eje X van en el mismo orden que los valores.
        ProduccionXAxes = new Axis[]
        {
            new Axis { Labels = datos.Select(d => d.NombreProducto).ToArray() }
        };
    }

    [RelayCommand]
    private void ExportarReporte()
    {
        // Mock export action
    }
}
