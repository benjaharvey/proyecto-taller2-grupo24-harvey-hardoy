using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface ICategoriaRepositorio
    {
        List<Categoria> ObtenerTodos();

        Categoria? ObtenerPorId(int id);
        Categoria? ObtenerPorNombre(string nombre);

        void Agregar(Categoria categoria);
        void Actualizar(Categoria categoria);
        void Eliminar(int id);

/* CONSULTAR EN CLASE CON EL PROFESOR SI ES NECESARIO TENER UN REACTIVAR PRODUCTO */
        void Reactivar(int id);
    }
}