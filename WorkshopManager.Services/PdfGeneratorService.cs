using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Services;

public interface IPdfGeneratorService
{
    byte[] GenerateMechanicStatisticsPdf(MechanicStatisticsViewModel statistics);
    byte[] GenerateMechanicsComparisonPdf(MechanicsComparisonViewModel comparison);
    byte[] GenerateOrderReportPdf(OrderReportViewModel report);
}

public class PdfGeneratorService : IPdfGeneratorService
{
    public PdfGeneratorService()
    {
        // Konfiguracja QuestPDF - wymaga licencji dla użytku komercyjnego
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateMechanicStatisticsPdf(MechanicStatisticsViewModel statistics)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Text($"Statystyki Mechanika - {statistics.MechanicFullName}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        // Informacje podstawowe
                        col.Item().Text(text =>
                        {
                            text.Span("Okres: ").SemiBold();
                            text.Span($"{statistics.Period} ({statistics.StartDate:dd.MM.yyyy} - {statistics.EndDate:dd.MM.yyyy})");
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Podsumowanie
                        col.Item().PaddingTop(10).Text("Podsumowanie").SemiBold().FontSize(16);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            // Nagłówki
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Metryka").SemiBold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Wartość").SemiBold();
                            });

                            // Dane
                            table.Cell().Element(CellStyle).Text("Łączna liczba zleceń");
                            table.Cell().Element(CellStyle).AlignRight().Text(statistics.TotalOrders.ToString());

                            table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).Text("Zakończone zlecenia");
                            table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).AlignRight()
                                .Text(statistics.CompletedOrders.ToString());

                            table.Cell().Element(CellStyle).Text("Anulowane zlecenia");
                            table.Cell().Element(CellStyle).AlignRight().Text(statistics.CancelledOrders.ToString());

                            table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).Text("W trakcie realizacji");
                            table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).AlignRight()
                                .Text(statistics.InProgressOrders.ToString());

                            table.Cell().Element(CellStyle).Text("Łączne zarobki").Bold();
                            table.Cell().Element(CellStyle).AlignRight()
                                .Text($"{statistics.TotalEarnings:C}").Bold().FontColor(Colors.Green.Darken2);
                        });

                        // Rozkład według statusu
                        if (statistics.StatusBreakdown.Any())
                        {
                            col.Item().PaddingTop(20).Text("Rozkład zleceń według statusu").SemiBold().FontSize(16);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Liczba").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Zarobki").SemiBold();
                                });

                                var isAlternate = false;
                                foreach (var statusStat in statistics.StatusBreakdown)
                                {
                                    var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                                    table.Cell().Element(CellStyle).Background(bgColor).Text(statusStat.Status);
                                    table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                        .Text(statusStat.Count.ToString());
                                    table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                        .Text($"{statusStat.TotalEarnings:C}");

                                    isAlternate = !isAlternate;
                                }
                            });
                        }

                        // Statystyki dzienne (tylko jeśli nie ma zbyt wielu dni)
                        if (statistics.DailyStatistics.Any() && statistics.DailyStatistics.Count <= 31)
                        {
                            col.Item().PageBreak();
                            col.Item().Text("Statystyki dzienne").SemiBold().FontSize(16);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Data").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Zlecenia").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Zakończone").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Zarobki").SemiBold();
                                });

                                var isAlternate = false;
                                foreach (var daily in statistics.DailyStatistics)
                                {
                                    var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                                    table.Cell().Element(CellStyle).Background(bgColor).Text(daily.Date.ToString("dd.MM.yyyy"));
                                    table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                        .Text(daily.OrdersCount.ToString());
                                    table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                        .Text(daily.CompletedCount.ToString());
                                    table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                        .Text($"{daily.Earnings:C}");

                                    isAlternate = !isAlternate;
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Strona ");
                        text.CurrentPageNumber();
                        text.Span(" z ");
                        text.TotalPages();
                        text.Span($" • Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    public byte[] GenerateMechanicsComparisonPdf(MechanicsComparisonViewModel comparison)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // Poziomo dla lepszej czytelności tabeli
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .Text("Porównanie Mechaników")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        // Informacje podstawowe
                        col.Item().Text(text =>
                        {
                            text.Span("Okres: ").SemiBold();
                            text.Span($"{comparison.Period} ({comparison.StartDate:dd.MM.yyyy} - {comparison.EndDate:dd.MM.yyyy})");
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Liczba mechaników: ").SemiBold();
                            text.Span(comparison.Mechanics.Count.ToString());
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Tabela porównawcza
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30); // Pozycja
                                columns.RelativeColumn(3); // Imię i nazwisko
                                columns.RelativeColumn(2); // Wszystkie
                                columns.RelativeColumn(2); // Zakończone
                                columns.RelativeColumn(2); // Anulowane
                                columns.RelativeColumn(2); // W trakcie
                                columns.RelativeColumn(2); // Zarobki
                                columns.RelativeColumn(2); // % ukończenia
                                columns.RelativeColumn(2); // Śr. zarobki
                            });

                            // Nagłówki
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("#").SemiBold();
                                header.Cell().Element(HeaderStyle).Text("Mechanik").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Wszystkie").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Zakończone").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Anulowane").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("W trakcie").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Zarobki").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("% ukończ.").SemiBold();
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Śr./zlec.").SemiBold();
                            });

                            // Dane
                            int position = 1;
                            foreach (var mechanic in comparison.Mechanics)
                            {
                                var bgColor = position % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                var medalColor = position == 1 ? Colors.Yellow.Darken1 :
                                                position == 2 ? Colors.Grey.Medium :
                                                position == 3 ? Colors.Orange.Darken1 : Colors.White;

                                table.Cell().Element(CellStyle).Background(medalColor).AlignCenter()
                                    .Text(position.ToString()).Bold();
                                table.Cell().Element(CellStyle).Background(bgColor)
                                    .Text(mechanic.MechanicFullName);
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text(mechanic.TotalOrders.ToString());
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text(mechanic.CompletedOrders.ToString());
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text(mechanic.CancelledOrders.ToString());
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text(mechanic.InProgressOrders.ToString());
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text($"{mechanic.TotalEarnings:C}").FontColor(Colors.Green.Darken2);
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text($"{mechanic.CompletionRate:F1}%");
                                table.Cell().Element(CellStyle).Background(bgColor).AlignRight()
                                    .Text($"{mechanic.AverageEarningsPerOrder:C}");

                                position++;
                            }

                            // Podsumowanie
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4)
                                .Text("").Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4)
                                .Text("SUMA / ŚREDNIA").Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text(comparison.Mechanics.Sum(m => m.TotalOrders).ToString()).Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text(comparison.Mechanics.Sum(m => m.CompletedOrders).ToString()).Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text(comparison.Mechanics.Sum(m => m.CancelledOrders).ToString()).Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text(comparison.Mechanics.Sum(m => m.InProgressOrders).ToString()).Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text($"{comparison.Mechanics.Sum(m => m.TotalEarnings):C}").Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text($"{comparison.Mechanics.Average(m => m.CompletionRate):F1}%").Bold();
                            table.Cell().Element(CellStyle).Background(Colors.Blue.Lighten4).AlignRight()
                                .Text($"{comparison.Mechanics.Average(m => m.AverageEarningsPerOrder):C}").Bold();
                        });

                        // Najlepsi mechanicy
                        col.Item().PaddingTop(20).Text("🏆 TOP 3 Mechanicy według zarobków").SemiBold().FontSize(14);

                        var top3 = comparison.Mechanics.Take(3).ToList();
                        for (int i = 0; i < top3.Count; i++)
                        {
                            var m = top3[i];
                            col.Item().Text(text =>
                            {
                                text.Span($"{i + 1}. ").Bold();
                                text.Span($"{m.MechanicFullName}: ").SemiBold();
                                text.Span($"{m.TotalEarnings:C}").FontColor(Colors.Green.Darken2).Bold();
                                text.Span($" ({m.CompletedOrders} zakończonych zleceń)");
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Strona ");
                        text.CurrentPageNumber();
                        text.Span(" • Wygenerowano: ");
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateOrderReportPdf(OrderReportViewModel report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text($"RAPORT ZLECENIA #{report.OrderId}")
                            .FontSize(18).Bold();
                        col.Item().AlignCenter().Text($"Status: {GetStatusName(report.Status)}")
                            .FontSize(12);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(col =>
                    {
                        // Informacje podstawowe
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("INFORMACJE O ZLECENIU").Bold().FontSize(12);
                                c.Item().PaddingTop(5).Text($"Klient: {report.ClientFullName}");
                                c.Item().Text($"Pojazd: {report.RegistrationNumber}");
                                c.Item().Text($"Mechanik: {report.MechanicFullName}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DATY").Bold().FontSize(12);
                                c.Item().PaddingTop(5).Text($"Zgłoszenie: {report.SubmissionDate:dd.MM.yyyy HH:mm}");
                                if (report.StartDate.HasValue)
                                    c.Item().Text($"Rozpoczęcie: {report.StartDate.Value:dd.MM.yyyy HH:mm}");
                                if (report.EndDate.HasValue)
                                    c.Item().Text($"Zakończenie: {report.EndDate.Value:dd.MM.yyyy HH:mm}");
                                if (report.StartDate.HasValue && report.EndDate.HasValue)
                                {
                                    var duration = report.EndDate.Value - report.StartDate.Value;
                                    c.Item().Text($"Czas realizacji: {duration.Days} dni {duration.Hours} godz.").Bold();
                                }
                            });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Opis problemu
                        col.Item().PaddingVertical(10).Column(c =>
                        {
                            c.Item().Text("ZGŁOSZONY PROBLEM").Bold().FontSize(12);
                            c.Item().PaddingTop(5).Text(report.IssueDescription);
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Wycena początkowa
                        col.Item().PaddingVertical(10).Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("WYCENA POCZĄTKOWA").Bold().FontSize(12);
                            c.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text($"Netto: {report.EntryEstimatedCostNetto:N2} PLN");
                                r.RelativeItem().Text($"VAT ({report.VatRate}%): {(report.EntryEstimatedCostBrutto - report.EntryEstimatedCostNetto):N2} PLN");
                                r.RelativeItem().Text($"Brutto: {report.EntryEstimatedCostBrutto:N2} PLN").Bold();
                            });
                        });

                        // Wykonane naprawy
                        if (report.RepairTasks.Any())
                        {
                            col.Item().PaddingTop(10).Column(c =>
                            {
                                c.Item().Text($"WYKONANE NAPRAWY ({report.RepairTasks.Count})").Bold().FontSize(12);
                                c.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("#");
                                        header.Cell().Element(CellStyle).Text("Opis naprawy");
                                        header.Cell().Element(CellStyle).AlignRight().Text("Koszt");
                                        header.Cell().Element(CellStyle).AlignCenter().Text("Status");
                                    });

                                    int index = 1;
                                    foreach (var task in report.RepairTasks)
                                    {
                                        table.Cell().Element(CellStyle).Text(index.ToString());
                                        table.Cell().Element(CellStyle).Text(task.Description);
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{task.Cost:N2} PLN");
                                        table.Cell().Element(CellStyle).AlignCenter().Text(task.IsCompleted ? "Ukończone" : "W trakcie");
                                        index++;
                                    }

                                    table.Cell().ColumnSpan(2).Element(CellStyle).Background(Colors.Grey.Lighten3).Text("SUMA:").Bold();
                                    table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).AlignRight().Text($"{report.TotalNettoRepairTasks:N2} PLN").Bold();
                                    table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3);
                                });
                            });
                        }

                        // Koszty dodatkowe
                        if (report.AdditionalCosts.Any())
                        {
                            col.Item().PaddingTop(10).Column(c =>
                            {
                                c.Item().Text($"KOSZTY DODATKOWE ({report.AdditionalCosts.Count})").Bold().FontSize(12);
                                c.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("#");
                                        header.Cell().Element(CellStyle).Text("Opis");
                                        header.Cell().Element(CellStyle).AlignRight().Text("Koszt");
                                        header.Cell().Element(CellStyle).AlignCenter().Text("Data");
                                        header.Cell().Element(CellStyle).AlignCenter().Text("Status");
                                    });

                                    int index = 1;
                                    foreach (var cost in report.AdditionalCosts)
                                    {
                                        var bgColor = cost.IsAccepted ? Colors.White : Colors.Grey.Lighten3;
                                        table.Cell().Element(CellStyle).Background(bgColor).Text(index.ToString());
                                        table.Cell().Element(CellStyle).Background(bgColor).Text(cost.Description);
                                        table.Cell().Element(CellStyle).Background(bgColor).AlignRight().Text($"{cost.Cost:N2} PLN");
                                        table.Cell().Element(CellStyle).Background(bgColor).AlignCenter().Text($"{cost.AddedDate:dd.MM.yyyy}");
                                        table.Cell().Element(CellStyle).Background(bgColor).AlignCenter().Text(cost.IsAccepted ? "Zaakceptowane" : "Odrzucone");
                                        index++;
                                    }

                                    table.Cell().ColumnSpan(2).Element(CellStyle).Background(Colors.Grey.Lighten3).Text("SUMA ZAAKCEPTOWANYCH:").Bold();
                                    table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).AlignRight().Text($"{report.TotalNettoAdditionalCosts:N2} PLN").Bold();
                                    table.Cell().ColumnSpan(2).Element(CellStyle).Background(Colors.Grey.Lighten3);
                                });
                            });
                        }

                        // Podsumowanie finansowe
                        col.Item().PaddingTop(15).Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("PODSUMOWANIE FINANSOWE").Bold().FontSize(14);
                            c.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().Text("Wycena początkowa (netto):").FontSize(10);
                                table.Cell().AlignRight().Text($"{report.EntryEstimatedCostNetto:N2} PLN").FontSize(10);

                                if (report.RepairTasks.Any())
                                {
                                    table.Cell().Text("Wykonane naprawy (netto):").FontSize(10);
                                    table.Cell().AlignRight().Text($"+ {report.TotalNettoRepairTasks:N2} PLN").FontSize(10);
                                }

                                if (report.AdditionalCosts.Any(ac => ac.IsAccepted))
                                {
                                    table.Cell().Text("Koszty dodatkowe zaakceptowane (netto):").FontSize(10);
                                    table.Cell().AlignRight().Text($"+ {report.TotalNettoAdditionalCosts:N2} PLN").FontSize(10);
                                }

                                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Suma netto:").Bold().FontSize(11);
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"{report.TotalNetto:N2} PLN").Bold().FontSize(11);

                                table.Cell().Text($"VAT ({report.VatRate}%):").FontSize(10);
                                table.Cell().AlignRight().Text($"{report.TotalVat:N2} PLN").FontSize(10);

                                table.Cell().Background(Colors.Green.Darken1).Padding(5).Text("RAZEM DO ZAPŁATY (brutto):").Bold().FontSize(12).FontColor(Colors.White);
                                table.Cell().Background(Colors.Green.Darken1).Padding(5).AlignRight().Text($"{report.TotalBrutto:N2} PLN").Bold().FontSize(14).FontColor(Colors.White);
                            });
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Strona ");
                        text.CurrentPageNumber();
                        text.Span(" • Wygenerowano: ");
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
            });
        });

        return document.GeneratePdf();
    }

    private string GetStatusName(Model.DataModels.RepairOrderStatusValue status)
    {
        return status switch
        {
            Model.DataModels.RepairOrderStatusValue.Created => "Utworzone",
            Model.DataModels.RepairOrderStatusValue.PendingApproval => "Do akceptacji",
            Model.DataModels.RepairOrderStatusValue.Approved => "Zaakceptowane",
            Model.DataModels.RepairOrderStatusValue.InProgress => "W realizacji",
            Model.DataModels.RepairOrderStatusValue.ReadyForPickup => "Gotowe do odbioru",
            Model.DataModels.RepairOrderStatusValue.Completed => "Zakończone",
            Model.DataModels.RepairOrderStatusValue.Cancelled => "Anulowane",
            _ => status.ToString()
        };
    }

    private static IContainer HeaderStyle(IContainer container)
    {
        return container
            .Border(0.5f)
            .BorderColor(Colors.Grey.Darken1)
            .Background(Colors.Grey.Lighten2)
            .Padding(5);
    }
}
