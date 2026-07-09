namespace HRMS.Application.DTOs
{
    public class HolidayRequest
    {
        public DateTime HolidayDate { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
