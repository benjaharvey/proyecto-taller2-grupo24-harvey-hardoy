using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Presentacion.ViewModels;

namespace Presentacion.ViewModels;

public partial class ClientesViewModel : ObservableObject, ISeccionRecargable{

    private static readonly Regex RegexNombre = new(@"^\p{L}+([ '\-]\p{L}+)*$", RegexOptions.Compiled);
    private static readonly Regex RegexDni = new(@"^\d{7,8}$", RegexOptions.Compiled);

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

    public void Recargar()
    {
        CargarGrilla();
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
                (c.Dni?.ToLowerInvariant().Contains(texto) ?? false) ||
                (c.Apellido?.ToLowerInvariant().Contains(texto) ?? false) ||
                (c.Nombre?.ToLowerInvariant().Contains(texto) ?? false) ||
                (c.Email?.ToLowerInvariant().Contains(texto) ?? false)
            );
            ClientesFiltrados = new ObservableCollection<ClienteDTO>(filtrados);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        if (!ValidarFormulario(out var nuevo)) return;

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
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleccioná un cliente de la lista para actualizar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!ValidarFormulario(out var dto)) return;

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
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleccioná un cliente de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
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

    private bool ValidarFormulario(out ClienteDTO dto)
    {
        dto = new ClienteDTO();

        string nombre = FormNombre.Trim();
        string apellido = FormApellido.Trim();
        string dni = FormDni.Trim();
        string email = FormEmail.Trim();

        if (!ValidarNombre(nombre, "nombre")) return false;
        if (!ValidarNombre(apellido, "apellido")) return false;

        if (!RegexDni.IsMatch(dni))
        {
            MessageBox.Show("El DNI debe tener 7 u 8 dígitos, sin puntos ni espacios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (email.Length == 0)
        {
            MessageBox.Show("Ingresá el email.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (email.Length > 100)
        {
            MessageBox.Show("El email no puede superar los 100 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!MailAddress.TryCreate(email, out var direccion)
            || direccion.Address != email
            || !direccion.Host.Contains('.')
            || direccion.Host.EndsWith('.'))
        {
            MessageBox.Show("El email no tiene un formato válido (ej: nombre@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormFechaNacimiento is not DateTime fechaNacimiento)
        {
            MessageBox.Show("Seleccioná una fecha de nacimiento.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (fechaNacimiento.Date > DateTime.Today)
        {
            MessageBox.Show("La fecha de nacimiento no puede ser futura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        int edad = DateTime.Today.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;

        if (edad > 100)
        {
            MessageBox.Show("La fecha de nacimiento no es válida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        dto = new ClienteDTO
        {
            Nombre = nombre,
            Apellido = apellido,
            Dni = dni,
            Email = email,
            FechaNacimiento = fechaNacimiento,
            UsuarioId = SesionActual.UsuarioLogueado?.Id ?? 0
        };

        return true;
    }

    private static bool ValidarNombre(string valor, string campo)
    {
        if (valor.Length == 0)
        {
            MessageBox.Show($"Ingresá el {campo}.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (valor.Length < 2 || valor.Length > 50)
        {
            MessageBox.Show($"El {campo} debe tener entre 2 y 50 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!RegexNombre.IsMatch(valor))
        {
            MessageBox.Show($"El {campo} solo puede contener letras, espacios, guiones y apóstrofes.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
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
