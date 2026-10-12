using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Aplicacion.CasosDeUso;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentacion.ViewModels;

public partial class ClientesViewModel : ObservableObject{

    private readonly CrearCliente _crearCliente;
    private readonly ActualizarCliente _actualizarCliente;
    private readonly EliminarCliente _eliminarCliente;
    private readonly ReactivarCliente _reactivarCliente;
    private readonly ListarClientes _listarClientes;

    private readonly ObservableCollection<ClienteItem> _todosLosClientes = new();

    [ObservableProperty]
    private ObservableCollection<ClienteItem> _clientesFiltrados = new();

    [ObservableProperty]
    private ClienteItem? _selectedCliente;

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

    partial void OnBusquedaTextoChanged(string value)
    {
        ActualizarFiltro();
    }

    partial void OnSelectedClienteChanged(ClienteItem? value)
    {
        if (value != null)
        {
            EsEdicion = true;
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

    private void ActualizarFiltro()
    {
        if (string.IsNullOrWhiteSpace(BusquedaTexto))
        {
            ClientesFiltrados = new ObservableCollection<ClienteItem>(_todosLosClientes);
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
            ClientesFiltrados = new ObservableCollection<ClienteItem>(filtrados);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        if (string.IsNullOrWhiteSpace(FormNombre) || string.IsNullOrWhiteSpace(FormApellido) || string.IsNullOrWhiteSpace(FormDni))
        {
            MessageBox.Show("Nombre, Apellido y DNI son campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_todosLosClientes.Any(c => c.Dni.Equals(FormDni.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Ya existe un cliente registrado con el DNI {FormDni.Trim()}.", "DNI Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(FormEmail) && !FormEmail.Contains("@"))
        {
            MessageBox.Show("El correo electrónico no tiene un formato válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string usuarioActual = SesionActual.UsuarioLogueado != null 
            ? $"{SesionActual.UsuarioLogueado.Nombre} {SesionActual.UsuarioLogueado.Apellido}" 
            : "Vendedor Mostrador";

        var nuevo = new ClienteItem
        {
            Id = _todosLosClientes.Count > 0 ? _todosLosClientes.Max(c => c.Id) + 1 : 1,
            Nombre = FormNombre.Trim(),
            Apellido = FormApellido.Trim(),
            Dni = FormDni.Trim(),
            Email = FormEmail?.Trim() ?? string.Empty,
            FechaNacimiento = FormFechaNacimiento,
            RegistradoPor = usuarioActual,
            FechaRegistro = DateTime.Now
        };

        _todosLosClientes.Add(nuevo);
        ActualizarFiltro();
        SelectedCliente = nuevo;
        MessageBox.Show("Cliente guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        LimpiarFormulario();
    }

    [RelayCommand]
    private void Actualizar()
    {
        if (SelectedCliente == null) return;

        if (string.IsNullOrWhiteSpace(FormNombre) || string.IsNullOrWhiteSpace(FormApellido) || string.IsNullOrWhiteSpace(FormDni))
        {
            MessageBox.Show("Nombre, Apellido y DNI son campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_todosLosClientes.Any(c => c.Id != SelectedCliente.Id && c.Dni.Equals(FormDni.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Ya existe otro cliente registrado con el DNI {FormDni.Trim()}.", "DNI Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(FormEmail) && !FormEmail.Contains("@"))
        {
            MessageBox.Show("El correo electrónico no tiene un formato válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedCliente.Nombre = FormNombre.Trim();
        SelectedCliente.Apellido = FormApellido.Trim();
        SelectedCliente.Dni = FormDni.Trim();
        SelectedCliente.Email = FormEmail?.Trim() ?? string.Empty;
        SelectedCliente.FechaNacimiento = FormFechaNacimiento;

        ActualizarFiltro();
        MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        LimpiarFormulario();
    }

    [RelayCommand]
    private void Eliminar()
    {
        if (SelectedCliente == null) return;

        var result = MessageBox.Show($"¿Estás seguro de que deseas eliminar al cliente {SelectedCliente.NombreCompleto}?",
            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _todosLosClientes.Remove(SelectedCliente);
            ActualizarFiltro();
            LimpiarFormulario();
            MessageBox.Show("Cliente eliminado con éxito.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
