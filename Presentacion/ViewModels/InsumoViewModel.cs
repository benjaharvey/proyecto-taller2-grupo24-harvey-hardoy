using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

namespace Presentacion.ViewModels;

public partial class InsumoViewModel : ObservableObject
{
    private readonly CrearInsumo _crearInsumo;
    private readonly ActualizarInsumo _actualizarInsumo;
    private readonly EliminarInsumo _eliminarInsumo;
    private readonly ReactivarInsumo _reactivarInsumo;
    private readonly ListarInsumos _listarInsumos;

    private List<InsumoDTO> _todosLosInsumos = new();

    [ObservableProperty]
    private ObservableCollection<InsumoDTO> _insumosFiltrados = new();

    [ObservableProperty]
    private InsumoDTO? _selectedInsumo;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // Campos del formulario
    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private string _formUnidad = "kg";

    [ObservableProperty]
    private decimal _formStock = 0;

    [ObservableProperty]
    private decimal _formStockMinimo = 5;

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private string _textoBotonEstado = "Dar de Baja";

    // Modales / Diálogos
    [ObservableProperty]
    private bool _mostrarDialogoNuevoInsumo;

    [ObservableProperty]
    private bool _mostrarDialogoIngresoInsumo;

    [ObservableProperty]
    private InsumoDTO? _insumoSeleccionadoIngreso;

    [ObservableProperty]
    private decimal _cantidadIngresoInsumo = 10;

    public ObservableCollection<string> UnidadesSugeridas { get; } = new()
    {
        "kg", "g", "l", "ml", "u"
    };

    public InsumoViewModel(
        CrearInsumo crearInsumo,
        ActualizarInsumo actualizarInsumo,
        EliminarInsumo eliminarInsumo,
        ReactivarInsumo reactivarInsumo,
        ListarInsumos listarInsumos)
    {
        _crearInsumo = crearInsumo;
        _actualizarInsumo = actualizarInsumo;
        _eliminarInsumo = eliminarInsumo;
        _reactivarInsumo = reactivarInsumo;
        _listarInsumos = listarInsumos;

        CargarGrilla();
    }

    public void CargarGrilla()
    {
        _todosLosInsumos = _listarInsumos.Ejecutar(incluirEliminados: true)
            .OrderBy(i => !i.Activo)
            .ThenBy(i => i.Nombre)
            .ToList();

        AplicarFiltro();
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnSelectedInsumoChanged(InsumoDTO? value)
    {
        if (value != null)
        {
            EsEdicion = true;
            TextoBotonEstado = value.Activo ? "Dar de Baja" : "Reactivar";

            FormNombre = value.Nombre;
            FormUnidad = value.Unidad;
            FormStock = value.Stock;
            FormStockMinimo = value.StockMinimo;
        }
        else
        {
            LimpiarFormulario();
        }
    }

    private void AplicarFiltro()
    {
        if (string.IsNullOrWhiteSpace(BusquedaTexto))
        {
            InsumosFiltrados = new ObservableCollection<InsumoDTO>(_todosLosInsumos);
        }
        else
        {
            var filtro = BusquedaTexto.Trim();
            var filtrados = _todosLosInsumos.Where(i =>
                (i.Nombre != null && i.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (i.Unidad != null && i.Unidad.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            )
            .OrderBy(i => !i.Activo)
            .ThenBy(i => i.Nombre);

            InsumosFiltrados = new ObservableCollection<InsumoDTO>(filtrados);
        }
    }

    [RelayCommand]
    private void AbrirNuevoInsumo()
    {
        LimpiarFormulario();
        MostrarDialogoNuevoInsumo = true;
    }

    [RelayCommand]
    private void CerrarNuevoInsumo()
    {
        MostrarDialogoNuevoInsumo = false;
    }

    [RelayCommand]
    private void GuardarNuevoInsumo()
    {
        var dto = new InsumoDTO
        {
            Id = 0,
            Nombre = FormNombre,
            Unidad = FormUnidad,
            Stock = FormStock,
            StockMinimo = FormStockMinimo
        };

        try
        {
            _crearInsumo.Ejecutar(dto);
            MessageBox.Show("Insumo creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            MostrarDialogoNuevoInsumo = false;
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
    private void AbrirIngresoInsumo(InsumoDTO? insumo)
    {
        InsumoSeleccionadoIngreso = insumo ?? InsumosFiltrados.FirstOrDefault(i => i.Activo);
        CantidadIngresoInsumo = 10;
        MostrarDialogoIngresoInsumo = true;
    }

    [RelayCommand]
    private void CerrarIngresoInsumo()
    {
        MostrarDialogoIngresoInsumo = false;
    }

    [RelayCommand]
    private void GuardarIngresoInsumo()
    {
        if (InsumoSeleccionadoIngreso == null)
        {
            MessageBox.Show("Seleccioná un insumo para el ingreso.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CantidadIngresoInsumo <= 0)
        {
            MessageBox.Show("La cantidad a ingresar debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var insumo = InsumoSeleccionadoIngreso;
            insumo.Stock += CantidadIngresoInsumo;
            _actualizarInsumo.Ejecutar(insumo);
            MessageBox.Show($"Se incrementó el stock de \"{insumo.Nombre}\" a {insumo.Stock:N3} {insumo.Unidad}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            MostrarDialogoIngresoInsumo = false;
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al registrar el ingreso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        var dto = new InsumoDTO
        {
            Id = EsEdicion && SelectedInsumo != null ? SelectedInsumo.Id : 0,
            Nombre = FormNombre,
            Unidad = FormUnidad,
            Stock = FormStock,
            StockMinimo = FormStockMinimo
        };

        try
        {
            if (EsEdicion)
            {
                _actualizarInsumo.Ejecutar(dto);
                MessageBox.Show("Insumo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _crearInsumo.Ejecutar(dto);
                MessageBox.Show("Insumo creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private void CambiarEstado(InsumoDTO? insumoParam)
    {
        var insumo = insumoParam ?? SelectedInsumo;
        if (insumo == null) return;

        bool esActivo = insumo.Activo;
        string accion = esActivo ? "dar de baja" : "reactivar";
        var confirmacion = MessageBox.Show(
            $"¿Seguro que querés {accion} el insumo \"{insumo.Nombre}\"?",
            "Confirmar acción",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            if (esActivo)
            {
                _eliminarInsumo.Ejecutar(insumo.Id);
                MessageBox.Show("Insumo dado de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _reactivarInsumo.Ejecutar(insumo.Id);
                MessageBox.Show("Insumo reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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

    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
    }

    [RelayCommand]
    private void LimpiarBusqueda()
    {
        BusquedaTexto = string.Empty;
    }

    private void LimpiarFormulario()
    {
        SelectedInsumo = null;
        EsEdicion = false;
        TextoBotonEstado = "Dar de Baja";

        FormNombre = string.Empty;
        FormUnidad = "kg";
        FormStock = 0;
        FormStockMinimo = 5;
    }
}
