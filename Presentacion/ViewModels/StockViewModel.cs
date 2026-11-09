using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

namespace Presentacion.ViewModels;

public class StockProductoItem
{
    public int ProductoId { get; set; }
    public int SucursalId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; } = 15;
    public string NivelAlerta => StockActual == 0 ? "Crítico (Sin Stock)" : StockActual <= StockMinimo ? "Bajo Stock" : "Óptimo";
}

public partial class StockViewModel : ObservableObject, ISeccionRecargable
{
    private const string TodasConsolidado = "Todas (Consolidado)";
    private const string TodasCategorias = "Todas";

    private readonly ListarSucursales _listarSucursales;
    private readonly ListarCategorias _listarCategorias;
    private readonly ConsultarStockPorSucursal _consultarStock;
    private readonly AjustarStockProducto _ajustarStock;

    private List<SucursalDTO> _sucursalesDb = new();
    private List<CategoriaDTO> _categoriasDb = new();

    [ObservableProperty]
    private string _tabActivo = "Productos"; // "Productos" o "Insumos"

    [ObservableProperty]
    private string _filtroSucursal = TodasConsolidado;

    [ObservableProperty]
    private string _filtroCategoria = TodasCategorias;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // El vendedor solo ve su propia sucursal (tomada de la sesión, no elegible).
    [ObservableProperty]
    private bool _esAdmin = SesionActual.EsAdmin;

    // - Productos por sucursal: admin y vendedor (RF-12).
    // - Insumos globales (cocina central): admin y cocinero.
    public bool PuedeVerProductosTab => EsAdmin || SesionActual.EsVendedor;

    public bool PuedeVerInsumosTab => EsAdmin || SesionActual.EsCocinero;

    // Productos Stock (detalle por sucursal o consolidado según el filtro)
    [ObservableProperty]
    private ObservableCollection<StockProductoItem> _stockProductos = new();

    [ObservableProperty]
    private bool _mostrarDialogoAjusteProducto;

    [ObservableProperty]
    private StockProductoItem? _productoAjustando;

    [ObservableProperty]
    private int _nuevoStockProducto;

    public ObservableCollection<string> Sucursales { get; } = new();

    public ObservableCollection<string> Categorias { get; } = new();

    public InsumoViewModel InsumosVM { get; }

    public StockViewModel(
        InsumoViewModel insumosVM,
        ListarSucursales listarSucursales,
        ListarCategorias listarCategorias,
        ConsultarStockPorSucursal consultarStock,
        AjustarStockProducto ajustarStock)
    {
        InsumosVM = insumosVM;
        _listarSucursales = listarSucursales;
        _listarCategorias = listarCategorias;
        _consultarStock = consultarStock;
        _ajustarStock = ajustarStock;

        // Si no puede ver el tab de productos (cocinero), arranca en insumos.
        if (!PuedeVerProductosTab && PuedeVerInsumosTab)
            TabActivo = "Insumos";

        CargarSucursales();
        CargarCategorias();
        CargarStock();
    }

/* leido */
    private void CargarCategorias()
    {
        string seleccionPrevia = FiltroCategoria;
        _categoriasDb = _listarCategorias.Ejecutar();

        Categorias.Clear();
        Categorias.Add(TodasCategorias);
        foreach (var c in _categoriasDb)
            Categorias.Add(c.Nombre);

        FiltroCategoria = Categorias.Contains(seleccionPrevia) ? seleccionPrevia : TodasCategorias;
    }

/* leido */
    private void CargarSucursales()
    {
        string seleccionPrevia = FiltroSucursal;
        _sucursalesDb = _listarSucursales.Ejecutar();

        Sucursales.Clear();

        if (EsAdmin)
        {
            Sucursales.Add(TodasConsolidado);
            foreach (var s in _sucursalesDb)
                Sucursales.Add(s.Nombre);
            FiltroSucursal = Sucursales.Contains(seleccionPrevia) ? seleccionPrevia : TodasConsolidado;
        }
        else
        {
            var sucursalSesion = _sucursalesDb
                .FirstOrDefault(s => s.Id == SesionActual.UsuarioLogueado?.SucursalId);

            if (sucursalSesion != null)
            {
                Sucursales.Add(sucursalSesion.Nombre);
                FiltroSucursal = sucursalSesion.Nombre;
            }
        }
    }

    public void Recargar()
    {
        CargarSucursales();
        CargarCategorias();
        InsumosVM.Recargar();
        CargarStock();
    }

/* leido */
    private void CargarStock()
    {
        int? sucursalId = null;

        // el vendedor SIEMPRE consulta su propia sucursal,
        // venga el filtro como venga (no puede ver otras ni el consolidado).
        if (!EsAdmin)
        {
            sucursalId = SesionActual.UsuarioLogueado?.SucursalId;
        }
        else if (FiltroSucursal != TodasConsolidado)
        {
            var sucursal = _sucursalesDb.FirstOrDefault(s => s.Nombre == FiltroSucursal);
            if (sucursal != null)
                sucursalId = sucursal.Id;
        }

        int? categoriaId = null;
        if (FiltroCategoria != TodasCategorias)
            categoriaId = _categoriasDb.FirstOrDefault(c => c.Nombre == FiltroCategoria)?.Id;

        var consulta = _consultarStock.Ejecutar(sucursalId, categoriaId);

        // Sin sucursal concreta (admin, "Todas"): el consolidado es la vista principal.
        var filas = sucursalId.HasValue ? consulta.Detalle : consulta.Consolidado;

        AplicarStock(filas);
    }

    private void AplicarStock(List<StockSucursalDTO> filas)
    {
        StockProductos.Clear();

        foreach (var f in filas.Where(FiltrarPorTexto))
        {
            StockProductos.Add(new StockProductoItem
            {
                ProductoId = f.ProductoId,
                SucursalId = f.SucursalId,
                Codigo = $"P-{f.ProductoId:D3}",
                Nombre = f.NombreProducto,
                Categoria = f.NombreCategoria,
                // Vista por sucursal: el nombre del combo. Consolidado: el que trae el DTO.
                Sucursal = FiltroSucursal == TodasConsolidado ? f.NombreSucursal : FiltroSucursal,
                StockActual = f.Cantidad
            });
        }
    }

    private bool FiltrarPorTexto(StockSucursalDTO f) =>
        string.IsNullOrWhiteSpace(BusquedaTexto) ||
        f.NombreProducto.Contains(BusquedaTexto, StringComparison.OrdinalIgnoreCase) ||
        f.NombreCategoria.Contains(BusquedaTexto, StringComparison.OrdinalIgnoreCase);

    partial void OnBusquedaTextoChanged(string value)
    {
        InsumosVM.BusquedaTexto = value;
        CargarStock();
    }

    partial void OnFiltroSucursalChanged(string value)
    {
        if (Sucursales.Count > 0)
            CargarStock();
    }

    partial void OnFiltroCategoriaChanged(string value)
    {
        // value es null mientras se hace Categorias.Clear() (el ComboBox limpia la selección).
        if (value != null && Categorias.Count > 0)
            CargarStock();
    }

    [RelayCommand]
    private void CambiarTab(string tab)
    {
        TabActivo = tab;
    }

    [RelayCommand]
    private void AbrirAjusteProducto(StockProductoItem? prod)
    {
        // el ajuste manual de stock es exclusivo del administrador.
        if (prod == null || !EsAdmin) return;
        if(prod.SucursalId == 0)
        {
            MessageBox.Show("Debe seleccionar una sucursal en el selector para poder ajustar el stock.", "Ajuste de stock",
            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ProductoAjustando = prod;
        NuevoStockProducto = prod.StockActual;
        MostrarDialogoAjusteProducto = true;
    }

    [RelayCommand]
    private void GuardarAjusteProducto()
    {
        if (ProductoAjustando == null)
        {
            MostrarDialogoAjusteProducto = false;
            return;
        }

        if (NuevoStockProducto < 0)
        {
            MessageBox.Show("La cantidad no puede ser negativa.", "Ajuste de stock",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NuevoStockProducto > 1000000)
        {
            MessageBox.Show("La cantidad no puede superar 1.000.000 unidades.", "Ajuste de stock",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // registra fecha/hora y la resultante nunca es negativa.
            _ajustarStock.Ejecutar(
                ProductoAjustando.SucursalId,
                ProductoAjustando.ProductoId,
                NuevoStockProducto);

            ProductoAjustando.StockActual = NuevoStockProducto;
            CargarStock();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ajuste de stock",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo ajustar el stock.\n\nDetalle: {ex.Message}", "Error inesperado",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MostrarDialogoAjusteProducto = false;
    }

    [RelayCommand]
    private void CerrarAjusteProducto()
    {
        MostrarDialogoAjusteProducto = false;
    }
}
