using Aplicacion.DTOs;
using Dominio.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Aplicacion.Servicios
{
    public class ComprobantePdfServicio : IComprobantePdfServicio
    {
        public string Generar(VentaDTO venta)
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Maie",
                "Facturas");

            Directory.CreateDirectory(carpeta);

            var ruta = Path.Combine(carpeta, $"{venta.NumeroComprobante}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Maie — Dulces Artesanales").FontSize(20).SemiBold();
                        col.Item().Text($"Comprobante de venta {venta.NumeroComprobante}");
                        col.Item().Text("Comprobante no fiscal")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
                            row.RelativeItem().Text($"Sucursal: {venta.NombreSucursal}");
                            row.RelativeItem().Text($"Vendedor: {venta.NombreVendedor}");
                        });

                        col.Item().Text($"Cliente: {venta.NombreCliente} — DNI {venta.DniCliente}");
                        col.Item().Text($"Método de pago: {venta.MetodoPago}");

                        col.Item().PaddingTop(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            tabla.Header(h =>
                            {
                                h.Cell().Element(Encabezado).Text("Producto");
                                h.Cell().Element(Encabezado).AlignRight().Text("Cant.");
                                h.Cell().Element(Encabezado).AlignRight().Text("Precio unit.");
                                h.Cell().Element(Encabezado).AlignRight().Text("Subtotal");
                            });

                            foreach (var detalle in venta.Detalles)
                            {
                                tabla.Cell().Element(Celda).Text(detalle.NombreProducto);
                                tabla.Cell().Element(Celda).AlignRight().Text(detalle.Cantidad.ToString());
                                tabla.Cell().Element(Celda).AlignRight().Text($"${detalle.PrecioUnit:N2}");
                                tabla.Cell().Element(Celda).AlignRight().Text($"${detalle.Subtotal:N2}");
                            }
                        });

                        col.Item().AlignRight().PaddingTop(10)
                            .Text($"Total: ${venta.Total:N2}")
                            .FontSize(14)
                            .Bold();

                        if (venta.Estado == EstadoVenta.CANCELADA)
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Background(Colors.Red.Lighten4)
                                .Padding(8)
                                .Text("VENTA CANCELADA")
                                .FontColor(Colors.Red.Darken2)
                                .Bold();
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(ruta);

            return ruta;
        }

        private static IContainer Encabezado(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Padding(4)
                .DefaultTextStyle(x => x.SemiBold());
        }

        private static IContainer Celda(IContainer container)
        {
            return container
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4);
        }
    }
}