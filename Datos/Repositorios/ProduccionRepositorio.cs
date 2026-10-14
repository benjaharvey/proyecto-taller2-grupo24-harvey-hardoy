using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;

namespace Datos.Repositorios
{
    public class ProduccionRepositorio: IProduccionRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProduccionRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Agregar(Produccion p_produccion)
        {
            
        }

        public List<Produccion> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Producciones
                .Include(p => p.Producto)
                .Include(p => p.Usuario)
                .Include(p => p.Sucursal)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }
    }
}