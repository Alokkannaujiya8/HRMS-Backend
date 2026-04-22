namespace HRMS.Domain.Entities
{
    public class EmployeeRemark
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string Remark { get; set; } = string.Empty;

        public string AddedBy { get; set; } = "System";

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
