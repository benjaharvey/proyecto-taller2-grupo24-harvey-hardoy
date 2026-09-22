using System;
using System.Windows;
using Aplicacion.DTOs;
using Aplicacion.CasosDeUso;

namespace Presentacion
{
    public partial class UsuarioWindow : Window
    {
        private readonly CrearUsuario _crearUsuario;
        private readonly ListarUsuarios _listarUsuarios;
        private readonly ActualizarUsuario _actualizarUsuario;
        private readonly EliminarUsuario _eliminarUsuario;
        private readonly ListarRoles _listarRoles;

        public UsuarioWindow(
            CrearUsuario crearUsuario,
            ListarUsuarios listarUsuarios,
            ActualizarUsuario actualizarUsuario,
            EliminarUsuario eliminarUsuario,
            ListarRoles listarRoles)
        {
            InitializeComponent();

            _crearUsuario = crearUsuario;
            _listarUsuarios = listarUsuarios;
            _actualizarUsuario = actualizarUsuario;
            _eliminarUsuario = eliminarUsuario;
            _listarRoles = listarRoles;

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

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (!TryLeerFormulario(out var dto)) return;

            try
            {
                _crearUsuario.Ejecutar(dto);
                CargarGrilla();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsuarios.SelectedItem is not UsuarioDTO seleccionado) return;
            if (!TryLeerFormulario(out var dto)) return;

            try
            {
                _actualizarUsuario.Ejecutar(seleccionado.Id, dto);
                CargarGrilla();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool TryLeerFormulario(out UsuarioCrearDTO dto)
        {
            dto = new UsuarioCrearDTO();

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

            if (dtpFechaNacimiento.SelectedDate is not DateTime fechaNacimiento)
            {
                MessageBox.Show("Seleccioná una fecha de nacimiento.");
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

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridUsuarios.SelectedItem is not UsuarioDTO seleccionado) return;

            try
            {
                _eliminarUsuario.Ejecutar(seleccionado.Id);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }

            CargarGrilla();
        }
    }
}