namespace HRMS.Domain.Entities
{
    public class EmployeeSkill
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string? Proficiency { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
