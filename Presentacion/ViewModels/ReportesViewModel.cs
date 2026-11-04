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
using Dominio.Enums;

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

public partial class ReportesViewModel : ObservableObject, ISeccionRecargable
{
    // Filtros de la barra superior: ya gobiernan el ranking de más vendidos.
    // Cada vez que cambian se vuelve a calcular (ver los métodos On...Changed más abajo).
    [ObservableProperty]
    private DateTime? _fechaDesde = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime? _fechaHasta = DateTime.Today;

    [ObservableProperty]
    private string _sucursalFiltro = TodasLasSucursales;

    private const string TodasLasSucursales = "Todas las Sucursales";

    // KPI Cards
    // Ganancia = lo cobrado menos lo que costó producir lo vendido. TotalIngresosVentas es lo cobrado (facturado).
    [ObservableProperty]
    private decimal _totalGanancia = 0;

    [ObservableProperty]
    private decimal _totalIngresosVentas = 0;

    [ObservableProperty]
    private int _totalOperacionesVenta = 0;

    [ObservableProperty]
    private int _totalUnidadesVendidas = 0;

    [ObservableProperty]
    private decimal _ticketPromedio = 0;

    // Visual lists
    [ObservableProperty]
    private ObservableCollection<TopProductoReporteItem> _topProductos = new();

    [ObservableProperty]
    private ObservableCollection<SucursalVentaReporteItem> _ventasPorSucursal = new();

    // ---- Reporte: recaudación por método de pago
    [ObservableProperty]
    private ISeries[] _metodoPagoSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ObservableCollection<RecaudacionPorMetodoPagoDTO> _metodosPago = new();

    [ObservableProperty]
    private decimal _totalRecaudadoMetodoPago = 0;

    // Reparto efectivo vs. digital del período (0-100).
    [ObservableProperty]
    private double _participacionEfectivo = 0;

    [ObservableProperty]
    private double _participacionDigital = 0;

    [ObservableProperty]
    private bool _metodoPagoSinDatos;

    // ---- Reporte: recaudación por categoría
    [ObservableProperty]
    private ISeries[] _categoriaSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _categoriaXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private ObservableCollection<RecaudacionPorCategoriaDTO> _categorias = new();

    [ObservableProperty]
    private decimal _totalRecaudadoCategorias = 0;

    [ObservableProperty]
    private string _categoriaLider = "—";

    [ObservableProperty]
    private double _participacionCategoriaLider = 0;

    [ObservableProperty]
    private bool _categoriaSinDatos;

    public Axis[] CategoriaYAxes { get; } =
    {
        new Axis { MinLimit = 0, Name = "Recaudado" }
    };

    // Reporte: productos con stock sin ventas
    [ObservableProperty]
    private ObservableCollection<StockSinVentaDTO> _stockSinVenta = new();

    [ObservableProperty]
    private CategoriaDTO? _categoriaStockFiltro;

    [ObservableProperty]
    private bool _stockSinVentaSinDatos;

    [ObservableProperty]
    private int _productosSinVenta = 0;

    [ObservableProperty]
    private decimal _valorizadoSinVenta = 0;

    public ObservableCollection<CategoriaDTO> CategoriasFiltro { get; } = new();

    // Casa Central y Fábrica son administrativa/producción, no puntos de venta.
    // Cuando el modelo tenga un TipoSucursal, esta lista se reemplaza.
    private static readonly string[] SucursalesSinVenta = { "Casa Central", "Fábrica" };

    // Se llena en el constructor con las sucursales reales de la base.
    public ObservableCollection<string> Sucursales { get; } = new();


    // ---- Ranking de productos más vendidos (datos reales) ----

    public string TituloRanking => MostrarMenosVendidos
        ? "📉 Ranking de Productos Menos Vendidos"
        : "🏆 Ranking de Productos Más Vendidos";

    // El botón ofrece pasar al otro modo.
    public string TextoBotonRanking => MostrarMenosVendidos
        ? "Ver más vendidos"
        : "Ver menos vendidos";

    private const int CantidadEnRanking = 10;
    private readonly ObtenerProductosMasVendidos _obtenerProductosMasVendidos;
    private readonly ObtenerProductosMenosVendidos _obtenerProductosMenosVendidos;
    private readonly ObtenerVentasPorSucursal _obtenerVentasPorSucursal;
    private readonly ObtenerResumenVentas _obtenerResumenVentas;
    private readonly ObtenerRecaudacionPorMetodoPago _obtenerRecaudacionPorMetodoPago;
    private readonly ObtenerRecaudacionPorCategoria _obtenerRecaudacionPorCategoria;
    private readonly ObtenerStockSinVentas _obtenerStockSinVentas;

    private readonly ObtenerVentasPorVendedor _obtenerVentasPorVendedor;
    private readonly List<SucursalDTO> _sucursalesDb = new();


    // Reporte: producción por producto (datos reales)
    private readonly ObtenerProduccionPorProducto _obtenerProduccionPorProducto;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TituloRanking))]
    [NotifyPropertyChangedFor(nameof(TextoBotonRanking))]
    private bool _mostrarMenosVendidos = false;

    [ObservableProperty]
    private ISeries[] _vendedorSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private bool _vendedoresSinDatos;

    [ObservableProperty]
    private ISeries[] _produccionSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _produccionXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private bool _produccionSinDatos;

    [ObservableProperty]
    private CategoriaDTO? _produccionCategoriaFiltro;

    // El eje Y arranca en 0 y avanza de a 1 (son unidades enteras).
    public Axis[] ProduccionYAxes { get; } =
    {
        new Axis { MinLimit = 0, MinStep = 1, Name = "Unidades elaboradas" }
    };

    public ReportesViewModel(
        ObtenerProduccionPorProducto obtenerProduccionPorProducto,
        ObtenerProductosMasVendidos obtenerProductosMasVendidos,
        ObtenerProductosMenosVendidos obtenerProductosMenosVendidos,
        ObtenerVentasPorSucursal obtenerVentasPorSucursal,
        ObtenerVentasPorVendedor obtenerVentasPorVendedor,
        ObtenerResumenVentas obtenerResumenVentas,
        ObtenerRecaudacionPorMetodoPago obtenerRecaudacionPorMetodoPago,
        ObtenerRecaudacionPorCategoria obtenerRecaudacionPorCategoria,
        ObtenerStockSinVentas obtenerStockSinVentas,
        ListarSucursales listarSucursales,
        ListarCategorias listarCategorias)
    {
        _obtenerProduccionPorProducto = obtenerProduccionPorProducto;
        _obtenerProductosMasVendidos = obtenerProductosMasVendidos;
        _obtenerProductosMenosVendidos = obtenerProductosMenosVendidos;
        _obtenerVentasPorSucursal = obtenerVentasPorSucursal;
        _obtenerResumenVentas = obtenerResumenVentas;
        _obtenerRecaudacionPorMetodoPago = obtenerRecaudacionPorMetodoPago;
        _obtenerRecaudacionPorCategoria = obtenerRecaudacionPorCategoria;
        _obtenerStockSinVentas = obtenerStockSinVentas;
        _obtenerVentasPorVendedor = obtenerVentasPorVendedor;


        // Combo de la barra superior: "Todas" + el nombre de cada punto de venta.
        _sucursalesDb = listarSucursales.Ejecutar();
        Sucursales.Add(TodasLasSucursales);
        foreach (var s in _sucursalesDb.Where(s => !SucursalesSinVenta.Contains(s.Nombre)))
            Sucursales.Add(s.Nombre);

        CategoriasFiltro.Add(new CategoriaDTO { Id = 0, Nombre = "Todas las categorías" });
        foreach (var c in listarCategorias.Ejecutar())
            CategoriasFiltro.Add(c);

        // Asignar la categoría dispara la carga inicial del reporte de producción
        // (ver OnProduccionCategoriaFiltroChanged).
        ProduccionCategoriaFiltro = CategoriasFiltro[0];

        CargarRankingProductos();
        CargarVentasPorSucursal();
        CargarResumenVentas();
        CargarRecaudacionPorMetodoPago();
        CargarRecaudacionPorCategoria();

        // El setter de la categoría dispara la carga inicial del reporte de stock
        // (ver OnCategoriaStockFiltroChanged): va último para que todo esté listo.
        CategoriaStockFiltro = CategoriasFiltro[0];
        CargarVentasPorVendedor();
    }

    public void Recargar()
    {
        // Sin rango completo no hay nada que consultar y no conviene molestar al entrar a la sección.
        if (FechaDesde == null || FechaHasta == null) return;

        RefrescarReportes();
    }

    private bool TryObtenerRango(out DateTime desde, out DateTime hasta)
    {
        if (FechaDesde == null || FechaHasta == null)
        {
            desde = default;
            hasta = default;
            MessageBox.Show("Elegí la fecha desde y la fecha hasta.", "Reportes",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            return false;

        } else
        {
            desde = FechaDesde.Value;
            hasta = FechaHasta.Value;

            if (desde.Date > hasta.Date)
            {
                MessageBox.Show("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.", "Reportes",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;
            }

            return true;
        }
    }
    // El ranking y las tarjetas dependen de la sucursal elegida (ventas por sucursal compara todas).
    partial void OnSucursalFiltroChanged(string value)
    {
        RefrescarReportes(incluirVentasPorSucursal: false);
    }

    // Las fechas se aplican solas al elegirlas: no hace falta un botón de "Aplicar".
    partial void OnFechaDesdeChanged(DateTime? value)
    {
        AplicarFiltrosDeFecha();
    }

    partial void OnFechaHastaChanged(DateTime? value)
    {
        AplicarFiltrosDeFecha();
    }

    private void AplicarFiltrosDeFecha()
    {
        // Mientras el usuario completa el rango no se consulta.
        if (FechaDesde is not DateTime desde || FechaHasta is not DateTime hasta) return;

        if (desde.Date > hasta.Date)
        {
            MessageBox.Show("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.", "Reportes",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        RefrescarReportes();
    }

    // La categoría solo afecta al reporte de stock: no hay que recargar el resto.
    partial void OnCategoriaStockFiltroChanged(CategoriaDTO? value)
    {
        CargarStockSinVentas();
    }

    // La categoría de producción solo afecta a ese reporte.
    partial void OnProduccionCategoriaFiltroChanged(CategoriaDTO? value)
    {
        CargarProduccion();
    }

    private void CargarRankingProductos()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        // "Todas las Sucursales" (o un nombre que no se encuentre) = sin filtro (null).
        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        List<ProductoMasVendidoDTO> datos;
        try
        {
            if(MostrarMenosVendidos == true)
            {
                datos = _obtenerProductosMenosVendidos.Ejecutar(desde, hasta, sucursalId, CantidadEnRanking);
            } else
            {
                datos = _obtenerProductosMasVendidos.Ejecutar(desde, hasta, sucursalId, CantidadEnRanking);
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
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
        if (!TryObtenerRango(out var desde, out var hasta)) return;

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

    // Llena las tarjetas de arriba: ganancia, facturado, ventas confirmadas y unidades vendidas.
    private void CargarResumenVentas()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        // "Todas las Sucursales" (o un nombre que no se encuentre) = sin filtro (null).
        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        ResumenVentasDTO resumen;
        try
        {
            resumen = _obtenerResumenVentas.Ejecutar(desde, hasta, sucursalId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        TotalGanancia = resumen.Ganancia;
        TotalIngresosVentas = resumen.Facturado;
        TotalOperacionesVenta = resumen.CantidadVentas;
        TotalUnidadesVendidas = resumen.Unidades;
        TicketPromedio = resumen.TicketPromedio;

    }

    private void CargarVentasPorVendedor()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        List<VentaPorVendedorDTO> datos;

        try
        {
            datos = _obtenerVentasPorVendedor.Ejecutar(desde, hasta, sucursalId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        VendedoresSinDatos = datos.Count == 0;

        VendedorSeries = datos
            .Select(d => new PieSeries<int>
            {
                Name = d.NombreVendedor,
                Values = new[] { d.CantidadVentas }
            })
            .ToArray();
    }

    private void RefrescarReportes(bool incluirVentasPorSucursal = true)
    {
        // Se valida una sola vez: si el rango no sirve, no se disparan siete mensajes iguales.
        if (!TryObtenerRango(out _, out _)) return;

        CargarProduccion();
        CargarRankingProductos();
        if (incluirVentasPorSucursal)
            CargarVentasPorSucursal();
        CargarResumenVentas();
        CargarRecaudacionPorMetodoPago();
        CargarRecaudacionPorCategoria();
        CargarStockSinVentas();
        CargarVentasPorVendedor();
    }

    // Reporte por metodo de pago
    private static readonly Dictionary<MetodoPago, SKColor> ColoresPorMetodoPago = new()
    {
        [MetodoPago.EFECTIVO] = SKColor.Parse("#622B16"),
        [MetodoPago.TARJETA] = SKColor.Parse("#9A4600"),
        [MetodoPago.TRANSFERENCIA] = SKColor.Parse("#BE8E75")
    };

    private void CargarRecaudacionPorMetodoPago()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        // "Todas las Sucursales" (o un nombre que no se encuentre) = sin filtro (null).
        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        List<RecaudacionPorMetodoPagoDTO> datos;
        try
        {
            datos = _obtenerRecaudacionPorMetodoPago.Ejecutar(desde, hasta, sucursalId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MetodosPago = new ObservableCollection<RecaudacionPorMetodoPagoDTO>(datos);

        TotalRecaudadoMetodoPago = datos.Sum(d => d.TotalRecaudado);
        MetodoPagoSinDatos = TotalRecaudadoMetodoPago <= 0;

        // Efectivo vs. digital
        var efectivo = datos.FirstOrDefault(d => d.Metodo == MetodoPago.EFECTIVO)?.TotalRecaudado ?? 0;

        ParticipacionEfectivo = TotalRecaudadoMetodoPago > 0
            ? Math.Round((double)(efectivo * 100 / TotalRecaudadoMetodoPago), 1)
            : 0;

        ParticipacionDigital = TotalRecaudadoMetodoPago > 0
            ? Math.Round(100 - ParticipacionEfectivo, 1)
            : 0;

        // Las porciones en cero no se dibujan: no aportan información y ensucian la torta.
        // En la tabla sí aparecen, para que el cero sea explícito.
        MetodoPagoSeries = datos
            .Where(d => d.TotalRecaudado > 0)
            .Select(d => (ISeries)new PieSeries<decimal>
            {
                Name = d.Nombre,
                Values = new[] { d.TotalRecaudado },
                Fill = new SolidColorPaint(ColoresPorMetodoPago[d.Metodo]),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 11,
                DataLabelsFormatter = p => $"{p.StackedValue!.Share:P0}",
                InnerRadius = 55
            })
            .ToArray();
    }

    // Reporte por categoria
    private void CargarRecaudacionPorCategoria()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;

        List<RecaudacionPorCategoriaDTO> datos;
        try
        {
            datos = _obtenerRecaudacionPorCategoria.Ejecutar(desde, hasta, sucursalId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Categorias = new ObservableCollection<RecaudacionPorCategoriaDTO>(datos);

        TotalRecaudadoCategorias = datos.Sum(d => d.TotalRecaudado);
        CategoriaSinDatos = TotalRecaudadoCategorias <= 0;

        var lider = datos.FirstOrDefault();

        CategoriaLider = lider?.NombreCategoria ?? "—";
        ParticipacionCategoriaLider = lider?.Participacion ?? 0;

        CategoriaSeries = new ISeries[]
        {
            new ColumnSeries<decimal>
            {
                Name = "Recaudado",
                Values = datos.Select(d => d.TotalRecaudado).ToArray(),
                Fill = new SolidColorPaint(SKColor.Parse("#9A4600"))
            }
        };

        CategoriaXAxes = new Axis[]
        {
            new Axis { Labels = datos.Select(d => d.NombreCategoria).ToArray() }
        };
    }

    // Reporte de stock sin ventas
    private void CargarStockSinVentas()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;
        int? categoriaId = CategoriaStockFiltro is { Id: > 0 } ? CategoriaStockFiltro.Id : null;

        List<StockSinVentaDTO> datos;
        try
        {
            datos = _obtenerStockSinVentas.Ejecutar(desde, hasta, sucursalId, categoriaId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        StockSinVenta = new ObservableCollection<StockSinVentaDTO>(datos);
        ProductosSinVenta = datos.Count;
        ValorizadoSinVenta = datos.Sum(d => d.Valorizado);
        StockSinVentaSinDatos = datos.Count == 0;
    }

    // Producción por producto: usa los filtros globales (fechas + sucursal) de la barra superior.
    private void CargarProduccion()
    {
        if (!TryObtenerRango(out var desde, out var hasta)) return;

        // "Todas las Sucursales" (o un nombre que no se encuentre) = sin filtro (null).
        int? sucursalId = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalFiltro)?.Id;
        int? categoriaId = ProduccionCategoriaFiltro is { Id: > 0 } ? ProduccionCategoriaFiltro.Id : null;

        List<ProduccionPorProductoDTO> datos;
        try
        {
            datos = _obtenerProduccionPorProducto.Ejecutar(desde, hasta, null, sucursalId, categoriaId);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Reportes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el reporte.\n\nDetalle: {ex.Message}", "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
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
        // Rotadas para que los nombres largos no se solapen entre sí.
        ProduccionXAxes = new Axis[]
        {
            new Axis
            {
                Labels = datos.Select(d => d.NombreProducto).ToArray(),
                LabelsRotation = -45,
                TextSize = 10
            }
        };
    }

    [RelayCommand]
    private void AlternarRanking()
    {
        MostrarMenosVendidos = !MostrarMenosVendidos;
        CargarRankingProductos();
    }
}
