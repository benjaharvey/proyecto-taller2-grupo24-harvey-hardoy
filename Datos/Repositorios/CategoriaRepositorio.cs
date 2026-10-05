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

        

    }
}