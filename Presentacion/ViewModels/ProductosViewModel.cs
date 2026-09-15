using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public class ProductoItem
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public string RecetaResumen { get; set; } = string.Empty;
}

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
    [ObservableProperty]
    private ObservableCollection<ProductoItem> _productos = new();

    [ObservableProperty]
    private ProductoItem? _selectedProducto;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    [ObservableProperty]
    private string _filtroCategoria = "Todas";

    [ObservableProperty]
    private string _formTitulo = "Nuevo Producto";

    // Form fields
    [ObservableProperty]
    private string _formCodigo = string.Empty;

    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private string _formCategoria = "Alfajores";

    [ObservableProperty]
    private decimal _formPrecio = 0;

    [ObservableProperty]
    private string _formDescripcion = string.Empty;

    [ObservableProperty]
    private ObservableCollection<InsumoRecetaItem> _formRecetaInsumos = new();

    // Dialogo agregar insumo a receta
    [ObservableProperty]
    private bool _mostrarModalAgregarInsumo;

    [ObservableProperty]
    private InsumoOpcionItem? _insumoSeleccionadoParaReceta;

    [ObservableProperty]
    private decimal _nuevoInsumoCantidad = 50;

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Todas", "Alfajores", "Conitos", "Tabletas", "Tortas", "Especiales"
    };

    public ObservableCollection<InsumoOpcionItem> InsumosDisponibles { get; } = new();

    public ProductosViewModel()
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        FormTitulo = "Nuevo Producto";
        FormCodigo = $"PROD-00{Productos.Count + 1}";
        FormNombre = string.Empty;
        FormCategoria = "Alfajores";
        FormPrecio = 0;
        FormDescripcion = string.Empty;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
    }

    [RelayCommand]
    private void EditarProducto(ProductoItem? prod)
    {
        if (prod == null) return;
        FormTitulo = $"Editar {prod.Nombre}";
        FormCodigo = prod.Codigo;
        FormNombre = prod.Nombre;
        FormCategoria = prod.Categoria;
        FormPrecio = prod.Precio;
        FormDescripcion = prod.Descripcion;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>();
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
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        LimpiarFormulario();
    }

    [RelayCommand]
    private void ToggleEstadoProducto(ProductoItem? prod)
    {
        if (prod != null)
        {
            prod.Activo = !prod.Activo;
            var index = Productos.IndexOf(prod);
            if (index >= 0)
            {
                Productos[index] = prod;
            }
        }
    }
}
