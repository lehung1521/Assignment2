using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace B2BWholesale.Data.Services;

public sealed class ShippingManifestPdfGenerator : IShippingManifestPdfGenerator
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("vi-VN");

    static ShippingManifestPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(ShippingManifestData manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("B2B WHOLESALE")
                        .FontSize(11).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(4).AlignCenter().Text("SHIPPING MANIFEST")
                        .FontSize(19).Bold();
                    column.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                page.Content().PaddingTop(18).Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Order ID: ").SemiBold();
                            text.Span(manifest.OrderId.ToString(CultureInfo.InvariantCulture));
                        });
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Order date: ").SemiBold();
                            text.Span(manifest.OrderDate.ToString("yyyy-MM-dd HH:mm"));
                        });
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Status: ").SemiBold();
                        text.Span(manifest.OrderStatus);
                    });

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(agent =>
                    {
                        agent.Spacing(3);
                        agent.Item().Text("SHIP TO").SemiBold().FontColor(Colors.Blue.Darken2);
                        agent.Item().Text(manifest.AgentName);
                        agent.Item().Text(manifest.AgentAddress);
                        agent.Item().Text($"Phone: {manifest.AgentPhone}");
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Item");
                            header.Cell().Element(HeaderCell).Text("Size");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Unit price");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Line total");
                        });

                        foreach (var line in manifest.Lines)
                        {
                            table.Cell().Element(BodyCell).Text(line.ItemName);
                            table.Cell().Element(BodyCell).Text(line.Size);
                            table.Cell().Element(BodyCell).AlignRight().Text(line.Quantity.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(line.UnitPrice));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(line.LineTotal));
                        }

                        table.Cell().ColumnSpan(4).Element(TotalCell).AlignRight().Text("Grand total").SemiBold();
                        table.Cell().Element(TotalCell).AlignRight().Text(FormatCurrency(manifest.GrandTotal)).SemiBold();
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated ");
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                    text.Span("  •  Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5);

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

    private static IContainer TotalCell(IContainer container) =>
        container.PaddingTop(8).PaddingHorizontal(5);

    private static string FormatCurrency(decimal amount) =>
        $"{amount.ToString("N0", CurrencyCulture)} ₫";
}
