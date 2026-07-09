namespace HRMS.Domain.Entities
{
    public class Holiday
    {
        public int Id { get; set; }

        public DateTime HolidayDate { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
