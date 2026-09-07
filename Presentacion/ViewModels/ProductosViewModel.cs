using System.Collections.ObjectModel;
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

public class InsumoRecetaItem
{
    public string InsumoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
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
    private bool _mostrarFormulario;

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

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Todas", "Alfajores", "Conitos", "Tabletas", "Tortas", "Especiales"
    };

    public ProductosViewModel()
    {
        CargarDatosMock();
    }

    private void CargarDatosMock()
    {
        Productos = new ObservableCollection<ProductoItem>
        {
            new() { Id = 1, Codigo = "ALF-001", Nombre = "Alfajor Clásico Dulce de Leche", Categoria = "Alfajores", Precio = 1800.00m, Descripcion = "Masa de cacao artesanal con dulce de leche repostero", Activo = true, RecetaResumen = "Harina (50g), DDL (60g), Chocolate (30g)" },
            new() { Id = 2, Codigo = "ALF-002", Nombre = "Alfajor de Nuez y Chocolate Blanco", Categoria = "Alfajores", Precio = 2100.00m, Descripcion = "Tapas de nuez con cobertura de chocolate blanco", Activo = true, RecetaResumen = "Nuez (40g), DDL (55g), Choc. Blanco (35g)" },
            new() { Id = 3, Codigo = "CON-001", Nombre = "Conito de Dulce de Leche", Categoria = "Conitos", Precio = 1600.00m, Descripcion = "Base de galleta artesanal y corazón de dulce de leche", Activo = true, RecetaResumen = "Galleta base (1u), DDL (80g), Choc. Negro (25g)" },
            new() { Id = 4, Codigo = "TAB-001", Nombre = "Tableta Marroc Artesanal", Categoria = "Tabletas", Precio = 2400.00m, Descripcion = "Praliné de maní y capas de chocolate con leche", Activo = true, RecetaResumen = "Pasta de Maní (70g), Choc. Leche (60g)" },
            new() { Id = 5, Codigo = "TOR-001", Nombre = "Mini Torta Rogel", Categoria = "Tortas", Precio = 3500.00m, Descripcion = "Capas crocantes de masa hojaldrada y merengue italiano", Activo = true, RecetaResumen = "Masa Hojaldre (4u), DDL (120g), Merengue (50g)" },
            new() { Id = 6, Codigo = "ALF-003", Nombre = "Alfajor Maicena Tradicional", Categoria = "Alfajores", Precio = 1500.00m, Descripcion = "Masa suave con coco rallado y dulce de leche", Activo = false, RecetaResumen = "Almidón maíz (60g), DDL (50g), Coco (15g)" }
        };

        if (Productos.Count > 0)
        {
            SelectedProducto = Productos[0];
        }
    }

    [RelayCommand]
    private void NuevoProducto()
    {
        FormTitulo = "Nuevo Producto";
        FormCodigo = $"PROD-00{Productos.Count + 1}";
        FormNombre = string.Empty;
        FormCategoria = "Alfajores";
        FormPrecio = 0;
        FormDescripcion = string.Empty;
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>
        {
            new() { InsumoNombre = "Dulce de Leche Repostero", Cantidad = 60, UnidadMedida = "gr" },
            new() { InsumoNombre = "Harina 0000", Cantidad = 50, UnidadMedida = "gr" }
        };
        MostrarFormulario = true;
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
        FormRecetaInsumos = new ObservableCollection<InsumoRecetaItem>
        {
            new() { InsumoNombre = "Dulce de Leche Repostero", Cantidad = 60, UnidadMedida = "gr" },
            new() { InsumoNombre = "Chocolate Cobertura Semiamargo", Cantidad = 35, UnidadMedida = "gr" }
        };
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void GuardarFormulario()
    {
        MostrarFormulario = false;
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
    }

    [RelayCommand]
    private void ToggleEstadoProducto(ProductoItem? prod)
    {
        if (prod != null)
        {
            prod.Activo = !prod.Activo;
            // Trigger refresh
            var index = Productos.IndexOf(prod);
            if (index >= 0)
            {
                Productos[index] = prod;
            }
        }
    }
}
