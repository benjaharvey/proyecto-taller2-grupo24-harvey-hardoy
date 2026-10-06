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
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProductoRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Producto> ObtenerTodos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Productos.Include(p => p.ProductoInsumos)
            .ThenInclude(pi => pi.Insumo)
            .Where(producto => producto.DeletedAt == null).ToList();
        }

        public Producto? ObtenerPorId(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Productos.Include(p => p.ProductoInsumos)
            .ThenInclude(pi => pi.Insumo)
            .FirstOrDefault(producto => producto.Id == id && producto.DeletedAt == null);
        }

        public Producto? ObtenerPorNombre(string nombre)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Productos.FirstOrDefault(producto => producto.Nombre == nombre && producto.DeletedAt == null);
        }

        public int ContarPorCategoria(int categoriaId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Productos.Count(productos => productos.CategoriaId == categoriaId && productos.DeletedAt == null);
        }

        public void Agregar(Producto producto)
        {
            using var context = _contextFactory.CreateDbContext();
            producto.CreatedAt = DateTime.Now;
            context.Productos.Add(producto);
            context.SaveChanges();
        }

        public void Actualizar(Producto p_Producto)
        {
            using var context = _contextFactory.CreateDbContext();
            var productoExistente = context.Productos
                                            .Include(p => p.ProductoInsumos)
                                            .FirstOrDefault(producto => producto.Id == p_Producto.Id && producto.DeletedAt == null)
                ?? throw new InvalidOperationException("El producto que intentás modificar ya no existe. Recargá la lista.");

            productoExistente.Nombre = p_Producto.Nombre;
            productoExistente.Precio = p_Producto.Precio;
            productoExistente.RutaImagen = p_Producto.RutaImagen;
            productoExistente.CategoriaId = p_Producto.CategoriaId;

            // Sacar insumos que el usuario saco de la receta
            var nuevosInsumoIds = p_Producto.ProductoInsumos.Select(pi => pi.InsumoId).ToHashSet();
                var insumosAEliminar = productoExistente.ProductoInsumos
                    .Where(pi => !nuevosInsumoIds.Contains(pi.InsumoId))
                    .ToList();

                foreach (var item in insumosAEliminar)
                {
                    productoExistente.ProductoInsumos.Remove(item);
                }

                // Modificar existentes o agregar nuevos
                foreach (var nuevoItem in p_Producto.ProductoInsumos)
                {
                    var existente = productoExistente.ProductoInsumos
                        .FirstOrDefault(pi => pi.InsumoId == nuevoItem.InsumoId);

                    if (existente != null)
                    {
                        // Si ya estaba, actualizamos la cantidad
                        existente.CantidadNecesaria = nuevoItem.CantidadNecesaria;
                    }
                    else
                    {
                        // Si es nuevo en la receta, lo agregamos
                        productoExistente.ProductoInsumos.Add(new ProductoInsumo
                        {
                            ProductoId = productoExistente.Id,
                            InsumoId = nuevoItem.InsumoId,
                            CantidadNecesaria = nuevoItem.CantidadNecesaria
                        });
                    }
                }

            productoExistente.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var producto = context.Productos.FirstOrDefault(p_producto => p_producto.Id == id && p_producto.DeletedAt == null);
            if(producto == null)
            {
                throw new InvalidOperationException("El producto que intentas eliminar no existe!. Reingrese el id");
            } else
            {
                producto.DeletedAt = DateTime.Now;
                context.SaveChanges();
            }
            
        }

        public void Reactivar(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var producto = context.Productos.FirstOrDefault(producto => producto.Id == id);
            if (producto == null) return;
            producto.DeletedAt = null;
            producto.UpdatedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}
