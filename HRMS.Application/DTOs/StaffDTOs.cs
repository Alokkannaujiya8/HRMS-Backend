namespace HRMS.Application.DTOs
{
    public class StaffListQueryRequest
    {
        public string? Search { get; set; }
        public string? Division { get; set; }
        public string? Designation { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class StaffListItemResponse
    {
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Division { get; set; }
        public string? Designation { get; set; }
        public decimal Salary { get; set; }
    }

    public class StaffListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyCollection<StaffListItemResponse> Items { get; set; } = [];
    }

    public class EditStaffProfileRequest
    {
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Designation { get; set; }
        public string? Division { get; set; }
        public string? Pan { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsSeventhPayCommission { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public int DepartmentId { get; set; }
    }

    public class StaffOnboardRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? Address { get; set; }
        public string? Designation { get; set; }
        public string? Division { get; set; }
        public string? Pan { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsSeventhPayCommission { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IfscCode { get; set; }
    }

    public class EmployeeDetailsResponse
    {
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public DateTime JoinDate { get; set; }
        public string? Designation { get; set; }
        public string? Division { get; set; }
        public string? DepartmentName { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? EmploymentType { get; set; }
        public string? Pan { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public bool IsSeventhPayCommission { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public bool IsAadhaarVerified { get; set; }
        public IReadOnlyCollection<EmployeeSkillResponse> Skills { get; set; } = [];
        public IReadOnlyCollection<EmployeeRemarkResponse> Remarks { get; set; } = [];
        public IReadOnlyCollection<EmployeeDocumentResponse> Documents { get; set; } = [];
        public SalarySummaryResponse? SalaryManagement { get; set; }
    }

    public class EmployeeSkillResponse
    {
        public int Id { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string? Proficiency { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class EmployeeRemarkResponse
    {
        public int Id { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string AddedBy { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }

    public class EmployeeDocumentResponse
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class SalarySummaryResponse
    {
        public decimal BasicSalary { get; set; }
        public decimal IncrementPercent { get; set; }
        public decimal IncrementAmount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal SpecialDeduction { get; set; }
        public decimal TotalSalary { get; set; }
    }

    public class MasterDataResponse
    {
        public IReadOnlyCollection<string> Divisions { get; set; } = [];
        public IReadOnlyCollection<string> Designations { get; set; } = [];
        public IReadOnlyCollection<DepartmentMasterItem> Departments { get; set; } = [];
    }

    public class DepartmentMasterItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class AddSkillRequest
    {
        public string SkillName { get; set; } = string.Empty;
        public string? Proficiency { get; set; }
    }

    public class AddRemarkRequest
    {
        public string Remark { get; set; } = string.Empty;
    }

    public class VerifyAadhaarRequest
    {
        public string AadhaarNumber { get; set; } = string.Empty;
    }

    public class SalaryManagementRequest
    {
        public int EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal IncrementPercent { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal SpecialDeduction { get; set; }
    }
}
