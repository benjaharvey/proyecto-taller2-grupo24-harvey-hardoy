using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

namespace Presentacion.Vistas;

public partial class AltaUsuarioView : UserControl
{
    public record SucursalOpcion(int Id, string Nombre);

    private readonly List<SucursalOpcion> _sucursales = new()
    {
        new SucursalOpcion(1, "Sucursal Centro"),
        new SucursalOpcion(2, "Sucursal Norte"),
        new SucursalOpcion(3, "Sucursal Sur")
    };

    private readonly CrearUsuario _crearUsuario;
    private readonly ActualizarUsuario _actualizarUsuario;
    private readonly EliminarUsuario _eliminarUsuario;
    private readonly ListarRoles _listarRoles;
    private readonly ListarUsuarios _listarUsuarios;

    public AltaUsuarioView()
    {
        InitializeComponent();

        _crearUsuario = App.Services.GetRequiredService<CrearUsuario>();
        _actualizarUsuario = App.Services.GetRequiredService<ActualizarUsuario>();
        _eliminarUsuario = App.Services.GetRequiredService<EliminarUsuario>();
        _listarRoles = App.Services.GetRequiredService<ListarRoles>();
        _listarUsuarios = App.Services.GetRequiredService<ListarUsuarios>();

        CargarCombos();
        CargarGrilla();
    }

    private void CargarCombos()
    {
        cmbRol.ItemsSource = null;
        cmbRol.ItemsSource = _listarRoles.Ejecutar().Where(r => r.Nombre != "Admin").ToList();

        cmbSucursal.ItemsSource = null;
        cmbSucursal.ItemsSource = _sucursales;
    }

    private void CargarGrilla()
    {
        dataGridUsuarios.ItemsSource = null;
        dataGridUsuarios.ItemsSource = _listarUsuarios.Ejecutar();
    }

    private void CmbRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ActualizarVisibilidadSucursal();
    }

    private void ActualizarVisibilidadSucursal()
    {
        string? rolNombre = null;
        if (cmbRol.SelectedItem is RolDTO rol)
        {
            rolNombre = rol.Nombre;
        }
        else if (dataGridUsuarios.SelectedItem is UsuarioDTO usuarioSel)
        {
            rolNombre = usuarioSel.NombreRol;
        }

        if (rolNombre == "Vendedor")
        {
            pnlSucursal.Visibility = Visibility.Visible;
        }
        else
        {
            pnlSucursal.Visibility = Visibility.Collapsed;
            cmbSucursal.SelectedIndex = -1;
        }
    }

    private void DataGridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        bool hayUsuarioSeleccionado = dataGridUsuarios.SelectedItem is UsuarioDTO;
        btnGuardarUsuario.IsEnabled = !hayUsuarioSeleccionado;
        btnActualizarUsuario.IsEnabled = hayUsuarioSeleccionado;
        btnEliminarUsuario.IsEnabled = hayUsuarioSeleccionado;

        if (dataGridUsuarios.SelectedItem is not UsuarioDTO seleccionado)
        {
            cmbRol.IsEnabled = true;
            return;
        }

        cmbRol.IsEnabled = seleccionado.NombreRol != "Admin";

        txtNombre.Text = seleccionado.Nombre;
        txtApellido.Text = seleccionado.Apellido;
        txtDni.Text = seleccionado.Dni;
        txtEmail.Text = seleccionado.Email;
        txtPassword.Clear();
        txtConfirmarPassword.Clear();
        dtpFechaNacimiento.SelectedDate = seleccionado.FechaNacimiento;
        txtDireccion.Text = seleccionado.Direccion;
        cmbRol.SelectedValue = seleccionado.RolId;

        ActualizarVisibilidadSucursal();
        if (seleccionado.NombreRol == "Vendedor")
        {
            cmbSucursal.SelectedValue = seleccionado.SucursalId;
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLeerFormulario(esEdicion: false, out var dto)) return;

        try
        {
            _crearUsuario.Ejecutar(dto);
            MessageBox.Show("Usuario registrado correctamente.");
            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void BtnActualizar_Click(object sender, RoutedEventArgs e)
    {
        if (dataGridUsuarios.SelectedItem is not UsuarioDTO seleccionado)
        {
            MessageBox.Show("Seleccioná un usuario de la lista para actualizar.");
            return;
        }

        if (!TryLeerFormulario(esEdicion: true, out var dto)) return;

        try
        {
            _actualizarUsuario.Ejecutar(seleccionado.Id, dto);
            MessageBox.Show("Usuario actualizado correctamente.");
            LimpiarFormulario();
            CargarGrilla();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (dataGridUsuarios.SelectedItem is not UsuarioDTO seleccionado)
        {
            MessageBox.Show("Seleccioná un usuario de la lista para eliminar.");
            return;
        }

        if (seleccionado.Id == SesionActual.UsuarioLogueado?.Id)
        {
            MessageBox.Show("No podés eliminar el usuario con el que iniciaste sesión.");
            return;
        }

        var confirmacion = MessageBox.Show(
            Window.GetWindow(this),
            $"¿Seguro que querés eliminar a {seleccionado.Nombre} {seleccionado.Apellido} ({seleccionado.Email})?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirmacion != MessageBoxResult.Yes) return;

        _eliminarUsuario.Ejecutar(seleccionado.Id);
        LimpiarFormulario();
        CargarGrilla();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        dataGridUsuarios.SelectedItem = null;
        LimpiarFormulario();
    }

    private bool TryLeerFormulario(bool esEdicion, out UsuarioCrearDTO dto)
    {
        dto = new UsuarioCrearDTO();

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("Ingresá el nombre.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtApellido.Text))
        {
            MessageBox.Show("Ingresá el apellido.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtDni.Text))
        {
            MessageBox.Show("Ingresá el DNI.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            MessageBox.Show("Ingresá el email.");
            return false;
        }

        if (!esEdicion && string.IsNullOrEmpty(txtPassword.Password))
        {
            MessageBox.Show("Ingresá una contraseña.");
            return false;
        }

        if (txtPassword.Password != txtConfirmarPassword.Password)
        {
            MessageBox.Show("Las contraseñas no coinciden.");
            return false;
        }

        if (dtpFechaNacimiento.SelectedDate is not DateTime fechaNacimiento)
        {
            MessageBox.Show("Seleccioná una fecha de nacimiento.");
            return false;
        }

        int rolId;
        string? rolNombre = null;
        if (cmbRol.SelectedItem is RolDTO rolElegido)
        {
            rolId = rolElegido.Id;
            rolNombre = rolElegido.Nombre;
        }
        else if (esEdicion && dataGridUsuarios.SelectedItem is UsuarioDTO { NombreRol: "Admin" } admin)
        {
            rolId = admin.RolId;
            rolNombre = admin.NombreRol;
        }
        else
        {
            MessageBox.Show("Seleccioná un rol.");
            return false;
        }

        int sucursalId = 1;
        if (rolNombre == "Vendedor")
        {
            if (cmbSucursal.SelectedValue is int sucursalElegida)
            {
                sucursalId = sucursalElegida;
            }
            else
            {
                MessageBox.Show("Seleccioná una sucursal para el vendedor.");
                return false;
            }
        }
        else if (rolNombre == "Cocinero")
        {
            sucursalId = 1; // Fábrica/Cocina principal
        }

        dto = new UsuarioCrearDTO
        {
            Nombre = txtNombre.Text,
            Apellido = txtApellido.Text,
            Dni = txtDni.Text,
            Email = txtEmail.Text,
            Password = txtPassword.Password,
            FechaNacimiento = fechaNacimiento,
            Direccion = txtDireccion.Text,
            RolId = rolId,
            SucursalId = sucursalId
        };
        return true;
    }

    private void LimpiarFormulario()
    {
        txtNombre.Clear();
        txtApellido.Clear();
        txtDni.Clear();
        txtEmail.Clear();
        txtPassword.Clear();
        txtConfirmarPassword.Clear();
        dtpFechaNacimiento.SelectedDate = null;
        txtDireccion.Clear();
        cmbRol.SelectedIndex = -1;
        cmbSucursal.SelectedIndex = -1;
        pnlSucursal.Visibility = Visibility.Collapsed;
    }
}
