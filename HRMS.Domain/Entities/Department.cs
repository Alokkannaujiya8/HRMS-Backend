namespace HRMS.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
