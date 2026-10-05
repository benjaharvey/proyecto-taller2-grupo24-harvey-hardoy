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

public partial class SucursalViewModel : ObservableObject
{
    private readonly CrearSucursal _crearSucursal;
    private readonly ActualizarSucursal _actualizarSucursal;
    private readonly EliminarSucursal _eliminarSucursal;
    private readonly ReactivarSucursal _reactivarSucursal;
    private readonly ListarSucursales _listarSucursales;

    private List<SucursalDTO> _todasLasSucursales = new();

    [ObservableProperty]
    private ObservableCollection<SucursalDTO> _sucursalesFiltradas = new();

    [ObservableProperty]
    private SucursalDTO? _selectedSucursal;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // Campos del formulario
    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private string _formUbicacion = string.Empty;

    [ObservableProperty]
    private string _formEstado = "ACTIVA";

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private string _textoBotonEstado = "Dar de Baja";

    public SucursalViewModel(
        CrearSucursal crearSucursal,
        ActualizarSucursal actualizarSucursal,
        EliminarSucursal eliminarSucursal,
        ReactivarSucursal reactivarSucursal,
        ListarSucursales listarSucursales)
    {
        _crearSucursal = crearSucursal;
        _actualizarSucursal = actualizarSucursal;
        _eliminarSucursal = eliminarSucursal;
        _reactivarSucursal = reactivarSucursal;
        _listarSucursales = listarSucursales;

        CargarGrilla();
    }

    public void CargarGrilla()
    {
        _todasLasSucursales = _listarSucursales.Ejecutar(incluirEliminadas: true)
            .OrderBy(s => s.Estado != "ACTIVA")
            .ThenBy(s => s.Nombre)
            .ToList();

        AplicarFiltro();
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnSelectedSucursalChanged(SucursalDTO? value)
    {
        if (value != null)
        {
            EsEdicion = true;
            TextoBotonEstado = value.Estado == "ACTIVA" ? "Dar de Baja" : "Reactivar";

            FormNombre = value.Nombre;
            FormUbicacion = value.Ubicacion;
            FormEstado = value.Estado;
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
            SucursalesFiltradas = new ObservableCollection<SucursalDTO>(_todasLasSucursales);
        }
        else
        {
            var filtro = BusquedaTexto.Trim();
            var filtrados = _todasLasSucursales.Where(s =>
                (s.Nombre != null && s.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (s.Ubicacion != null && s.Ubicacion.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (s.Estado != null && s.Estado.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            )
            .OrderBy(s => s.Estado != "ACTIVA")
            .ThenBy(s => s.Nombre);

            SucursalesFiltradas = new ObservableCollection<SucursalDTO>(filtrados);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        var dto = new SucursalDTO
        {
            Id = EsEdicion && SelectedSucursal != null ? SelectedSucursal.Id : 0,
            Nombre = FormNombre,
            Ubicacion = FormUbicacion,
            Estado = FormEstado
        };

        try
        {
            if (EsEdicion)
            {
                _actualizarSucursal.Ejecutar(dto);
                MessageBox.Show("Sucursal actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _crearSucursal.Ejecutar(dto);
                MessageBox.Show("Sucursal creada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private void CambiarEstado()
    {
        if (SelectedSucursal == null) return;

        var sucursal = SelectedSucursal;
        bool esActiva = sucursal.Estado == "ACTIVA";

        string accion = esActiva ? "dar de baja" : "reactivar";
        var confirmacion = MessageBox.Show(
            $"¿Seguro que querés {accion} la sucursal \"{sucursal.Nombre}\"?",
            "Confirmar acción",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            if (esActiva)
            {
                _eliminarSucursal.Ejecutar(sucursal.Id);
                MessageBox.Show("Sucursal dada de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _reactivarSucursal.Ejecutar(sucursal.Id);
                MessageBox.Show("Sucursal reactivada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
        SelectedSucursal = null;
        EsEdicion = false;
        TextoBotonEstado = "Dar de Baja";

        FormNombre = string.Empty;
        FormUbicacion = string.Empty;
        FormEstado = "ACTIVA";
    }
}
