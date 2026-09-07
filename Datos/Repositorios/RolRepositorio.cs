using Dominio.Entidades;
using Dominio.Interfaces;

namespace Datos.Repositorios
{
    public class RolRepositorio : IRolRepositoro
    {
        private List<Usuario> usuarios = new List<Usuario>();
        private int siguienteId = 1;

        public List<Rol> ObtenerTodos()
        {
            return roles.Where(r => r.DeletedAt == null).ToList();
        }
    }
}