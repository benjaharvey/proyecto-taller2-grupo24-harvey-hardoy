using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Aplicacion.DTOs;
using Aplicacion.CasosDeUso;

namespace Presentacion
{
    public partial class RolWindow : Window
    {
        private readonly CrearRol _crearRol;
        private readonly ListarRoles _listarRoles;
        private readonly ActualizarRol _actualizarRol;
        private readonly EliminarRol _eliminarRol;

        public RolWindow(CrearRol crearRol, ListarRoles listarRoles, ActualizarRol actualizarRol, EliminarRol eliminarRol)
        {
            InitializeComponent();

            _crearRol = crearRol;
            _listarRoles = listarRoles;
            _actualizarRol = actualizarRol;
            _eliminarRol = eliminarRol;

            CargarGrilla();
        }

        private void CargarGrilla()
        {
            dataGridRoles.ItemsSource = null;
            dataGridRoles.ItemsSource = _listarRoles.Ejecutar();
        }

        private void dataGridRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool haySeleccion = dataGridRoles.SelectedItem is RolDTO;
            btnAgregar.IsEnabled = !haySeleccion;
            btnActualizar.IsEnabled = haySeleccion;
            btnEliminar.IsEnabled = haySeleccion;

            // Sin esto el TextBox queda vacío y "Actualizar" renombraría el rol a cadena vacía.
            if (dataGridRoles.SelectedItem is RolDTO seleccionado)
                txtNombre.Text = seleccionado.Nombre;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dto = new RolDTO { Nombre = txtNombre.Text };
                _crearRol.Ejecutar(dto);
                CargarGrilla();
                txtNombre.Clear();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridRoles.SelectedItem is not RolDTO seleccionado)
            {
                MessageBox.Show("Seleccioná un rol de la lista para actualizar.");
                return;
            }

            try
            {
                var dto = new RolDTO { Nombre = txtNombre.Text };
                _actualizarRol.Ejecutar(seleccionado.Id, dto);
                LimpiarFormulario();
                CargarGrilla();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridRoles.SelectedItem is not RolDTO seleccionado)
            {
                MessageBox.Show("Seleccioná un rol de la lista para eliminar.");
                return;
            }

            var confirmacion = MessageBox.Show(
                this,
                $"¿Seguro que querés eliminar el rol \"{seleccionado.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                _eliminarRol.Ejecutar(seleccionado.Id);
                LimpiarFormulario();
                CargarGrilla();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                CargarGrilla();
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            dataGridRoles.SelectedItem = null;
            txtNombre.Clear();
        }

        private void btnGestionarUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var usuarioWindow = App.Services.GetRequiredService<UsuarioWindow>();
            usuarioWindow.Show();
        }
    }
}
