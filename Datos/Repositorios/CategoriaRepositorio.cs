using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Entidades;
using Dominio.Interfaces;
using Datos.Conexion;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design.Serialization;
using Microsoft.VisualBasic;

namespace Datos.Repositorios
{
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public CategoriaRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Categoria> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Categorias.Where(categoria => categoria.DeletedAt == null).ToList();
        }

        public Categoria? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Categorias.FirstOrDefault(categoria => categoria.DeletedAt == null && categoria.Id == id);
        }

        public Categoria? ObtenerPorNombre(string p_nombre)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Categorias.FirstOrDefault(categoria => categoria.DeletedAt == null && categoria.Nombre == p_nombre);
        }

        public void Agregar(Categoria categoria)
        {
            using var context = _contextFactory.CreateDbContext();
            categoria.CreatedAt = DateTime.Now;
            context.Categorias.Add(categoria);
            context.SaveChanges();
        }

        public void Actualizar(Categoria p_categoria)
        {
            using var context = _contextFactory.CreateDbContext();
            var categoriaExistente = context.Categorias.FirstOrDefault(categoria => categoria.Id == p_categoria.Id && categoria.DeletedAt == null)
                ?? throw new InvalidOperationException("La categoria que intentás modificar ya no existe. Recargá la lista.");

            categoriaExistente.Nombre = p_categoria.Nombre;
            categoriaExistente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var categoria = context.Categorias.FirstOrDefault(categoria => categoria.Id == id && categoria.DeletedAt == null);
            if(categoria == null)
            {
                throw new InvalidOperationException("La categoria que intentas eliminar no existe!. Reingrese el id");
            } else
            {
                categoria.DeletedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Reactivar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var categoria = context.Categorias.FirstOrDefault(p_categoria => p_categoria.Id == id);
            if (categoria == null) return;
            categoria.DeletedAt = null;
            categoria.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}