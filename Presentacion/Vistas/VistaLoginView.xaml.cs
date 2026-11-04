using Aplicacion.CasosDeUso;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentacion
{
    public partial class VistaLogin : Window
    {

        private readonly IniciarSesion _iniciarSesion;

        public VistaLogin(IniciarSesion iniciarSesion)
        {
            InitializeComponent();
            _iniciarSesion = iniciarSesion;
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Ingresá tu email y contraseña.", "Inicio de sesión",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var usuario = _iniciarSesion.Ejecutar(email, password);
                SesionActual.UsuarioLogueado = usuario;

                var mainWindow = App.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error de inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo iniciar sesión.\n\nDetalle: {ex.Message}", "Error inesperado",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
