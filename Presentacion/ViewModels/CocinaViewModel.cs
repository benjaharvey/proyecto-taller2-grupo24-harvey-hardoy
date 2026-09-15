using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class ConsumoInsumoItem : ObservableObject
{
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
    public string InsumosResumen { get; set; } = string.Empty;
    public string Estado { get; set; } = "ENTREGADO";
}

public partial class CocinaViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ProductoElaborableItem> _productosDisponibles = new();

    [ObservableProperty]
    private ProductoElaborableItem? _productoSeleccionado;

    [ObservableProperty]
    private int _cantidadAElaborar = 50;

    [ObservableProperty]
    private string _sucursalDestino = "Sucursal Centro";

    [ObservableProperty]
    private ObservableCollection<ConsumoInsumoItem> _calculoConsumos = new();

    [ObservableProperty]
    private bool _esProduccionFactible = true;

    [ObservableProperty]
    private string _mensajeFactibilidad = "Producción factible: todos los insumos están disponibles en el stock global.";

    [ObservableProperty]
    private ObservableCollection<ProduccionHistoricaItem> _historialProduccion = new();

    [ObservableProperty]
    private string _cocineroActual = "Segundo Hardoy (Cocinero / Maestro Pastelero)";

    public ObservableCollection<string> Sucursales { get; } = new()
    {
        "Sucursal Centro", "Sucursal Norte", "Sucursal Sur"
    };

    public CocinaViewModel()
    {
        ActualizarFactibilidad();
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
            var totalReq = Math.Round(fb.CantidadPorUnidad * CantidadAElaborar, 2);
            var item = new ConsumoInsumoItem
            {
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
                faltantesTexto += $"{item.InsumoNombre} (Faltan {item.Faltante:N2} {item.UnidadMedida}), ";
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

        // Descontar insumos mock
        foreach (var c in CalculoConsumos)
        {
            c.StockGlobalDisponible -= c.TotalRequerido;
        }

        var insumosTexto = string.Join(", ", CalculoConsumos.Select(c => $"{c.TotalRequerido:N1}{c.UnidadMedida} {c.InsumoNombre}"));

        var nuevoLote = new ProduccionHistoricaItem
        {
            CodigoLote = $"PRD-2026-00{HistorialProduccion.Count + 46}",
            FechaHora = DateTime.Now,
            ProductoNombre = ProductoSeleccionado.Nombre,
            CantidadElaborada = CantidadAElaborar,
            SucursalDestino = SucursalDestino,
            Cocinero = CocineroActual,
            InsumosResumen = insumosTexto,
            Estado = "ENVIADO Y STOCK SUMADO"
        };

        HistorialProduccion.Insert(0, nuevoLote);
        ActualizarFactibilidad();
    }
}
