using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    public class HolidayController : ApiControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidays([FromQuery] int? year, [FromQuery] int? month, [FromQuery] bool activeOnly = true)
        {
            return Ok(await _holidayService.GetHolidaysAsync(year, month, activeOnly));
        }

        [HttpPost]
        public async Task<IActionResult> CreateHoliday([FromBody] HolidayRequest request)
        {
            var holiday = await _holidayService.CreateHolidayAsync(request);
            return CreatedAtAction(nameof(GetHolidays), new { id = holiday.Id }, holiday);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHoliday(int id, [FromBody] HolidayRequest request)
        {
            var holiday = await _holidayService.UpdateHolidayAsync(id, request);
            return holiday == null ? NotFound("Holiday not found.") : Ok(holiday);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var deleted = await _holidayService.DeleteHolidayAsync(id);
            return deleted ? NoContent() : NotFound("Holiday not found.");
        }
    }
}
