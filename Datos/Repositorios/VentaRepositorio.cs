using System.Data;
using Datos.Conexion;
using Dominio.Consultas;
using Dominio.Entidades;
using Dominio.Enums;
using Dominio.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Datos.Repositorios
{
    public class VentaRepositorio: IVentaRepositorio
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public VentaRepositorio(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Esto es cuando se abre la venta (se crea como pendiente y vacia)
        public int Abrir(VentaCabecera venta)
        {
            using var context = _contextFactory.CreateDbContext();

            venta.Estado = EstadoVenta.PENDIENTE;
            venta.Total = 0m;
            venta.CreatedAt = DateTime.Now;
            venta.UpdatedAt = null;

            context.VentasCabecera.Add(venta);
            context.SaveChanges();

            return venta.Id;
        }

        // Obtenemos la venta pendiente (la usamos cuando volvemos a la seccion Venta o por si hubo un crash del sistema)
        public VentaCabecera? ObtenerPendiente(int usuarioId)
        {
            using var context = _contextFactory.CreateDbContext();

            return context.VentasCabecera
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Sucursal)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.UsuarioId == usuarioId
                                  && v.Estado == EstadoVenta.PENDIENTE);
        }

        // Obtenemos la venta por id (la usamos para generar el comprobante, traerlo del historial y traer el detalle de la venta en el historial)
        public VentaCabecera? ObtenerPorId(int ventaId)
        {
            using var context = _contextFactory.CreateDbContext();

            return context.VentasCabecera
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Sucursal)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == ventaId);
        }

        public void AgregarItem(int ventaId, int productoId, int cantidad, decimal precioUnit)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
            if (precioUnit <= 0)
                throw new InvalidOperationException("El precio unitario debe ser mayor a cero.");

            using var context = _contextFactory.CreateDbContext();
            var ahora = DateTime.Now;

            var venta = CargarPendiente(context, ventaId);

            // El precio se congela en la primera carga del producto.
            var item = venta.Detalles.FirstOrDefault(d => d.ProductoId == productoId);

            if (item == null) // si no existe, creamos la ventadetalle con ese producto
            {
                venta.Detalles.Add(new VentaDetalle
                {
                    VentaCabeceraId = venta.Id,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnit = precioUnit,
                    CreatedAt = ahora
                });
            }
            else //sumamos a la cantidad del item que ya se cargo
            {
                item.Cantidad += cantidad;
            }

            venta.Total = CalcularTotal(venta);
            venta.UpdatedAt = ahora;

            context.SaveChanges();
        }

        public void CambiarCantidad(int detalleId, int cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            using var context = _contextFactory.CreateDbContext();
            var ahora = DateTime.Now;

            // buscamos la cabecera por id del detalle para verificar estado y recalcular el total
            var ventaId = ObtenerVentaIdDelDetalle(context, detalleId);
            var venta = CargarPendiente(context, ventaId);

            var item = venta.Detalles.FirstOrDefault(d => d.Id == detalleId)
                ?? throw new InvalidOperationException("El ítem no pertenece a la venta.");

            item.Cantidad = cantidad;

            // luego de modificar la cantidad, recalculamos total
            venta.Total = CalcularTotal(venta);
            venta.UpdatedAt = ahora;

            context.SaveChanges();
        }

        public void QuitarItem(int detalleId)
        {
            using var context = _contextFactory.CreateDbContext();
            var ahora = DateTime.Now;

            // buscamos la cabecera por id del detalle para verificar estado y recalcular el total
            var ventaId = ObtenerVentaIdDelDetalle(context, detalleId);
            var venta = CargarPendiente(context, ventaId);

            var item = venta.Detalles.FirstOrDefault(d => d.Id == detalleId)
                ?? throw new InvalidOperationException("El ítem no pertenece a la venta.");

            // Se saca de la colección para que el total se recalcule sin el ítem
            venta.Detalles.Remove(item);
            context.VentasDetalle.Remove(item);

            venta.Total = CalcularTotal(venta);
            venta.UpdatedAt = ahora;

            context.SaveChanges();
        }

        // Cambios de estado
        public void Confirmar(int ventaId, int clienteId, MetodoPago metodoPago)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaccion = context.Database.BeginTransaction();

            var ahora = DateTime.Now;

            var sucursalId = context.VentasCabecera
                .AsNoTracking()
                .Where(v => v.Id == ventaId)
                .Select(v => (int?)v.SucursalId)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("La venta no existe.");

            // Transición condicional y atómica. Cliente, método de pago y total
            // se completan en el mismo UPDATE que cambia el estado, porque
            // nuestro check exige no nulos fuera de PENDIENTE.
            var filas = context.Database.ExecuteSqlRaw("""
                UPDATE VentasCabecera
                    SET Estado     = 'CONFIRMADA',
                       ClienteId  = @ClienteId,
                       MetodoPago = @MetodoPago,
                       Total      = (SELECT SUM(Cantidad * PrecioUnit)
                                       FROM VentasDetalle
                                      WHERE VentaCabeceraId = @VentaId),
                       UpdatedAt  = @Ahora
                WHERE Id = @VentaId
                   AND Estado = 'PENDIENTE'
                   AND EXISTS (SELECT 1 FROM VentasDetalle WHERE VentaCabeceraId = @VentaId);
                """,
                new SqlParameter("@VentaId", ventaId),
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@MetodoPago", metodoPago.ToString()),
                new SqlParameter("@Ahora", ahora));

            if (filas == 0)
                throw new InvalidOperationException("La venta ya no está pendiente o no tiene ítems.");

            // Ordenar por ProductoId antes de tocar stock evita problemas
            // entre dos confirmaciones a la vez.
            var detalles = context.VentasDetalle
                .AsNoTracking()
                .Where(d => d.VentaCabeceraId == ventaId)
                .OrderBy(d => d.ProductoId)
                .Select(d => new { d.ProductoId, NombreProducto = d.Producto!.Nombre, d.Cantidad })
                .ToList();

            ValidarProductosActivos(context, detalles.Select(d => d.ProductoId).ToList());

            foreach (var detalle in detalles)
            {
                var descontadas = DescontarStock(context, sucursalId, detalle.ProductoId, detalle.Cantidad, ahora);

                if (descontadas == 0)
                    throw new InvalidOperationException(
                        $"Stock insuficiente de {detalle.NombreProducto} en la sucursal {sucursalId}.");
            }

            transaccion.Commit();
        }

        public void Cancelar(int ventaId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaccion = context.Database.BeginTransaction();

            var ahora = DateTime.Now;

            var sucursalId = context.VentasCabecera
                .AsNoTracking()
                .Where(v => v.Id == ventaId)
                .Select(v => (int?)v.SucursalId)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("La venta no existe.");

            var filas = context.Database.ExecuteSqlRaw("""
                UPDATE VentasCabecera
                   SET Estado    = 'CANCELADA',
                       UpdatedAt = @Ahora
                 WHERE Id = @VentaId
                   AND Estado = 'CONFIRMADA';
                """,
                new SqlParameter("@VentaId", ventaId),
                new SqlParameter("@Ahora", ahora));

            if (filas == 0)
                throw new InvalidOperationException("Solo se puede cancelar una venta confirmada.");

            var detalles = context.VentasDetalle
                .AsNoTracking()
                .Where(d => d.VentaCabeceraId == ventaId)
                .OrderBy(d => d.ProductoId)
                .Select(d => new { d.ProductoId, d.Cantidad })
                .ToList();

            foreach (var detalle in detalles)
                ReponerStock(context, sucursalId, detalle.ProductoId, detalle.Cantidad, ahora);

            transaccion.Commit();
        }

        public void Descartar(int ventaId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaccion = context.Database.BeginTransaction();

            // Borrado condicional: si la venta se confirmó en el medio, la
            // cabecera no se elimina y el rollback devuelve los detalles.
            context.Database.ExecuteSqlRaw("""
                DELETE FROM VentasDetalle
                 WHERE VentaCabeceraId = @VentaId
                   AND EXISTS (SELECT 1 FROM VentasCabecera
                                WHERE Id = @VentaId AND Estado = 'PENDIENTE');
                """,
                new SqlParameter("@VentaId", ventaId));

            var filas = context.Database.ExecuteSqlRaw("""
                DELETE FROM VentasCabecera
                 WHERE Id = @VentaId
                   AND Estado = 'PENDIENTE';
                """,
                new SqlParameter("@VentaId", ventaId));

            if (filas == 0)
                throw new InvalidOperationException("Solo se puede descartar una venta pendiente.");

            transaccion.Commit();
        }

        public List<VentaCabecera> ObtenerHistorial(VentaFiltro filtro)
        {
            using var context = _contextFactory.CreateDbContext();

            var query = context.VentasCabecera
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Sucursal)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.Estado != EstadoVenta.PENDIENTE);

            if (filtro.SucursalId.HasValue)
                query = query.Where(v => v.SucursalId == filtro.SucursalId.Value);

            if (filtro.UsuarioId.HasValue)
                query = query.Where(v => v.UsuarioId == filtro.UsuarioId.Value);

            if (filtro.MetodoPago.HasValue)
                query = query.Where(v => v.MetodoPago == filtro.MetodoPago.Value);

            if (filtro.Desde.HasValue)
                query = query.Where(v => v.CreatedAt >= filtro.Desde.Value.Date);

            if (filtro.Hasta.HasValue)
            {
                // Rango semiabierto: incluye todo el día elegido.
                var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
                query = query.Where(v => v.CreatedAt < hastaExclusivo);
            }

            if (!string.IsNullOrWhiteSpace(filtro.ClienteTexto))
            {
                var texto = filtro.ClienteTexto!.Trim();
                query = query.Where(v =>
                    v.Cliente != null &&
                    (v.Cliente.Nombre.Contains(texto) ||
                     v.Cliente.Apellido.Contains(texto) ||
                     v.Cliente.Dni.Contains(texto) ||
                     v.Cliente.Email.Contains(texto)));
            }

            return query
                .OrderByDescending(v => v.CreatedAt)
                .ThenByDescending(v => v.Id)
                .ToList();
        }

        public List<(string NombreProducto, int Unidades, decimal Recaudado)> ObtenerProductosMasVendidos(
            DateTime desde, DateTime hastaExclusivo, int? sucursalId, int top)
        {
            using var context = _contextFactory.CreateDbContext();

            // Se arranca por los renglones de venta: lo que se agrupa y se suma está acá.
            var query = context.VentasDetalle
                .Where(d => d.Venta!.Estado == EstadoVenta.CONFIRMADA
                         && d.Venta.CreatedAt >= desde
                         && d.Venta.CreatedAt < hastaExclusivo);

            // Filtro opcional: solo se agrega si el usuario eligió una sucursal.
            if (sucursalId.HasValue)
                query = query.Where(d => d.Venta!.SucursalId == sucursalId.Value);

            // El GROUP BY, los SUM, el ORDER BY y el TOP se ejecutan en SQL Server.
            return query
                .GroupBy(d => d.Producto!.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Unidades = g.Sum(d => d.Cantidad),
                    Recaudado = g.Sum(d => d.Cantidad * d.PrecioUnit)
                })
                .OrderByDescending(x => x.Unidades)
                .Take(top)
                .AsEnumerable()
                .Select(x => (x.Nombre, x.Unidades, x.Recaudado))
                .ToList();
        }

        // ---------------------------------------------------------------
        // Helpers privados
        // ---------------------------------------------------------------

        private static VentaCabecera CargarPendiente(AppDbContext context, int ventaId)
        {
            var venta = context.VentasCabecera
                .Include(v => v.Detalles)
                .FirstOrDefault(v => v.Id == ventaId)
                ?? throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado != EstadoVenta.PENDIENTE)
                throw new InvalidOperationException("La venta ya fue confirmada o cancelada y no admite cambios.");

            return venta;
        }

        private static int ObtenerVentaIdDelDetalle(AppDbContext context, int detalleId)
        {
            return context.VentasDetalle
                .AsNoTracking()
                .Where(d => d.Id == detalleId)
                .Select(d => (int?)d.VentaCabeceraId)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("El ítem no existe.");
        }

        private static decimal CalcularTotal(VentaCabecera venta)
        {
            return Math.Round(
                venta.Detalles.Sum(d => d.Cantidad * d.PrecioUnit),
                2,
                MidpointRounding.AwayFromZero);
        }

        private static void ValidarProductosActivos(AppDbContext context, List<int> productoIds)
        {
            if (productoIds.Count == 0)
                return;

            var dadosDeBaja = context.Productos
                .AsNoTracking()
                .Where(p => productoIds.Contains(p.Id) && p.DeletedAt != null)
                .Select(p => p.Nombre)
                .ToList();

            if (dadosDeBaja.Count > 0)
                throw new InvalidOperationException(
                    $"La venta incluye productos dados de baja: {string.Join(", ", dadosDeBaja)}.");
        }

        private static int DescontarStock(
            AppDbContext context, int sucursalId, int productoId, int cantidad, DateTime ahora)
        {
            var filasAfectadas = new SqlParameter("@FilasAfectadas", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            context.Database.ExecuteSqlRaw(
                "EXEC dbo.DescontarStockVenta @SucursalId, @ProductoId, @Cantidad, @Ahora, @FilasAfectadas OUTPUT",
                new SqlParameter("@SucursalId", sucursalId),
                new SqlParameter("@ProductoId", productoId),
                new SqlParameter("@Cantidad", cantidad),
                new SqlParameter("@Ahora", ahora),
                filasAfectadas);

            return Convert.ToInt32(filasAfectadas.Value);
        }

        private static void ReponerStock(
            AppDbContext context, int sucursalId, int productoId, int cantidad, DateTime ahora)
        {
            var filasAfectadas = new SqlParameter("@FilasAfectadas", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            context.Database.ExecuteSqlRaw(
                "EXEC dbo.ReponerStockVenta @SucursalId, @ProductoId, @Cantidad, @Ahora, @FilasAfectadas OUTPUT",
                new SqlParameter("@SucursalId", sucursalId),
                new SqlParameter("@ProductoId", productoId),
                new SqlParameter("@Cantidad", cantidad),
                new SqlParameter("@Ahora", ahora),
                filasAfectadas);
        }
    }
}