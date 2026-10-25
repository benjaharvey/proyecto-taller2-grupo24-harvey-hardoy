using Aplicacion.DTOs;
using Dominio.Entidades;

namespace Aplicacion.Mapeadores
{
    public static class VentaMapper
    {
        public static VentaDTO ToDTO(VentaCabecera venta)
        {
            return new VentaDTO
            {
                Id = venta.Id,
                NumeroComprobante = $"VTA-{venta.CreatedAt:yyyy}-{venta.Id:D6}",
                Fecha = venta.CreatedAt,

                SucursalId = venta.SucursalId,
                NombreSucursal = venta.Sucursal?.Nombre ?? "Sin sucursal",

                ClienteId = venta.ClienteId,
                NombreCliente = venta.Cliente == null
                    ? "Consumidor final"
                    : $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim(),
                DniCliente = venta.Cliente?.Dni ?? "",

                UsuarioId = venta.UsuarioId,
                NombreVendedor = venta.Usuario == null
                    ? ""
                    : $"{venta.Usuario.Nombre} {venta.Usuario.Apellido}".Trim(),

                MetodoPago = venta.MetodoPago,
                Estado = venta.Estado,
                CantidadItems = venta.Detalles.Sum(d => d.Cantidad),
                Total = venta.Total,

                Detalles = venta.Detalles
                    .Select(d => new VentaDetalleDTO
                    {
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto?.Nombre ?? "Producto eliminado",
                        Cantidad = d.Cantidad,
                        PrecioUnit = d.PrecioUnit
                    })
                    .ToList()
            };
        }
    }
}