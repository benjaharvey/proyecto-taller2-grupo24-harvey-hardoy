using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.CasosDeUso;
using Aplicacion.DTOs;

namespace Presentacion.Vistas;

public partial class AltaUsuarioView : UserControl
{
    private readonly CrearUsuario _crearUsuario;
    private readonly ListarRoles _listarRoles;
    private readonly ListarUsuarios _listarUsuarios;

    public AltaUsuarioView()
    {
        InitializeComponent();

        _crearUsuario = App.Services.GetRequiredService<CrearUsuario>();
        _listarRoles = App.Services.GetRequiredService<ListarRoles>();
        _listarUsuarios = App.Services.GetRequiredService<ListarUsuarios>();

        CargarComboRoles();
        CargarGrilla();
    }

    private void CargarComboRoles()
    {
        cmbRol.ItemsSource = null;
        cmbRol.ItemsSource = _listarRoles.Ejecutar();
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

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLeerFormulario(out var dto)) return;

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

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
    }

    private bool TryLeerFormulario(out UsuarioCrearDTO dto)
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

        if (string.IsNullOrEmpty(txtPassword.Password))
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

        if (cmbRol.SelectedValue is not int rolId)
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
