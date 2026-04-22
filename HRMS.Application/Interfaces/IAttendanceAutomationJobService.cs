namespace HRMS.Application.Interfaces
{
    public interface IAttendanceAutomationJobService
    {
        Task MarkAbsentEmployeesAsync();
    }
}
