using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;
public partial class InsumoRecetaItem : ObservableObject
{
    [ObservableProperty]
    private string _insumoNombre = string.Empty;

    [ObservableProperty]
    private decimal _cantidad = 10;

    [ObservableProperty]
    private string _unidadMedida = "gr";
}

public class InsumoOpcionItem
{
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = "gr";
}

public partial class ProductosViewModel : ObservableObject
{

    private readonly CrearProducto _crearProducto;
    private readonly ActualizarProducto _actualizarProducto;
    private readonly EliminarProducto _eliminarProducto;
    private readonly ReactivarProducto _reactivarProducto;
    private readonly ListarProductos _listarProductos;

    private readonly ListarCategorias _listarCategorias;
    private readonly ListarInsumos _listarInsumos;

    [ObservableProperty]
    private ObservableCollection<ProductoDTO> _productos = new();

    private List<ProductoDTO> _todosLosProductos = new();

    [ObservableProperty]
    private ProductoDTO? _selectedProducto;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // null = "todas las categorías"
    [ObservableProperty]
    private CategoriaDTO? _filtroCategoria;

    public ObservableCollection<CategoriaDTO> Categorias { get; } = new();

    [ObservableProperty]
    private string _formTitulo = "Nuevo Producto";

    // Form fields
    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private CategoriaDTO? _formCategoria;

    [ObservableProperty]
    private int _formPrecio = 0;

    [ObservableProperty]
    private bool _esEdicion;

    // null = producto nuevo; con valor = Id del producto que se está editando
    private int? _productoEditandoId;

    [ObservableProperty]
    private ObservableCollection<InsumoRecetaItem> _formRecetaInsumos = new();

    // Dialogo agregar insumo a receta
    [ObservableProperty]
    private bool _mostrarModalAgregarInsumo;

    [ObservableProperty]
    private InsumoOpcionItem? _insumoSeleccionadoParaReceta;

    [ObservableProperty]
    private decimal _nuevoInsumoCantidad = 50;
    public ObservableCollection<InsumoOpcionItem> InsumosDisponibles { get; } = new();

    public ProductosViewModel(CrearProducto crearProducto,
    ActualizarProducto actualizarProducto,
    EliminarProducto eliminarProducto,
    ReactivarProducto reactivarProducto,
    ListarProductos listarProductos,
    ListarCategorias listarCategorias,
    ListarInsumos listarInsumos)
    {
        _crearProducto = crearProducto;
        _actualizarProducto = actualizarProducto;
        _eliminarProducto = eliminarProducto;
        _reactivarProducto = reactivarProducto;
        _listarProductos = listarProductos;
        _listarCategorias = listarCategorias;
        _listarInsumos = listarInsumos;

        foreach (var categoria in _listarCategorias.Ejecutar())
        {
            Categorias.Add(categoria);
        }

        foreach (var insumo in _listarInsumos.Ejecutar())
        {
            InsumosDisponibles.Add(new InsumoOpcionItem
            {
                Nombre = insumo.Nombre,
                UnidadMedida = insumo.Unidad
            });
        }

        CargarGrilla();
        LimpiarFormulario();
    }

    public void CargarGrilla()
    {
        _todosLosProductos = _listarProductos.Ejecutar(incluirEliminados: true)
            .OrderBy(p => !p.Activo)
            .ThenBy(p => p.Nombre)
            .ToList();

        AplicarFiltro();
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnFiltroCategoriaChanged(CategoriaDTO? value)
    {
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        IEnumerable<ProductoDTO> filtrados = _todosLosProductos;

        if (!string.IsNullOrWhiteSpace(BusquedaTexto))
        {
            var texto = BusquedaTexto.Trim();
            filtrados = filtrados.Where(p =>
                p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                p.NombreCategoria.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (FiltroCategoria != null)
        {
            filtrados = filtrados.Where(p => p.CategoriaId == FiltroCategoria.Id);
        }

        Productos = new ObservableCollection<ProductoDTO>(filtrados);
    }

    // Al seleccionar una fila de la grilla se carga el formulario en modo edición
    partial void OnSelectedProductoChanged(ProductoDTO? value)
    {
        if (value != null)
        {
            EsEdicion = true;
            _productoEditandoId = value.Id;
            FormTitulo = $"Editar {value.Nombre}";
            FormNombre = value.Nombre;
            FormPrecio = value.Precio;
            FormCategoria = Categorias.FirstOrDefault(c => c.Id == value.CategoriaId);
            FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
        }
        else
        {
            LimpiarFormulario();
        }
    }

    private void LimpiarFormulario()
    {
        SelectedProducto = null;
        EsEdicion = false;
        FormTitulo = "Nuevo Producto";
        FormNombre = string.Empty;
        FormCategoria = null;
        FormPrecio = 0;
        _productoEditandoId = null;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        BusquedaTexto = string.Empty;
        FiltroCategoria = null;
    }

    [RelayCommand]
    private void AbrirModalAgregarInsumo()
    {
        InsumoSeleccionadoParaReceta = InsumosDisponibles.FirstOrDefault();
        NuevoInsumoCantidad = 30;
        MostrarModalAgregarInsumo = true;
    }

    [RelayCommand]
    private void ConfirmarAgregarInsumo()
    {
        if (InsumoSeleccionadoParaReceta != null && NuevoInsumoCantidad > 0)
        {
            var existe = FormRecetaInsumos.FirstOrDefault(i => i.InsumoNombre.Equals(InsumoSeleccionadoParaReceta.Nombre, System.StringComparison.OrdinalIgnoreCase));
            if (existe != null)
            {
                existe.Cantidad += NuevoInsumoCantidad;
            }
            else
            {
                FormRecetaInsumos.Add(new InsumoRecetaItem
                {
                    InsumoNombre = InsumoSeleccionadoParaReceta.Nombre,
                    Cantidad = NuevoInsumoCantidad,
                    UnidadMedida = InsumoSeleccionadoParaReceta.UnidadMedida
                });
            }
        }
        MostrarModalAgregarInsumo = false;
    }

    [RelayCommand]
    private void CerrarModalAgregarInsumo()
    {
        MostrarModalAgregarInsumo = false;
    }

    [RelayCommand]
    private void QuitarInsumoReceta(InsumoRecetaItem? item)
    {
        if (item != null)
        {
            FormRecetaInsumos.Remove(item);
        }
    }

    [RelayCommand]
    private void GuardarFormulario()
    {
        var dto = new ProductoCrearDTO
        {
            Nombre = FormNombre?.Trim() ?? string.Empty,
            Precio = FormPrecio,
            CategoriaId = FormCategoria?.Id ?? 0
        };

        try
        {
            if (_productoEditandoId.HasValue)
            {
                _actualizarProducto.Ejecutar(_productoEditandoId.Value, dto);
                MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _crearProducto.Ejecutar(dto);
                MessageBox.Show("Producto creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        LimpiarFormulario();
    }

    [RelayCommand]
    private void ToggleEstadoProducto(ProductoDTO? prod)
    {
        var producto = prod ?? SelectedProducto;
        if (producto == null) return;

        bool esActivo = producto.Activo;
        string accion = esActivo ? "dar de baja" : "reactivar";
        var confirmacion = MessageBox.Show(
            $"¿Seguro que querés {accion} el producto \"{producto.Nombre}\"?",
            "Confirmar acción",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            if (esActivo)
            {
                _eliminarProducto.Ejecutar(producto.Id);
            }
            else
            {
                _reactivarProducto.Ejecutar(producto.Id);
            }

            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al modificar el estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
