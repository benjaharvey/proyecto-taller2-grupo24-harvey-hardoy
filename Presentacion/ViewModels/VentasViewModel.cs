using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dominio.Enums;
using CasoConfirmarVenta = Aplicacion.CasosDeUso.ConfirmarVenta;

namespace Presentacion.ViewModels;

// Los campos de los productos del catalogo de venta
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

// Los campos para el detalle individual de un conjunto de detalles
public partial class DetalleVentaItem : ObservableObject
{
    public int DetalleId { get; set; }           // id de VentasDetalle en la base
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _cantidad = 1;

    // Lo usa el VM para persistir cuando el usuario tipea la cantidad
    public Action<DetalleVentaItem>? AlCambiarCantidad { get; set; }

    partial void OnCantidadChanged(int value) => AlCambiarCantidad?.Invoke(this);

    public decimal Subtotal => PrecioUnitario * Cantidad;
}

// Los campos para el registro del cliente desde el boton 'Crear Cliente'
public class ClienteRegistradoItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string InfoResumen => $"{NombreCompleto} · DNI: {Dni}";
}

// ViewModel del Punto de Venta: catalogo, carrito, cliente, cobro y comprobante.
public partial class VentasViewModel : ObservableObject
{
    // Los casos de uso que llamamos en el VM
    private readonly AbrirVenta _abrirVenta;
    private readonly AgregarItemVenta _agregarItem;
    private readonly ModificarCantidadItem _modificarCantidad;
    private readonly QuitarItemVenta _quitarItem;
    private readonly CasoConfirmarVenta _confirmarVenta;
    private readonly DescartarVentaPendiente _descartarVenta;
    private readonly ObtenerVenta _obtenerVenta;
    private readonly GenerarComprobante _generarComprobante;
    private readonly ConsultarStockPorSucursal _consultarStock;
    private readonly ListarProductos _listarProductos;
    private readonly ListarClientes _listarClientes;
    private readonly CrearCliente _crearCliente;

    // Identificadores de la venta en curso y de la sesion
    private int _ventaId;
    private int _usuarioId;
    private int _sucursalId;

    // Datos de la sesion que se muestran en el ticket
    [ObservableProperty] private string _sucursalActual = "";
    [ObservableProperty] private string _vendedorActual = "";

    // Cliente asociado a la venta: se exige recien al confirmar
    [ObservableProperty] private int? _clienteId;
    [ObservableProperty] private string _clienteDni = "";
    [ObservableProperty] private string _clienteNombre = "Consumidor Final";


    // Propiedades relacionadas a la seleccion del metodo de pago
    [ObservableProperty] private MetodoPago? _metodoPagoSeleccionado;
    public IReadOnlyList<MetodoPago> MetodosPago { get; } = Enum.GetValues<MetodoPago>();


    // Propiedades relacionadas a la seleccion/creacion del cliente
    [ObservableProperty] private bool _mostrarDialogoSeleccionarCliente;
    [ObservableProperty] private string _busquedaClienteTexto = string.Empty;
    [ObservableProperty] private ObservableCollection<ClienteRegistradoItem> _clientesRegistrados = new();
    [ObservableProperty] private ObservableCollection<ClienteRegistradoItem> _clientesFiltrados = new();
    [ObservableProperty] private ClienteRegistradoItem? _clienteSeleccionadoEnLista;

    [ObservableProperty] private bool _mostrarDialogoNuevoCliente;
    [ObservableProperty] private string _nuevoClienteNombre = string.Empty;
    [ObservableProperty] private string _nuevoClienteApellido = string.Empty;
    [ObservableProperty] private string _nuevoClienteDni = string.Empty;
    [ObservableProperty] private string _nuevoClienteEmail = string.Empty;
    [ObservableProperty] private DateTime _nuevoClienteFechaNacimiento = new(2000, 1, 1);

    // Propiedades relacionadas al catalogo de productos
    [ObservableProperty] private string _busquedaCatalogo = string.Empty;
    [ObservableProperty] private string _categoriaFiltro = "Todas";
    [ObservableProperty] private ObservableCollection<CatalogoVentaItem> _catalogo = new();

    [ObservableProperty] private ObservableCollection<DetalleVentaItem> _carrito = new();

    // Categorias del catalogo: se llenan en CargarCatalogo
    public ObservableCollection<string> Categorias { get; } = new() { "Todas" };

    // Se calculan siempre desde el carrito en memoria, nunca con otra consulta
    public decimal TotalVenta => Carrito.Sum(item => item.Subtotal);
    public int CantidadTotalItems => Carrito.Sum(item => item.Cantidad);

    public VentasViewModel(
        AbrirVenta abrirVenta,
        AgregarItemVenta agregarItem,
        ModificarCantidadItem modificarCantidad,
        QuitarItemVenta quitarItem,
        CasoConfirmarVenta confirmarVenta,
        DescartarVentaPendiente descartarVenta,
        ObtenerVenta obtenerVenta,
        GenerarComprobante generarComprobante,
        ConsultarStockPorSucursal consultarStock,
        ListarProductos listarProductos,
        ListarClientes listarClientes,
        CrearCliente crearCliente)
    {
        _abrirVenta = abrirVenta;
        _agregarItem = agregarItem;
        _modificarCantidad = modificarCantidad;
        _quitarItem = quitarItem;
        _confirmarVenta = confirmarVenta;
        _descartarVenta = descartarVenta;
        _obtenerVenta = obtenerVenta;
        _generarComprobante = generarComprobante;
        _consultarStock = consultarStock;
        _listarProductos = listarProductos;
        _listarClientes = listarClientes;
        _crearCliente = crearCliente;

        Carrito.CollectionChanged += (_, _) => NotificarTotales();

        // El POS es del vendedor: admin y cocinero no deben abrir una venta pendiente.
        if (!SesionActual.EsVendedor) return;

        Inicializar();
    }

    // Retoma la venta pendiente del vendedor o crea una nueva
    private void Inicializar()
    {
        var usuario = SesionActual.UsuarioLogueado;
        if (usuario == null) return;

        _usuarioId = usuario.Id;
        _sucursalId = usuario.SucursalId;
        SucursalActual = usuario.NombreSucursal;
        VendedorActual = usuario.NombreCompleto;

        try
        {
            var venta = _abrirVenta.Ejecutar(_usuarioId, _sucursalId);
            _ventaId = venta.Id;

            RecargarCarrito();
            CargarCatalogo();
            CargarClientes();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo abrir la venta: {ex.Message}",
                "Ventas", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- Catálogo y carrito ----------

    // Cruza el stock de la sucursal con el precio de cada producto activo
    private void CargarCatalogo()
    {
        var stock = _consultarStock.Ejecutar(_sucursalId);
        var productos = _listarProductos.Ejecutar();

        Catalogo.Clear();

        foreach (var s in stock.Detalle)
        {
            var producto = productos.FirstOrDefault(p => p.Id == s.ProductoId);
            if (producto == null) continue;

            Catalogo.Add(new CatalogoVentaItem
            {
                Id = s.ProductoId,
                Codigo = $"P-{s.ProductoId:D4}",
                Nombre = s.NombreProducto,
                Categoria = s.NombreCategoria,
                Precio = producto.Precio,
                StockDisponible = s.Cantidad
            });
        }

        Categorias.Clear();
        Categorias.Add("Todas");
        foreach (var categoria in Catalogo
                     .Select(c => c.Categoria)
                     .Where(c => !string.IsNullOrWhiteSpace(c))
                     .Distinct()
                     .OrderBy(c => c))
        {
            Categorias.Add(categoria);
        }
    }

    // Se recarga desde la base para tener el DetalleId y el total que calculo el repositorio
    private void RecargarCarrito()
    {
        var venta = _obtenerVenta.Ejecutar(_ventaId);
        if (venta == null) return;

        Carrito.Clear();

        foreach (var detalle in venta.Detalles)
        {
            var item = new DetalleVentaItem
            {
                DetalleId = detalle.Id,
                ProductoId = detalle.ProductoId,
                Nombre = detalle.NombreProducto,
                PrecioUnitario = detalle.PrecioUnit,
                Cantidad = detalle.Cantidad
            };

            // Se engancha después de crear el item: la cantidad que viene de la
            // base no debe disparar el guardado.
            item.AlCambiarCantidad = PersistirCantidadTipeada;
            Carrito.Add(item);
        }

        NotificarTotales();
    }

    [RelayCommand]
    private void AgregarAlCarrito(CatalogoVentaItem? item)
    {
        if (item == null) return;

        // Sin stock en esta sucursal ni siquiera se intenta (el caso de uso tambien lo valida)
        if (item.StockDisponible <= 0)
        {
            Aviso($"No hay stock de {item.Nombre} en esta sucursal.");
            return;
        }

        try
        {
            _agregarItem.Ejecutar(_ventaId, item.Id, 1);
            RecargarCarrito();
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void IncrementarCantidad(DetalleVentaItem? item)
    {
        if (item == null) return;

        try
        {
            _modificarCantidad.Ejecutar(_ventaId, item.DetalleId, item.Cantidad + 1);
            RecargarCarrito();
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void DecrementarCantidad(DetalleVentaItem? item)
    {
        if (item == null) return;

        try
        {
            // En cantidad 1, el menos quita el item: no existe cantidad 0
            if (item.Cantidad > 1)
                _modificarCantidad.Ejecutar(_ventaId, item.DetalleId, item.Cantidad - 1);
            else
                _quitarItem.Ejecutar(_ventaId, item.DetalleId);

            RecargarCarrito();
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void QuitarDelCarrito(DetalleVentaItem? item)
    {
        if (item == null) return;

        try
        {
            _quitarItem.Ejecutar(_ventaId, item.DetalleId);
            RecargarCarrito();
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); }
        catch (Exception ex) { Error(ex); }
    }

    // Se dispara cuando el usuario tipea la cantidad y sale del TextBox
    private void PersistirCantidadTipeada(DetalleVentaItem item)
    {
        if (item.Cantidad <= 0)
        {
            Aviso("La cantidad debe ser mayor a cero.");
            RecargarCarrito();
            return;
        }

        try
        {
            _modificarCantidad.Ejecutar(_ventaId, item.DetalleId, item.Cantidad);
            RecargarCarrito();
        }
        catch (InvalidOperationException ex)
        {
            Aviso(ex.Message);
            RecargarCarrito();
        }
        catch (Exception ex)
        {
            Error(ex);
            RecargarCarrito();
        }
    }

    // ---------- Confirmación / descarte ----------

    [RelayCommand]
    private void ConfirmarVenta()
    {
        // Validaciones de negocio antes de abrir la transaccion
        if (Carrito.Count == 0) { Aviso("El carrito está vacío."); return; }
        if (ClienteId is null) { Aviso("Seleccioná un cliente antes de cobrar."); return; }
        if (MetodoPagoSeleccionado is null) { Aviso("Seleccioná el método de pago."); return; }

        var confirmacion = MessageBox.Show(
            $"¿Confirmar la venta por ${TotalVenta:N2}? Se descontará el stock.",
            "Confirmar venta", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes) return;

        var ventaConfirmadaId = _ventaId;

        try
        {
            _confirmarVenta.Ejecutar(ventaConfirmadaId, ClienteId.Value, MetodoPagoSeleccionado.Value);
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); return; }
        catch (Exception ex) { Error(ex); return; }

        // La venta ya es un hecho en la db
        try
        {
            var ruta = _generarComprobante.Ejecutar(ventaConfirmadaId);
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"La venta se confirmó, pero no se pudo generar el comprobante:\n{ex.Message}\n\n" +
                "Podés regenerarlo desde el historial.",
                "Comprobante", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        IniciarNuevaVenta();
    }

    [RelayCommand]
    private void CancelarVenta()
    {
        if (Carrito.Count == 0) { Aviso("No hay una venta en curso para descartar."); return; }

        var confirmacion = MessageBox.Show(
            "¿Descartar la venta en curso? El carrito se elimina y no se afecta el stock.",
            "Descartar venta", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            _descartarVenta.Ejecutar(_usuarioId);
            IniciarNuevaVenta();
        }
        catch (Exception ex) { Error(ex); }
    }

    // Despues de confirmar o descartar, el POS queda listo para la proxima venta
    private void IniciarNuevaVenta()
    {
        try
        {
            var nueva = _abrirVenta.Ejecutar(_usuarioId, _sucursalId);
            _ventaId = nueva.Id;

            LimpiarVenta();
            CargarCatalogo();
        }
        catch (Exception ex) { Error(ex); }
    }

    // Limpia el estado local: el carrito de la base ya lo maneja el caso de uso
    private void LimpiarVenta()
    {
        Carrito.Clear();
        ClienteId = null;
        ClienteNombre = "Consumidor Final";
        ClienteDni = "";
        MetodoPagoSeleccionado = null;
        NotificarTotales();
    }

    // ---------- Clientes ----------

    private void CargarClientes()
    {
        var clientes = _listarClientes.Ejecutar();

        ClientesRegistrados = new ObservableCollection<ClienteRegistradoItem>(
            clientes.Select(c => new ClienteRegistradoItem
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Dni = c.Dni,
                Email = c.Email
            }));

        ClientesFiltrados = new ObservableCollection<ClienteRegistradoItem>(ClientesRegistrados);
    }

    partial void OnBusquedaClienteTextoChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ClientesFiltrados = new ObservableCollection<ClienteRegistradoItem>(ClientesRegistrados);
            return;
        }

        var filtrados = ClientesRegistrados.Where(c =>
            c.NombreCompleto.Contains(value, StringComparison.OrdinalIgnoreCase) ||
            c.Dni.Contains(value, StringComparison.OrdinalIgnoreCase) ||
            c.Email.Contains(value, StringComparison.OrdinalIgnoreCase));

        ClientesFiltrados = new ObservableCollection<ClienteRegistradoItem>(filtrados);
    }

    [RelayCommand]
    private void AbrirSeleccionarClienteModal()
    {
        BusquedaClienteTexto = string.Empty;
        CargarClientes();
        MostrarDialogoSeleccionarCliente = true;
    }

    [RelayCommand]
    private void AsociarClienteSeleccionado(ClienteRegistradoItem? cliente)
    {
        if (cliente != null)
        {
            ClienteId = cliente.Id;
            ClienteNombre = cliente.NombreCompleto;
            ClienteDni = cliente.Dni;
        }

        MostrarDialogoSeleccionarCliente = false;
    }

    [RelayCommand]
    private void CerrarSeleccionarClienteModal() => MostrarDialogoSeleccionarCliente = false;

    [RelayCommand]
    private void AbrirNuevoClienteModal()
    {
        MostrarDialogoSeleccionarCliente = false;
        NuevoClienteNombre = string.Empty;
        NuevoClienteApellido = string.Empty;
        NuevoClienteDni = string.Empty;
        NuevoClienteEmail = string.Empty;
        NuevoClienteFechaNacimiento = new DateTime(2000, 1, 1);
        MostrarDialogoNuevoCliente = true;
    }

    [RelayCommand]
    private void GuardarNuevoCliente()
    {
        var dto = new ClienteDTO
        {
            Nombre = NuevoClienteNombre,
            Apellido = NuevoClienteApellido,
            Dni = NuevoClienteDni,
            Email = NuevoClienteEmail,
            FechaNacimiento = NuevoClienteFechaNacimiento,
            UsuarioId = _usuarioId
        };

        try
        {
            _crearCliente.Ejecutar(dto);
        }
        catch (InvalidOperationException ex)
        {
            Aviso(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            Error(ex);
            return;
        }

        CargarClientes();

        var creado = ClientesRegistrados.FirstOrDefault(c => c.Dni == dto.Dni);
        if (creado != null)
            AsociarClienteSeleccionado(creado);

        MostrarDialogoNuevoCliente = false;
    }

    [RelayCommand]
    private void CerrarNuevoClienteModal() => MostrarDialogoNuevoCliente = false;

    // Infra del VM

    private void NotificarTotales()
    {
        OnPropertyChanged(nameof(TotalVenta));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    private static void Aviso(string mensaje) =>
        MessageBox.Show(mensaje, "Ventas", MessageBoxButton.OK, MessageBoxImage.Warning);

    private static void Error(Exception ex) =>
        MessageBox.Show($"Ocurrió un error: {ex.Message}", "Ventas", MessageBoxButton.OK, MessageBoxImage.Error);
}