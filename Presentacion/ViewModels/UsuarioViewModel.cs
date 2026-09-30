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

public record SucursalOpcion(int Id, string Nombre);

public partial class UsuarioViewModel : ObservableObject
{
    private readonly CrearUsuario _crearUsuario;
    private readonly ActualizarUsuario _actualizarUsuario;
    private readonly EliminarUsuario _eliminarUsuario;
    private readonly ReactivarUsuario _reactivarUsuario;
    private readonly ListarRoles _listarRoles;
    private readonly ListarUsuarios _listarUsuarios;

    private List<UsuarioDTO> _todosLosUsuarios = new();

    [ObservableProperty]
    private ObservableCollection<UsuarioDTO> _usuariosFiltrados = new();

    [ObservableProperty]
    private ObservableCollection<RolDTO> _rolesDisponibles = new();

    [ObservableProperty]
    private ObservableCollection<SucursalOpcion> _sucursalesDisponibles = new();

    [ObservableProperty]
    private UsuarioDTO? _selectedUsuario;

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
    private string _formPassword = string.Empty;

    [ObservableProperty]
    private string _formConfirmarPassword = string.Empty;

    [ObservableProperty]
    private DateTime? _formFechaNacimiento;

    [ObservableProperty]
    private string _formDireccion = string.Empty;

    [ObservableProperty]
    private RolDTO? _formRolSeleccionado;

    [ObservableProperty]
    private SucursalOpcion? _formSucursalSeleccionada;

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private bool _mostrarSucursal;

    [ObservableProperty]
    private bool _puedeEditarRol = true;

    [ObservableProperty]
    private string _textoBotonEstado = "Dar de Baja";

    public UsuarioViewModel(
        CrearUsuario crearUsuario,
        ActualizarUsuario actualizarUsuario,
        EliminarUsuario eliminarUsuario,
        ReactivarUsuario reactivarUsuario,
        ListarRoles listarRoles,
        ListarUsuarios listarUsuarios)
    {
        _crearUsuario = crearUsuario;
        _actualizarUsuario = actualizarUsuario;
        _eliminarUsuario = eliminarUsuario;
        _reactivarUsuario = reactivarUsuario;
        _listarRoles = listarRoles;
        _listarUsuarios = listarUsuarios;

        CargarCombos();
        CargarGrilla();
    }

    public void CargarCombos()
    {
        var roles = _listarRoles.Ejecutar().Where(r => r.Nombre != "Admin").ToList();
        RolesDisponibles = new ObservableCollection<RolDTO>(roles);

        var sucursales = new List<SucursalOpcion>
        {
            new(1, "Sucursal Centro"),
            new(2, "Sucursal Norte"),
            new(3, "Sucursal Sur")
        };
        SucursalesDisponibles = new ObservableCollection<SucursalOpcion>(sucursales);
    }

    public void CargarGrilla()
    {
        _todosLosUsuarios = _listarUsuarios.Ejecutar()
            .OrderBy(u => !u.Activo)
            .ThenBy(u => u.Nombre)
            .ToList();

        AplicarFiltro();
    }

    partial void OnBusquedaTextoChanged(string value)
    {
        AplicarFiltro();
    }

    partial void OnFormRolSeleccionadoChanged(RolDTO? value)
    {
        ActualizarVisibilidadSucursal();
    }

    partial void OnSelectedUsuarioChanged(UsuarioDTO? value)
    {
        if (value != null)
        {
            EsEdicion = true;
            TextoBotonEstado = value.Activo ? "Dar de Baja" : "Reactivar";
            PuedeEditarRol = value.NombreRol != "Admin";

            FormNombre = value.Nombre;
            FormApellido = value.Apellido;
            FormDni = value.Dni;
            FormEmail = value.Email;
            FormPassword = string.Empty;
            FormConfirmarPassword = string.Empty;
            FormFechaNacimiento = value.FechaNacimiento;
            FormDireccion = value.Direccion;

            FormRolSeleccionado = RolesDisponibles.FirstOrDefault(r => r.Id == value.RolId);

            ActualizarVisibilidadSucursal();
            if (value.NombreRol == "Vendedor")
            {
                FormSucursalSeleccionada = SucursalesDisponibles.FirstOrDefault(s => s.Id == value.SucursalId);
            }
        }
        else
        {
            LimpiarFormulario();
        }
    }

    private void ActualizarVisibilidadSucursal()
    {
        string? rolNombre = FormRolSeleccionado?.Nombre ?? SelectedUsuario?.NombreRol;
        MostrarSucursal = rolNombre == "Vendedor";
        if (!MostrarSucursal)
        {
            FormSucursalSeleccionada = null;
        }
    }

    private void AplicarFiltro()
    {
        if (string.IsNullOrWhiteSpace(BusquedaTexto))
        {
            UsuariosFiltrados = new ObservableCollection<UsuarioDTO>(_todosLosUsuarios);
        }
        else
        {
            var filtro = BusquedaTexto.Trim();
            var filtrados = _todosLosUsuarios.Where(u =>
                (u.Nombre != null && u.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.Apellido != null && u.Apellido.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.Dni != null && u.Dni.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.Email != null && u.Email.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.NombreRol != null && u.NombreRol.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.NombreSucursal != null && u.NombreSucursal.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.Direccion != null && u.Direccion.Contains(filtro, StringComparison.OrdinalIgnoreCase)) ||
                (u.Activo ? "activo" : "inactivo").Contains(filtro, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(u => !u.Activo)
            .ThenBy(u => u.Nombre);

            UsuariosFiltrados = new ObservableCollection<UsuarioDTO>(filtrados);
        }
    }

    [RelayCommand]
    private void LimpiarBusqueda()
    {
        BusquedaTexto = string.Empty;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (!ValidarFormulario(esEdicion: false, out var dto)) return;

        try
        {
            _crearUsuario.Ejecutar(dto);
            MessageBox.Show("Usuario registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void Actualizar()
    {
        if (SelectedUsuario == null)
        {
            MessageBox.Show("Seleccioná un usuario de la lista para actualizar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!ValidarFormulario(esEdicion: true, out var dto)) return;

        try
        {
            _actualizarUsuario.Ejecutar(SelectedUsuario.Id, dto);
            MessageBox.Show("Usuario actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void CambiarEstado()
    {
        if (SelectedUsuario == null)
        {
            MessageBox.Show("Seleccioná un usuario de la lista para cambiar su estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var usuario = SelectedUsuario;

        if (usuario.Activo)
        {
            if (usuario.Id == SesionActual.UsuarioLogueado?.Id)
            {
                MessageBox.Show("No podés dar de baja el usuario con el que iniciaste sesión.", "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés dar de baja a {usuario.Nombre} {usuario.Apellido} ({usuario.Email})?",
                "Confirmar baja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                _eliminarUsuario.Ejecutar(usuario.Id);
                MessageBox.Show("Usuario dado de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés reactivar a {usuario.Nombre} {usuario.Apellido} ({usuario.Email})?",
                "Confirmar reactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                _reactivarUsuario.Ejecutar(usuario.Id);
                MessageBox.Show("Usuario reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        LimpiarFormulario();
        CargarGrilla();
    }

    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
    }

    private bool ValidarFormulario(bool esEdicion, out UsuarioCrearDTO dto)
    {
        dto = new UsuarioCrearDTO();

        if (string.IsNullOrWhiteSpace(FormNombre))
        {
            MessageBox.Show("Ingresá el nombre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormApellido))
        {
            MessageBox.Show("Ingresá el apellido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormDni))
        {
            MessageBox.Show("Ingresá el DNI.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormEmail))
        {
            MessageBox.Show("Ingresá el email.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!esEdicion && string.IsNullOrEmpty(FormPassword))
        {
            MessageBox.Show("Ingresá una contraseña.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormPassword != FormConfirmarPassword)
        {
            MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (FormFechaNacimiento is not DateTime fechaNacimiento)
        {
            MessageBox.Show("Seleccioná una fecha de nacimiento.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        int rolId;
        string? rolNombre = null;
        if (FormRolSeleccionado != null)
        {
            rolId = FormRolSeleccionado.Id;
            rolNombre = FormRolSeleccionado.Nombre;
        }
        else if (esEdicion && SelectedUsuario is { NombreRol: "Admin" } admin)
        {
            rolId = admin.RolId;
            rolNombre = admin.NombreRol;
        }
        else
        {
            MessageBox.Show("Seleccioná un rol.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        int sucursalId = 1;
        if (rolNombre == "Vendedor")
        {
            if (FormSucursalSeleccionada != null)
            {
                sucursalId = FormSucursalSeleccionada.Id;
            }
            else
            {
                MessageBox.Show("Seleccioná una sucursal para el vendedor.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
        else if (rolNombre == "Cocinero")
        {
            sucursalId = 1;
        }

        dto = new UsuarioCrearDTO
        {
            Nombre = FormNombre.Trim(),
            Apellido = FormApellido.Trim(),
            Dni = FormDni.Trim(),
            Email = FormEmail.Trim(),
            Password = FormPassword,
            FechaNacimiento = fechaNacimiento,
            Direccion = FormDireccion.Trim(),
            RolId = rolId,
            SucursalId = sucursalId
        };

        return true;
    }

    private void LimpiarFormulario()
    {
        SelectedUsuario = null;
        EsEdicion = false;
        PuedeEditarRol = true;
        TextoBotonEstado = "Dar de Baja";

        FormNombre = string.Empty;
        FormApellido = string.Empty;
        FormDni = string.Empty;
        FormEmail = string.Empty;
        FormPassword = string.Empty;
        FormConfirmarPassword = string.Empty;
        FormFechaNacimiento = null;
        FormDireccion = string.Empty;
        FormRolSeleccionado = null;
        FormSucursalSeleccionada = null;
        MostrarSucursal = false;
    }
}
