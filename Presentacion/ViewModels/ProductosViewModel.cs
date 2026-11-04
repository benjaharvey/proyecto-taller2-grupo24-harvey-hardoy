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
    public int InsumoId { get; set; }

    [ObservableProperty]
    private string _insumoNombre = string.Empty;

    [ObservableProperty]
    private decimal _cantidad = 10;

    [ObservableProperty]
    private string _unidadMedida = "gr";
}

public class InsumoOpcionItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = "gr";
}

public partial class ProductosViewModel : ObservableObject, ISeccionRecargable
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
    private decimal _formPrecio = 0m;

    [ObservableProperty]
    private decimal _formCosto = 0m;

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
                Id = insumo.Id,
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

    public void Recargar()
    {
        CargarGrilla();
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
            FormCosto = value.Costo;
            FormCategoria = Categorias.FirstOrDefault(c => c.Id == value.CategoriaId);
            FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>(
                (value.Receta ?? new List<ProductoInsumoDTO>()).Select(r => new InsumoRecetaItem
                {
                    InsumoId = r.InsumoId,
                    InsumoNombre = r.NombreInsumo,
                    Cantidad = r.CantidadNecesaria,
                    UnidadMedida = r.Unidad
                }));
        }
        else
        {
            LimpiarFormulario();
        }
    }

    [RelayCommand]
    private void EditarProducto(ProductoDTO? prod)
    {
        if (prod == null) return;
        SelectedProducto = prod;
    }

    private void LimpiarFormulario()
    {
        SelectedProducto = null;
        EsEdicion = false;
        FormTitulo = "Nuevo Producto";
        FormNombre = string.Empty;
        FormCategoria = null;
        FormPrecio = 0;
        FormCosto = 0;
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
        if (InsumoSeleccionadoParaReceta == null)
        {
            MessageBox.Show("Seleccioná un insumo para agregar a la receta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NuevoInsumoCantidad <= 0)
        {
            MessageBox.Show("La cantidad del insumo debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NuevoInsumoCantidad > 1000000)
        {
            MessageBox.Show("La cantidad del insumo no puede superar 1.000.000.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existe = FormRecetaInsumos.FirstOrDefault(i => i.InsumoId == InsumoSeleccionadoParaReceta.Id);
        if (existe != null)
        {
            existe.Cantidad += NuevoInsumoCantidad;
        }
        else
        {
            FormRecetaInsumos.Add(new InsumoRecetaItem
            {
                InsumoId = InsumoSeleccionadoParaReceta.Id,
                InsumoNombre = InsumoSeleccionadoParaReceta.Nombre,
                Cantidad = NuevoInsumoCantidad,
                UnidadMedida = InsumoSeleccionadoParaReceta.UnidadMedida
            });
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
        if (!ValidarFormulario(out var dto)) return;

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

    private bool ValidarFormulario(out ProductoCrearDTO dto)
    {
        dto = new ProductoCrearDTO();

        string nombre = FormNombre?.Trim() ?? string.Empty;

        if (nombre.Length == 0)
        {
            MessageBox.Show("Ingresá el nombre del producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (nombre.Length < 2 || nombre.Length > 40)
        {
            MessageBox.Show("El nombre del producto debe tener entre 2 y 40 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormPrecio <= 0)
        {
            MessageBox.Show("El precio debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormPrecio > 100000000)
        {
            MessageBox.Show("El precio ingresado no es válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormCosto < 0)
        {
            MessageBox.Show("El costo de producción no puede ser negativo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormCosto > 100000000)
        {
            MessageBox.Show("El costo ingresado no es válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormCategoria == null)
        {
            MessageBox.Show("Seleccioná una categoría.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormRecetaInsumos.Any(i => i.Cantidad <= 0))
        {
            MessageBox.Show("La cantidad de cada insumo de la receta debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        dto = new ProductoCrearDTO
        {
            Nombre = nombre,
            Precio = FormPrecio,
            Costo = FormCosto,
            CategoriaId = FormCategoria.Id,
            Receta = FormRecetaInsumos.Select(i => new ProductoInsumoDTO
            {
                InsumoId = i.InsumoId,
                CantidadNecesaria = i.Cantidad
            }).ToList()
        };

        return true;
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
