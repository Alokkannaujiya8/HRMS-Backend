using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces
{
    /// <summary>
    /// Contract for managing transactional boundaries and repository access.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<AppUser> Users { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        IGenericRepository<PasswordHistory> PasswordHistories { get; }
        IGenericRepository<Attendance> Attendances { get; }
        IGenericRepository<LeaveRequest> LeaveRequests { get; }
        IGenericRepository<LeaveBalance> LeaveBalances { get; }
        IGenericRepository<Holiday> Holidays { get; }
        IGenericRepository<Payroll> Payrolls { get; }
        IGenericRepository<SalaryStructure> SalaryStructures { get; }
        IGenericRepository<SalaryManagement> SalaryManagements { get; }
        IGenericRepository<Asset> Assets { get; }
        IGenericRepository<AssetAssignment> AssetAssignments { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }
        IGenericRepository<AuditTrail> AuditTrails { get; }
        IGenericRepository<EmployeeDocument> EmployeeDocuments { get; }
        IGenericRepository<EmployeeRemark> EmployeeRemarks { get; }
        IGenericRepository<EmployeeSkill> EmployeeSkills { get; }
        IGenericRepository<EmployeeEducation> EmployeeEducations { get; }
        IGenericRepository<EmployeeExperience> EmployeeExperiences { get; }
        IGenericRepository<EmergencyContact> EmergencyContacts { get; }
        IGenericRepository<FamilyDetail> FamilyDetails { get; }
        IGenericRepository<PromotionHistory> PromotionHistories { get; }

        /// <summary>
        /// Retrieves a generic repository for a specified entity type.
        /// </summary>
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        /// <summary>
        /// Saves all changes made in this unit of work context to the database asynchronously.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a database transaction asynchronously.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the active transaction asynchronously.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the active transaction asynchronously.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
