using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.ViewModels;

public partial class ClientesViewModel : ObservableObject{

    private readonly CrearCliente _crearCliente;
    private readonly ActualizarCliente _actualizarCliente;
    private readonly EliminarCliente _eliminarCliente;
    private readonly ReactivarCliente _reactivarCliente;
    private readonly ListarClientes _listarClientes;

    private List<ClienteDTO> _todosLosClientes = new();

    [ObservableProperty]
    private ObservableCollection<ClienteDTO> _clientesFiltrados = new();

    [ObservableProperty]
    private ClienteDTO? _selectedCliente;

    [ObservableProperty]
    private string _busquedaTexto = string.Empty;

    // Form fields
    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private string _formApellido = string.Empty;

    [ObservableProperty]
    private string _formDni = string.Empty;

    [ObservableProperty]
    private string _formEmail = string.Empty;

    [ObservableProperty]
    private DateTime? _formFechaNacimiento;

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private string _textoBotonEstado = "Dar de Baja";

    public ClientesViewModel(CrearCliente crearCliente, ActualizarCliente actualizarCliente, EliminarCliente eliminarCliente, 
        ReactivarCliente reactivarCliente, ListarClientes listarClientes)
    {
        _crearCliente = crearCliente;
        _actualizarCliente = actualizarCliente;
        _eliminarCliente = eliminarCliente;
        _reactivarCliente = reactivarCliente;
        _listarClientes = listarClientes;

        CargarGrilla();
        LimpiarFormulario();
    }

    public void CargarGrilla()
    {
        _todosLosClientes = _listarClientes.Ejecutar(incluirEliminados: true)
            .OrderBy(p => !p.Activo)
            .ThenBy(p => p.Nombre)
            .ToList();
        AplicarFiltro();
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnSelectedClienteChanged(ClienteDTO? value)
    {
        if (value != null)
        {
            EsEdicion = true;
            TextoBotonEstado = value.Activo ? "Dar de Baja" : "Reactivar";

            FormNombre = value.Nombre;
            FormApellido = value.Apellido;
            FormDni = value.Dni;
            FormEmail = value.Email;
            FormFechaNacimiento = value.FechaNacimiento;
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
            ClientesFiltrados = new ObservableCollection<ClienteDTO>(_todosLosClientes);
        }
        else
        {
            var texto = BusquedaTexto.Trim().ToLowerInvariant();
            var filtrados = _todosLosClientes.Where(c =>
                c.Dni.ToLowerInvariant().Contains(texto) ||
                c.Apellido.ToLowerInvariant().Contains(texto) ||
                c.Nombre.ToLowerInvariant().Contains(texto) ||
                c.Email.ToLowerInvariant().Contains(texto)
            );
            ClientesFiltrados = new ObservableCollection<ClienteDTO>(filtrados);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        var nuevo = new ClienteDTO
        {
            Nombre = FormNombre,
            Apellido = FormApellido,
            Dni = FormDni,
            Email = FormEmail,
            FechaNacimiento = FormFechaNacimiento ?? default,
            UsuarioId = SesionActual.UsuarioLogueado?.Id ?? 0
        };

        try
        {
            _crearCliente.Ejecutar(nuevo);
            MessageBox.Show("Cliente guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private void Actualizar()
    {
        if (SelectedCliente == null) return;
        
        var dto = new ClienteDTO
        {
            Nombre = FormNombre,
            Apellido = FormApellido,
            Dni = FormDni,
            Email = FormEmail,
            FechaNacimiento = FormFechaNacimiento ?? default
        };

        try
        {
            _actualizarCliente.Ejecutar(SelectedCliente.Id, dto);
            MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private void Eliminar()
    {
        if (SelectedCliente == null) return;
        var cliente = SelectedCliente;
        bool esActivo = cliente.Activo;

        var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar al cliente {SelectedCliente.NombreCompleto}?",
            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if(result == MessageBoxResult.No)
        {
            return;
        } else
        {
            try
            {
                if(esActivo == true)
                {
                    _eliminarCliente.Ejecutar(cliente.Id);
                }
                else
                {
                    _reactivarCliente.Ejecutar(cliente.Id);
                }
                CargarGrilla();
                LimpiarFormulario();
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

    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        SelectedCliente = null;
        EsEdicion = false;
        FormNombre = string.Empty;
        FormApellido = string.Empty;
        FormDni = string.Empty;
        FormEmail = string.Empty;
        FormFechaNacimiento = null;
        TextoBotonEstado = "Dar de Baja";
    }
}
