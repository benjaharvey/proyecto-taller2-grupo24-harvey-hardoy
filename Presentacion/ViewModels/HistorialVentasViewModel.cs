using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dominio.Consultas;
using Dominio.Enums;
using CasoCancelarVenta = Aplicacion.CasosDeUso.CancelarVenta;

namespace Presentacion.ViewModels;

// ViewModel del historial: filtros sobre SQL, detalle de la venta y cancelacion (solo admin).
public partial class HistorialVentasViewModel : ObservableObject
{
    private readonly ListarVentas _listarVentas;
    private readonly CasoCancelarVenta _cancelarVenta;
    private readonly GenerarComprobante _generarComprobante;
    private readonly ListarSucursales _listarSucursales;
    private readonly ListarUsuarios _listarUsuarios;

    private readonly int _usuarioId;

    // Solo un usuario con este rol puede vender
    private const string RolVendedor = "Vendedor";

    // Casa Central y Fábrica son administrativa/producción, no puntos de venta.
    // Cuando el modelo tenga un TipoSucursal, esta lista se reemplaza.
    private static readonly string[] SucursalesSinVenta = { "Casa Central", "Fábrica" };

    // Filtros combinables: null o vacio = sin filtrar
    [ObservableProperty] private int? _filtroSucursalId;
    [ObservableProperty] private DateTime? _filtroDesde;
    [ObservableProperty] private DateTime? _filtroHasta;
    [ObservableProperty] private string _filtroClienteTexto = "";
    [ObservableProperty] private int? _filtroUsuarioId;
    [ObservableProperty] private MetodoPago? _filtroMetodoPago;

    // Datos de la grilla y del panel de detalle
    [ObservableProperty] private ObservableCollection<VentaDTO> _ventas = new();
    [ObservableProperty] private VentaDTO? _ventaSeleccionada;

    // Combos de filtros
    [ObservableProperty] private ObservableCollection<SucursalDTO> _sucursales = new();
    [ObservableProperty] private ObservableCollection<UsuarioDTO> _vendedores = new();
    public IReadOnlyList<MetodoPago> MetodosPago { get; } = Enum.GetValues<MetodoPago>();

    // RF-23: las canceladas no suman en los totales
    public int TotalOperaciones => Ventas.Count(v => v.Estado == EstadoVenta.CONFIRMADA);
    public int TotalUnidades => Ventas.Where(v => v.Estado == EstadoVenta.CONFIRMADA).Sum(v => v.CantidadItems);
    public decimal TotalImporte => Ventas.Where(v => v.Estado == EstadoVenta.CONFIRMADA).Sum(v => v.Total);

    public bool PuedeCancelarSeleccionada => VentaSeleccionada?.Estado == EstadoVenta.CONFIRMADA;

    public HistorialVentasViewModel(
        ListarVentas listarVentas,
        CasoCancelarVenta cancelarVenta,
        GenerarComprobante generarComprobante,
        ListarSucursales listarSucursales,
        ListarUsuarios listarUsuarios)
    {
        _listarVentas = listarVentas;
        _cancelarVenta = cancelarVenta;
        _generarComprobante = generarComprobante;
        _listarSucursales = listarSucursales;
        _listarUsuarios = listarUsuarios;

        _usuarioId = SesionActual.UsuarioLogueado?.Id ?? 0;

        // La vista existe para todos, pero solo el admin tiene acceso al historial.
        if (!SesionActual.EsAdmin) return;

        CargarCombos();
        Buscar();
    }

    private void CargarCombos()
    {
        try
        {
            var usuarios = _listarUsuarios.Ejecutar();

            // El filtro de vendedor solo ofrece usuarios activos con rol Vendedor,
            // no todos los usuarios del sistema.
            var vendedores = usuarios
                .Where(u => u.Activo && u.NombreRol == RolVendedor)
                .ToList();

            Vendedores = new ObservableCollection<UsuarioDTO>(vendedores);

            Sucursales = new ObservableCollection<SucursalDTO>(
                _listarSucursales.Ejecutar()
                    .Where(s => !SucursalesSinVenta.Contains(s.Nombre, StringComparer.OrdinalIgnoreCase)));
        }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void Buscar()
    {
        var filtro = new VentaFiltro
        {
            SucursalId = FiltroSucursalId,
            Desde = FiltroDesde,
            Hasta = FiltroHasta,
            ClienteTexto = string.IsNullOrWhiteSpace(FiltroClienteTexto) ? null : FiltroClienteTexto.Trim(),
            UsuarioId = FiltroUsuarioId,
            MetodoPago = FiltroMetodoPago
        };

        try
        {
            Ventas = new ObservableCollection<VentaDTO>(_listarVentas.Ejecutar(filtro));
            VentaSeleccionada = null;
            NotificarTotales();
        }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        FiltroSucursalId = null;
        FiltroDesde = null;
        FiltroHasta = null;
        FiltroClienteTexto = "";
        FiltroUsuarioId = null;
        FiltroMetodoPago = null;

        Buscar();
    }

    [RelayCommand]
    private void VerComprobante()
    {
        if (VentaSeleccionada == null)
        {
            Aviso("Seleccioná una venta para ver su comprobante.");
            return;
        }

        try
        {
            var ruta = _generarComprobante.Ejecutar(VentaSeleccionada.Id);
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }
        catch (Exception ex) { Error(ex); }
    }

    [RelayCommand]
    private void CancelarVenta()
    {
        if (VentaSeleccionada == null || !PuedeCancelarSeleccionada) return;

        var confirmacion = MessageBox.Show(
            $"¿Cancelar la venta {VentaSeleccionada.NumeroComprobante}?\n" +
            "Se repondrá el stock y la operación no se puede deshacer.",
            "Cancelar venta", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            // El caso de uso valida el rol de admin y el estado CONFIRMADA.
            // Se guarda el Id porque Buscar() limpia la selección de la grilla.
            var ventaId = VentaSeleccionada.Id;

            _cancelarVenta.Ejecutar(ventaId, _usuarioId);
            Buscar();

            // La venta ya quedó cancelada: se reescribe el PDF con la leyenda
            // CANCELADA para que el archivo en disco no contradiga a la base.
            // Si falla, solo se avisa: la cancelación ya está hecha.
            try
            {
                _generarComprobante.Ejecutar(ventaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"La venta se canceló, pero no se pudo regenerar el comprobante:\n{ex.Message}",
                    "Comprobante", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (InvalidOperationException ex) { Aviso(ex.Message); }
        catch (Exception ex) { Error(ex); }
    }

    partial void OnVentaSeleccionadaChanged(VentaDTO? value)
    {
        OnPropertyChanged(nameof(PuedeCancelarSeleccionada));
    }

    private void NotificarTotales()
    {
        OnPropertyChanged(nameof(TotalOperaciones));
        OnPropertyChanged(nameof(TotalUnidades));
        OnPropertyChanged(nameof(TotalImporte));
    }

    private static void Aviso(string mensaje) =>
        MessageBox.Show(mensaje, "Historial de Ventas", MessageBoxButton.OK, MessageBoxImage.Warning);

    private static void Error(Exception ex) =>
        MessageBox.Show($"Ocurrió un error: {ex.Message}", "Historial de Ventas", MessageBoxButton.OK, MessageBoxImage.Error);
}
