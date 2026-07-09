using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Infrastructure.Services
{
    public class PayslipService : IPayslipService
    {
        private readonly HrmsDbContext _context;

        public PayslipService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GeneratePayslipPdfAsync(int employeeId, int year, int month)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }

            var salary = await _context.SalaryStructures
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

            if (salary == null)
            {
                throw new InvalidOperationException("Salary structure not configured for this employee.");
            }

            var presentDays = await _context.Attendances
                .CountAsync(a => a.EmployeeId == employeeId
                    && a.AttendanceDate.Year == year
                    && a.AttendanceDate.Month == month
                    && a.Status == AttendanceStatuses.Present
                    && a.CheckInTime.HasValue);

            var totalDays = DateTime.DaysInMonth(year, month);
            var ratio = totalDays == 0 ? 0m : presentDays / (decimal)totalDays;

            var baseEarned = Math.Round(salary.Base * ratio, 2);
            var hraEarned = Math.Round(salary.HRA * ratio, 2);
            var daEarned = Math.Round(salary.DA * ratio, 2);
            var pfDeduction = Math.Round(salary.PFDeductions * ratio, 2);
            var taxDeduction = Math.Round(salary.Tax * ratio, 2);

            var gross = baseEarned + hraEarned + daEarned;
            var totalDeductions = pfDeduction + taxDeduction;
            var netSalary = gross - totalDeductions;

            var period = new DateTime(year, month, 1).ToString("MMMM yyyy");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Background("#0F172A").Padding(12).Column(inner =>
                        {
                            inner.Item().Text("HRMS PAYSLIP").FontColor(Colors.White).SemiBold().FontSize(20);
                            inner.Item().Text($"Salary Month: {period}").FontColor("#E2E8F0").FontSize(11);
                        });
                    });

                    page.Content().PaddingVertical(12).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Employee: {employee.Name}").SemiBold();
                        col.Item().Text($"Employee ID: {employee.Id}");
                        col.Item().Text($"Department: {employee.Department?.Name ?? "N/A"}");
                        col.Item().Text($"Present Days: {presentDays} / {totalDays}");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Earnings").SemiBold().FontSize(12);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });

                            t.Cell().Element(CellStyle).Text("Base");
                            t.Cell().Element(CellStyle).AlignRight().Text(baseEarned.ToString("0.00"));

                            t.Cell().Element(CellStyle).Text("HRA");
                            t.Cell().Element(CellStyle).AlignRight().Text(hraEarned.ToString("0.00"));

                            t.Cell().Element(CellStyle).Text("DA");
                            t.Cell().Element(CellStyle).AlignRight().Text(daEarned.ToString("0.00"));

                            t.Cell().Element(CellStyle).Text(x => x.Span("Gross Salary").SemiBold());
                            t.Cell().Element(CellStyle).AlignRight().Text(x => x.Span(gross.ToString("0.00")).SemiBold());
                        });

                        col.Item().Text("Deductions").SemiBold().FontSize(12);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });

                            t.Cell().Element(CellStyle).Text("PF");
                            t.Cell().Element(CellStyle).AlignRight().Text(pfDeduction.ToString("0.00"));

                            t.Cell().Element(CellStyle).Text("Tax");
                            t.Cell().Element(CellStyle).AlignRight().Text(taxDeduction.ToString("0.00"));

                            t.Cell().Element(CellStyle).Text(x => x.Span("Total Deductions").SemiBold());
                            t.Cell().Element(CellStyle).AlignRight().Text(x => x.Span(totalDeductions.ToString("0.00")).SemiBold());
                        });

                        col.Item().Background("#DCFCE7").Padding(8).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });

                            t.Cell().Text("Net Salary").SemiBold().FontSize(13);
                            t.Cell().AlignRight().Text(netSalary.ToString("0.00")).SemiBold().FontSize(13);
                        });
                    });

                    page.Footer().AlignCenter().Text("This is a system generated payslip.").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
        }
    }
}
