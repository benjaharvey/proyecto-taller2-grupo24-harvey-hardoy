using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

namespace Presentacion.ViewModels;

public class ConsumoInsumoItem : ObservableObject
{
    public int InsumoId { get; set; }
    public string InsumoNombre { get; set; } = string.Empty;
    public decimal CantidadPorUnidad { get; set; }
    public string UnidadMedida { get; set; } = "gr";
    public decimal TotalRequerido { get; set; }
    public decimal StockGlobalDisponible { get; set; }
    public bool EsSuficiente => StockGlobalDisponible >= TotalRequerido;
    public decimal Faltante => EsSuficiente ? 0 : TotalRequerido - StockGlobalDisponible;
}

public class ProductoElaborableItem
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public ObservableCollection<ConsumoInsumoItem> FormulaBase { get; set; } = new();
}

public class ProduccionHistoricaItem
{
    public string CodigoLote { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int CantidadElaborada { get; set; }
    public string SucursalDestino { get; set; } = string.Empty;
    public string Cocinero { get; set; } = string.Empty;
}

public partial class CocinaViewModel : ObservableObject
{
    private readonly ListarProductos _listarProductos;
    private readonly ListarInsumos _listarInsumos;
    private readonly ListarSucursales _listarSucursales;
    private readonly RegistrarProduccion _registrarProduccion;
    private readonly ListarProducciones _listarProducciones;

    private List<SucursalDTO> _sucursalesDb = new();

    [ObservableProperty]
    private ObservableCollection<ProductoElaborableItem> _productosDisponibles = new();

    [ObservableProperty]
    private ProductoElaborableItem? _productoSeleccionado;

    [ObservableProperty]
    private int _cantidadAElaborar = 1;

    [ObservableProperty]
    private string _sucursalDestino = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ConsumoInsumoItem> _calculoConsumos = new();

    [ObservableProperty]
    private bool _esProduccionFactible;

    [ObservableProperty]
    private string _mensajeFactibilidad = "Seleccione un producto y cantidad válida.";

    [ObservableProperty]
    private ObservableCollection<ProduccionHistoricaItem> _historialProduccion = new();

    public ObservableCollection<string> Sucursales { get; } = new();

    public CocinaViewModel(
        ListarProductos listarProductos,
        ListarInsumos listarInsumos,
        ListarSucursales listarSucursales,
        RegistrarProduccion registrarProduccion,
        ListarProducciones listarProducciones)
    {
        _listarProductos = listarProductos;
        _listarInsumos = listarInsumos;
        _listarSucursales = listarSucursales;
        _registrarProduccion = registrarProduccion;
        _listarProducciones = listarProducciones;

        CargarSucursales();
        CargarProductos();
        CargarHistorial();
        ActualizarFactibilidad();
    }

    private void CargarSucursales()
    {
        _sucursalesDb = _listarSucursales.Ejecutar();

        Sucursales.Clear();
        foreach (var s in _sucursalesDb)
            Sucursales.Add(s.Nombre);

        SucursalDestino = Sucursales.FirstOrDefault() ?? string.Empty;
    }

    // Trae productos con receta y el stock ACTUAL de cada insumo.
    private void CargarProductos()
    {
        var stockInsumos = _listarInsumos.Ejecutar().ToDictionary(i => i.Id);
        var seleccionadoId = ProductoSeleccionado?.Id;

        ProductosDisponibles.Clear();

        foreach (var p in _listarProductos.Ejecutar().Where(p => p.Receta.Count > 0))
        {
            var item = new ProductoElaborableItem
            {
                Id = p.Id,
                Codigo = $"P-{p.Id:D3}",
                Nombre = p.Nombre,
                Categoria = p.NombreCategoria
            };

            foreach (var r in p.Receta)
            {
                item.FormulaBase.Add(new ConsumoInsumoItem
                {
                    InsumoId = r.InsumoId,
                    InsumoNombre = r.NombreInsumo,
                    CantidadPorUnidad = r.CantidadNecesaria,
                    UnidadMedida = r.Unidad,
                    StockGlobalDisponible = stockInsumos.TryGetValue(r.InsumoId, out var insumo) ? insumo.Stock : 0
                });
            }

            ProductosDisponibles.Add(item);
        }

        // Si había uno elegido, lo vuelve a seleccionar con el stock actualizado.
        ProductoSeleccionado = ProductosDisponibles.FirstOrDefault(p => p.Id == seleccionadoId);
    }

    private void CargarHistorial()
    {
        HistorialProduccion.Clear();

        foreach (var p in _listarProducciones.Ejecutar())
        {
            HistorialProduccion.Add(new ProduccionHistoricaItem
            {
                CodigoLote = $"PRD-{p.Id:D4}",
                FechaHora = p.CreatedAt,
                ProductoNombre = p.NombreProducto,
                CantidadElaborada = p.Cantidad,
                SucursalDestino = p.NombreSucursal,
                Cocinero = p.NombreUsuario
            });
        }
    }

    partial void OnProductoSeleccionadoChanged(ProductoElaborableItem? value)
    {
        ActualizarFactibilidad();
    }

    partial void OnCantidadAElaborarChanged(int value)
    {
        ActualizarFactibilidad();
    }

    private void ActualizarFactibilidad()
    {
        CalculoConsumos.Clear();
        if (ProductoSeleccionado == null || CantidadAElaborar <= 0)
        {
            EsProduccionFactible = false;
            MensajeFactibilidad = "Seleccione un producto y cantidad válida.";
            return;
        }

        bool factible = true;
        string faltantesTexto = string.Empty;

        foreach (var fb in ProductoSeleccionado.FormulaBase)
        {
            var totalReq = Math.Round(fb.CantidadPorUnidad * CantidadAElaborar, 5);
            var item = new ConsumoInsumoItem
            {
                InsumoId = fb.InsumoId,
                InsumoNombre = fb.InsumoNombre,
                CantidadPorUnidad = fb.CantidadPorUnidad,
                UnidadMedida = fb.UnidadMedida,
                StockGlobalDisponible = fb.StockGlobalDisponible,
                TotalRequerido = totalReq
            };
            CalculoConsumos.Add(item);

            if (!item.EsSuficiente)
            {
                factible = false;
                faltantesTexto += $"{item.InsumoNombre} (Faltan {item.Faltante:0.#####} {item.UnidadMedida}), ";
            }
        }

        EsProduccionFactible = factible;
        if (factible)
        {
            MensajeFactibilidad = "✅ Producción factible: El stock global de insumos es suficiente para esta cantidad.";
        }
        else
        {
            MensajeFactibilidad = $"⚠️ Insumos insuficientes en cocina global: {faltantesTexto.TrimEnd(',', ' ')}";
        }
    }

    [RelayCommand]
    private void RegistrarProduccionYEnvio()
    {
        if (!EsProduccionFactible || ProductoSeleccionado == null || CantidadAElaborar <= 0) return;

        var sucursal = _sucursalesDb.FirstOrDefault(s => s.Nombre == SucursalDestino);
        if (sucursal == null)
        {
            MessageBox.Show("Seleccioná la sucursal de destino.", "Producción",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SesionActual.UsuarioLogueado == null)
        {
            MessageBox.Show("No hay una sesión activa. Volvé a iniciar sesión.", "Producción",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _registrarProduccion.Ejecutar(new ProduccionCrearDTO
            {
                ProductoId = ProductoSeleccionado.Id,
                SucursalId = sucursal.Id,
                UsuarioId = SesionActual.UsuarioLogueado.Id,
                Cantidad = CantidadAElaborar
            });
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Producción",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Recarga desde la base: stock de insumos descontado e historial con la nueva producción.
        CargarProductos();
        CargarHistorial();
        ActualizarFactibilidad();
    }
}
