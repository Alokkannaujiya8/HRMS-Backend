using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IHolidayService
    {
        Task<List<Holiday>> GetHolidaysAsync(int? year, int? month, bool activeOnly);

        Task<Holiday> CreateHolidayAsync(HolidayRequest request);

        Task<Holiday?> UpdateHolidayAsync(int id, HolidayRequest request);

        Task<bool> DeleteHolidayAsync(int id);
    }
}
