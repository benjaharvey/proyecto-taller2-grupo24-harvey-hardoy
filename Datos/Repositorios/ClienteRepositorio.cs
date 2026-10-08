using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ClienteRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Cliente> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Clientes.Include(c => c.Usuario).Where(c => c.DeletedAt == null).ToList();
        }

        public Cliente? ObtenerPorId(int p_id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Clientes.FirstOrDefault(c => c.DeletedAt == null && c.Id == p_id);
        }

        public Cliente? ObtenerPorDni(string p_dni)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Clientes.FirstOrDefault(c => c.DeletedAt == null && c.Dni == p_dni);
        }

        public Cliente? ObtenerPorEmail(string p_email)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Clientes.FirstOrDefault(c => c.DeletedAt == null && c.Email == p_email);
        }

        public void Agregar(Cliente cliente)
        {
            using var context = _contextFactory.CreateDbContext();
            cliente.CreatedAt = DateTime.Now;
            context.Clientes.Add(cliente);
            context.SaveChanges();
        }

        public void Eliminar(int id){
            using var context = _contextFactory.CreateDbContext();
            var clientePorEliminar = context.Clientes.FirstOrDefault(c => c.DeletedAt == null && c.Id == id);

            if (clientePorEliminar == null){
                throw new InvalidOperationException("El cliente que desea eliminar no existe / ya fue eliminado");
            } else {
                clientePorEliminar.DeletedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Reactivar(int id){
            using var context = _contextFactory.CreateDbContext();
            var clientePorReactivar = context.Clientes.FirstOrDefault(c => c.Id == id && c.DeletedAt != null);

            if (clientePorReactivar == null){
                throw new InvalidOperationException("El cliente que desea reactivar no existe / ya fue reactivado");
            } else {
                clientePorReactivar.DeletedAt = null;
                clientePorReactivar.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Actualizar(Cliente p_cliente){
            using var context = _contextFactory.CreateDbContext();
            var ClientePorActualizar = context.Clientes.FirstOrDefault(c => c.DeletedAt == null && c.Id == p_cliente.Id);
            if (ClientePorActualizar == null){
                throw new InvalidOperationException("El cliente que desea actualizar no existe");
            } else
            {
                ClientePorActualizar.Apellido = p_cliente.Apellido;
                ClientePorActualizar.Nombre = p_cliente.Nombre;
                ClientePorActualizar.Dni = p_cliente.Dni;
                ClientePorActualizar.Email = p_cliente.Email;
                ClientePorActualizar.FechaNacimiento = p_cliente.FechaNacimiento;
                ClientePorActualizar.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }

        }
            
    }
}