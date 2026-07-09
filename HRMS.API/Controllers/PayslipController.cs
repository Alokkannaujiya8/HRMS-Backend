using HRMS.API.Authorization;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize]
    public class PayslipController : ApiControllerBase
    {
        private readonly IPayslipService _payslipService;

        public PayslipController(IPayslipService payslipService)
        {
            _payslipService = payslipService;
        }

        [HttpGet("{employeeId:int}")]
        [HasPermission("CanViewSalary")]
        public async Task<IActionResult> DownloadPayslip(int employeeId, [FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2000 || month is < 1 or > 12)
            {
                return BadRequest("Invalid year or month.");
            }

            try
            {
                var pdfBytes = await _payslipService.GeneratePayslipPdfAsync(employeeId, year, month);
                return File(pdfBytes, "application/pdf", $"payslip-{employeeId}-{year}-{month:D2}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
