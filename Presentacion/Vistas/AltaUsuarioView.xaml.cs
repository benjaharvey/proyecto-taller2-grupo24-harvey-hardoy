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

        CargarComboRoles();
        CargarGrilla();
    }

    private void CargarComboRoles()
    {
        cmbRol.ItemsSource = null;
        cmbRol.ItemsSource = _listarRoles.Ejecutar().Where(r => r.Nombre != "Admin").ToList();
    }

    private void CargarGrilla()
    {
        dataGridUsuarios.ItemsSource = null;
        dataGridUsuarios.ItemsSource = _listarUsuarios.Ejecutar();
    }

    private void BtnActualizarLista_Click(object sender, RoutedEventArgs e)
    {
        CargarGrilla();
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
        txtSucursalId.Text = seleccionado.SucursalId.ToString();
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
        if (cmbRol.SelectedValue is int rolElegido)
        {
            rolId = rolElegido;
        }
        else if (esEdicion && dataGridUsuarios.SelectedItem is UsuarioDTO { NombreRol: "Admin" } admin)
        {
            rolId = admin.RolId;
        }
        else
        {
            MessageBox.Show("Seleccioná un rol.");
            return false;
        }

        if (!int.TryParse(txtSucursalId.Text, out var sucursalId))
        {
            MessageBox.Show("El ID de sucursal debe ser un número.");
            return false;
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
        txtSucursalId.Clear();
    }
}
