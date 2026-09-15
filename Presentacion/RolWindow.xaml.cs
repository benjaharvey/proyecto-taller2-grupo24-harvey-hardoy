using System;
using System.Windows;
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
            if (dataGridRoles.SelectedItem is not RolDTO seleccionado) return;

            var dto = new RolDTO { Nombre = txtNombre.Text };
            _actualizarRol.Ejecutar(seleccionado.Id, dto);
            CargarGrilla();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridRoles.SelectedItem is not RolDTO seleccionado) return;

            _eliminarRol.Ejecutar(seleccionado.Id);
            CargarGrilla();
        }

        private void btnGestionarUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var usuarioWindow = App.Services.GetRequiredService<UsuarioWindow>();
            usuarioWindow.Show();
        }
    }
}
