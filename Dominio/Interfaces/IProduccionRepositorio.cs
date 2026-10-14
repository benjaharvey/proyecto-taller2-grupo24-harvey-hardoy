using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IProduccionRepositorio
    {
        void Agregar(Produccion produccion);
        List<Produccion> ObtenerTodos();
    }
}