using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class CatalogoVentaItem
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int StockDisponible { get; set; }
    public string Icono { get; set; } = "🍪";
}

public partial class DetalleVentaItem : ObservableObject
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _cantidad = 1;

    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public class VentaHistoricaItem
{
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public int CantidadArticulos { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "CONFIRMADA"; // CONFIRMADA, ANULADA, CANCELADA
}

public partial class VentasViewModel : ObservableObject
{
    [ObservableProperty]
    private string _vistaActiva = "Caja"; // "Caja" o "Historial"

    [ObservableProperty]
    private string _sucursalActual = "Sucursal Centro";

    [ObservableProperty]
    private string _vendedorActual = "Segundo Hardoy";

    // Cliente
    [ObservableProperty]
    private string _clienteDni = string.Empty;

    [ObservableProperty]
    private string _clienteNombre = "Consumidor Final";

    [ObservableProperty]
    private bool _mostrarDialogoNuevoCliente;

    [ObservableProperty]
    private string _nuevoClienteNombre = string.Empty;

    [ObservableProperty]
    private string _nuevoClienteApellido = string.Empty;

    [ObservableProperty]
    private string _nuevoClienteDni = string.Empty;

    [ObservableProperty]
    private string _nuevoClienteEmail = string.Empty;

    // Buscador y catalogo
    [ObservableProperty]
    private string _busquedaCatalogo = string.Empty;

    [ObservableProperty]
    private string _categoriaFiltro = "Todas";

    [ObservableProperty]
    private ObservableCollection<CatalogoVentaItem> _catalogo = new();

    // Carrito
    [ObservableProperty]
    private ObservableCollection<DetalleVentaItem> _carrito = new();

    // Historial
    [ObservableProperty]
    private ObservableCollection<VentaHistoricaItem> _historialVentas = new();

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Todas", "Alfajores", "Conitos", "Tabletas", "Tortas"
    };

    public decimal TotalVenta => Carrito.Sum(item => item.Subtotal);
    public int CantidadTotalItems => Carrito.Sum(item => item.Cantidad);

    public VentasViewModel()
    {
        CargarDatosMock();
    }

    private void CargarDatosMock()
    {
        Catalogo = new ObservableCollection<CatalogoVentaItem>
        {
            new() { Id = 1, Codigo = "ALF-001", Nombre = "Alfajor Clásico DDL", Categoria = "Alfajores", Precio = 1800.00m, StockDisponible = 45, Icono = "🍫" },
            new() { Id = 2, Codigo = "ALF-002", Nombre = "Alfajor Nuez y Choc Blanco", Categoria = "Alfajores", Precio = 2100.00m, StockDisponible = 22, Icono = "🌰" },
            new() { Id = 3, Codigo = "CON-001", Nombre = "Conito Dulce de Leche", Categoria = "Conitos", Precio = 1600.00m, StockDisponible = 18, Icono = "🍦" },
            new() { Id = 4, Codigo = "TAB-001", Nombre = "Tableta Marroc Artesanal", Categoria = "Tabletas", Precio = 2400.00m, StockDisponible = 30, Icono = "🥜" },
            new() { Id = 5, Codigo = "TOR-001", Nombre = "Mini Torta Rogel", Categoria = "Tortas", Precio = 3500.00m, StockDisponible = 8, Icono = "🎂" },
            new() { Id = 6, Codigo = "ALF-003", Nombre = "Alfajor Maicena Tradicional", Categoria = "Alfajores", Precio = 1500.00m, StockDisponible = 35, Icono = "🥥" }
        };

        // Carrito inicial de muestra
        Carrito = new ObservableCollection<DetalleVentaItem>
        {
            new() { ProductoId = 1, Nombre = "Alfajor Clásico DDL", PrecioUnitario = 1800.00m, Cantidad = 2 },
            new() { ProductoId = 3, Nombre = "Conito Dulce de Leche", PrecioUnitario = 1600.00m, Cantidad = 1 }
        };
        Carrito.CollectionChanged += (s, e) => {
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(CantidadTotalItems));
        };

        HistorialVentas = new ObservableCollection<VentaHistoricaItem>
        {
            new() { NumeroVenta = "VTA-2026-0089", Fecha = DateTime.Now.AddHours(-1), ClienteNombre = "Juan Pérez (DNI: 35.849.120)", Vendedor = "Segundo Hardoy", Sucursal = "Sucursal Centro", CantidadArticulos = 4, Total = 7400.00m, Estado = "CONFIRMADA" },
            new() { NumeroVenta = "VTA-2026-0088", Fecha = DateTime.Now.AddHours(-3), ClienteNombre = "María González (DNI: 28.114.902)", Vendedor = "Segundo Hardoy", Sucursal = "Sucursal Centro", CantidadArticulos = 2, Total = 4200.00m, Estado = "CONFIRMADA" },
            new() { NumeroVenta = "VTA-2026-0087", Fecha = DateTime.Now.AddHours(-5), ClienteNombre = "Consumidor Final", Vendedor = "Segundo Hardoy", Sucursal = "Sucursal Centro", CantidadArticulos = 6, Total = 11200.00m, Estado = "ANULADA" },
            new() { NumeroVenta = "VTA-2026-0086", Fecha = DateTime.Now.AddDays(-1), ClienteNombre = "Carlos Rossi (DNI: 18.234.567)", Vendedor = "Segundo Hardoy", Sucursal = "Sucursal Centro", CantidadArticulos = 1, Total = 3500.00m, Estado = "CONFIRMADA" }
        };
    }

    [RelayCommand]
    private void CambiarTab(string tab)
    {
        VistaActiva = tab;
    }

    [RelayCommand]
    private void AgregarAlCarrito(CatalogoVentaItem? item)
    {
        if (item == null || item.StockDisponible <= 0) return;

        var existente = Carrito.FirstOrDefault(c => c.ProductoId == item.Id);
        if (existente != null)
        {
            if (existente.Cantidad < item.StockDisponible)
            {
                existente.Cantidad++;
                OnPropertyChanged(nameof(TotalVenta));
                OnPropertyChanged(nameof(CantidadTotalItems));
            }
        }
        else
        {
            var nuevo = new DetalleVentaItem
            {
                ProductoId = item.Id,
                Nombre = item.Nombre,
                PrecioUnitario = item.Precio,
                Cantidad = 1
            };
            nuevo.PropertyChanged += (s, e) => {
                OnPropertyChanged(nameof(TotalVenta));
                OnPropertyChanged(nameof(CantidadTotalItems));
            };
            Carrito.Add(nuevo);
        }
        OnPropertyChanged(nameof(TotalVenta));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    [RelayCommand]
    private void IncrementarCantidad(DetalleVentaItem? item)
    {
        if (item == null) return;
        var prod = Catalogo.FirstOrDefault(p => p.Id == item.ProductoId);
        if (prod != null && item.Cantidad < prod.StockDisponible)
        {
            item.Cantidad++;
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(CantidadTotalItems));
        }
    }

    [RelayCommand]
    private void DecrementarCantidad(DetalleVentaItem? item)
    {
        if (item == null) return;
        if (item.Cantidad > 1)
        {
            item.Cantidad--;
        }
        else
        {
            Carrito.Remove(item);
        }
        OnPropertyChanged(nameof(TotalVenta));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    [RelayCommand]
    private void QuitarDelCarrito(DetalleVentaItem? item)
    {
        if (item != null)
        {
            Carrito.Remove(item);
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(CantidadTotalItems));
        }
    }

    [RelayCommand]
    private void CancelarVenta()
    {
        Carrito.Clear();
        ClienteNombre = "Consumidor Final";
        ClienteDni = string.Empty;
        OnPropertyChanged(nameof(TotalVenta));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    [RelayCommand]
    private void ConfirmarVenta()
    {
        if (Carrito.Count == 0) return;

        var nuevaVenta = new VentaHistoricaItem
        {
            NumeroVenta = $"VTA-2026-00{HistorialVentas.Count + 90}",
            Fecha = DateTime.Now,
            ClienteNombre = string.IsNullOrWhiteSpace(ClienteDni) ? ClienteNombre : $"{ClienteNombre} (DNI: {ClienteDni})",
            Vendedor = VendedorActual,
            Sucursal = SucursalActual,
            CantidadArticulos = CantidadTotalItems,
            Total = TotalVenta,
            Estado = "CONFIRMADA"
        };

        HistorialVentas.Insert(0, nuevaVenta);
        CancelarVenta();
    }

    [RelayCommand]
    private void AbrirNuevoClienteModal()
    {
        NuevoClienteNombre = string.Empty;
        NuevoClienteApellido = string.Empty;
        NuevoClienteDni = string.Empty;
        NuevoClienteEmail = string.Empty;
        MostrarDialogoNuevoCliente = true;
    }

    [RelayCommand]
    private void GuardarNuevoCliente()
    {
        if (!string.IsNullOrWhiteSpace(NuevoClienteNombre) && !string.IsNullOrWhiteSpace(NuevoClienteDni))
        {
            ClienteNombre = $"{NuevoClienteNombre} {NuevoClienteApellido}".Trim();
            ClienteDni = NuevoClienteDni.Trim();
        }
        MostrarDialogoNuevoCliente = false;
    }

    [RelayCommand]
    private void CerrarNuevoClienteModal()
    {
        MostrarDialogoNuevoCliente = false;
    }

    [RelayCommand]
    private void AnularVenta(VentaHistoricaItem? venta)
    {
        if (venta != null && venta.Estado == "CONFIRMADA")
        {
            venta.Estado = "ANULADA";
            var index = HistorialVentas.IndexOf(venta);
            if (index >= 0)
            {
                HistorialVentas[index] = venta;
            }
        }
    }
}
