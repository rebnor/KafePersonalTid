using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using KafePersonalTid.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KafePersonalTid.Services
{
    public class PdfService
    {
        public byte[] GenerateEmployeeReportPdf(Employee employee, int year, int? month)
        {
            var culture = new CultureInfo("sv-SE");
            var monthName = (month.HasValue && month.Value >= 1 && month.Value <= 12)
                ? culture.DateTimeFormat.GetMonthName(month.Value)
                : "Hela året";
            var periodLabel = month.HasValue ? $"{monthName} {year}" : $"År {year}";

            var filteredEntries = employee.WorkEntries
                .Where(e => e.Date.Year == year && (!month.HasValue || e.Date.Month == month))
                .OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToList();

            var totalHours = TimeSpan.FromHours(filteredEntries.Sum(e => e.HoursWorked));
            var totalHoursDouble = totalHours.TotalHours;

            decimal hourlyRate = employee.HourlyWage;
            decimal totalSalary = hourlyRate * (decimal)totalHoursDouble;
            decimal moms = totalSalary * 0.25m;
            decimal skatt = totalSalary * 0.30m;
            decimal utbetalning = totalSalary - skatt;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"Lönespecifikation – {employee.Name}").Bold().FontSize(18).AlignCenter();
                        col.Item().Text($"Period: {periodLabel}").Italic().AlignCenter();
                        col.Item().PaddingBottom(15);
                    });

                    page.Content().Column(col =>
                    {
                        int? currentMonth = null;

                        // Tabell för arbetspass
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Datum").Bold();
                                header.Cell().Element(CellStyle).Text("Starttid").Bold();
                                header.Cell().Element(CellStyle).Text("Sluttid").Bold();
                                header.Cell().Element(CellStyle).Text("Tid").Bold();
                                header.Cell().Element(CellStyle).Text("Lön").Bold();
                            });

                            foreach (var entry in filteredEntries)
                            {
                                if (!month.HasValue && (currentMonth != entry.Date.Month))
                                {
                                    currentMonth = entry.Date.Month;
                                    var headerText = culture.DateTimeFormat.GetMonthName(currentMonth.Value);
                                    table.Cell().ColumnSpan(5).Element(CellStyle).Text($"{headerText} {entry.Date.Year}").Bold().FontSize(13);
                                }

                                var start = entry.StartTime ?? TimeSpan.Zero;
                                var end = start.Add(TimeSpan.FromHours(entry.HoursWorked));
                                var salary = (decimal)entry.HoursWorked * hourlyRate;

                                table.Cell().Element(CellStyle).Text(entry.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(start.ToString(@"hh\:mm"));
                                table.Cell().Element(CellStyle).Text(end.ToString(@"hh\:mm"));
                                table.Cell().Element(CellStyle).Text(FormatDuration(TimeSpan.FromHours(entry.HoursWorked)));
                                table.Cell().Element(CellStyle).Text($"{salary:N2} kr");
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.PaddingVertical(4).PaddingHorizontal(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        });

                        // Lönesammanställning
                        col.Item().PaddingTop(20).Text($"Timlön: {hourlyRate:N2} kr").Bold();
                        col.Item().Text($"Totalt antal timmar: {FormatDuration(totalHours)}");
                        col.Item().Text($"Bruttolön: {totalSalary:N2} kr");
                        col.Item().Text($"Moms (25%): {moms:N2} kr");
                        col.Item().Text($"Skatt (30%): {skatt:N2} kr");
                        col.Item().Text($"Utbetalning (nettolön): {utbetalning:N2} kr").Bold();
                        col.Item().PaddingBottom(10).LineHorizontal(1);

                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Genererad ").Italic();
                        txt.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                    });
                });
            });

            return document.GeneratePdf();
        }


        private static string FormatDuration(TimeSpan duration)
        {
            int hours = (int)duration.TotalHours;
            int minutes = duration.Minutes;
            return $"{hours} tim {minutes} min";
        }
    }
}
