using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly HrmsDbContext _context;

        public HolidayService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Holiday>> GetHolidaysAsync(int? year, int? month, bool activeOnly)
        {
            var query = _context.Holidays.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            if (year.HasValue)
            {
                query = query.Where(x => x.HolidayDate.Year == year.Value);
            }

            if (month.HasValue)
            {
                query = query.Where(x => x.HolidayDate.Month == month.Value);
            }

            return await query.OrderBy(x => x.HolidayDate).ToListAsync();
        }

        public async Task<Holiday> CreateHolidayAsync(HolidayRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Holiday name is required.");
            }

            var date = request.HolidayDate.Date;
            var exists = await _context.Holidays.AnyAsync(x => x.HolidayDate == date);
            if (exists)
            {
                throw new InvalidOperationException("Holiday already exists for this date.");
            }

            var holiday = new Holiday
            {
                HolidayDate = date,
                Name = request.Name.Trim(),
                Description = request.Description,
                IsActive = request.IsActive
            };

            await _context.Holidays.AddAsync(holiday);
            await _context.SaveChangesAsync();

            return holiday;
        }

        public async Task<Holiday?> UpdateHolidayAsync(int id, HolidayRequest request)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
            if (holiday == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Holiday name is required.");
            }

            var date = request.HolidayDate.Date;
            var duplicate = await _context.Holidays.AnyAsync(x => x.Id != id && x.HolidayDate == date);
            if (duplicate)
            {
                throw new InvalidOperationException("Holiday already exists for this date.");
            }

            holiday.HolidayDate = date;
            holiday.Name = request.Name.Trim();
            holiday.Description = request.Description;
            holiday.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return holiday;
        }

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
            if (holiday == null)
            {
                return false;
            }

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
