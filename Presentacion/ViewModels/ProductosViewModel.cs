using System.Collections.ObjectModel;
using System.Linq;
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
    private string _formCodigo = string.Empty;

    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private CategoriaDTO? _formCategoria;

    [ObservableProperty]
    private int _formPrecio = 0;

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
    ListarCategorias listarCategorias)
    {
        _crearProducto = crearProducto;
        _actualizarProducto = actualizarProducto;
        _eliminarProducto = eliminarProducto;
        _reactivarProducto = reactivarProducto;
        _listarProductos = listarProductos;
        _listarCategorias = listarCategorias;

        foreach (var categoria in _listarCategorias.Ejecutar())
        {
            Categorias.Add(categoria);
        }

        CargarGrilla();
        LimpiarFormulario();
    }

    public void CargarGrilla()
    {
        _todosLosProductos = _listarProductos.Ejecutar(incluirEliminadas: true);
        Productos = new ObservableCollection<ProductoDTO>(_todosLosProductos);
    }

    private void LimpiarFormulario()
    {
        FormTitulo = "Nuevo Producto";
        FormCodigo = $"PROD-00{Productos.Count + 1}";
        FormNombre = string.Empty;
        FormCategoria = null;
        FormPrecio = 0;
        _productoEditandoId = null;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
    }

    [RelayCommand]
    private void EditarProducto(ProductoDTO? prod)
    {
        /*
        if (prod == null) return;
        FormTitulo = $"Editar {prod.Nombre}";
        FormCodigo = prod.Codigo;
        FormNombre = prod.Nombre;
        FormCategoria = prod.Categoria;
        FormPrecio = prod.Precio;
        FormDescripcion = prod.Descripcion;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
        */
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
        /*
        if (!string.IsNullOrWhiteSpace(FormNombre))
        {
            var p = new ProductoItem
            {
                Id = Productos.Count + 1,
                Codigo = FormCodigo,
                Nombre = FormNombre,
                Categoria = FormCategoria,
                Precio = FormPrecio,
                Descripcion = FormDescripcion,
                Activo = true,
                RecetaResumen = string.Join(", ", FormRecetaInsumos.Select(i => $"{i.InsumoNombre} ({i.Cantidad}{i.UnidadMedida})"))
            };
            Productos.Insert(0, p);
            SelectedProducto = p;
        }
        LimpiarFormulario();
        */
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        LimpiarFormulario();
    }

    [RelayCommand]
    private void ToggleEstadoProducto(ProductoDTO? prod)
    {
        /*
        if (prod != null)
        {
            prod.Activo = !prod.Activo;
            var index = Productos.IndexOf(prod);
            if (index >= 0)
            {
                Productos[index] = prod;
            }
        }
        */
    }
}
