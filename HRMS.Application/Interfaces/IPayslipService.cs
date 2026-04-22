namespace HRMS.Application.Interfaces
{
    public interface IPayslipService
    {
        Task<byte[]> GeneratePayslipPdfAsync(int employeeId, int year, int month);
    }
}
