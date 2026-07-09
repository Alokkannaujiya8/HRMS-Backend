using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IEmployeeDetailsService
    {
        Task<EmployeeDetailsResponse> GetFullProfileAsync(int employeeId);
        Task AddSkillAsync(int employeeId, AddSkillRequest request);
        Task AddRemarkAsync(int employeeId, AddRemarkRequest request, string addedBy);
        Task UploadDocumentAsync(int employeeId, string documentType, string fileName, Stream fileStream);
        Task VerifyAadhaarAsync(int employeeId, VerifyAadhaarRequest request);
        Task<ReportFileResponse> GenerateAppointmentLetterAsync(int employeeId);
        Task<ReportFileResponse> GenerateAppraisalLetterAsync(int employeeId);
    }
}
