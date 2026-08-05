using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace HRMS.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of Unit of Work managing entity repositories and transactions for HrmsDbContext.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HrmsDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(HrmsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Employees = new GenericRepository<Employee>(_context);
            Users = new GenericRepository<AppUser>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
            PasswordHistories = new GenericRepository<PasswordHistory>(_context);
            Attendances = new GenericRepository<Attendance>(_context);
            LeaveRequests = new GenericRepository<LeaveRequest>(_context);
            LeaveBalances = new GenericRepository<LeaveBalance>(_context);
            Holidays = new GenericRepository<Holiday>(_context);
            Payrolls = new GenericRepository<Payroll>(_context);
            SalaryStructures = new GenericRepository<SalaryStructure>(_context);
            SalaryManagements = new GenericRepository<SalaryManagement>(_context);
            Assets = new GenericRepository<Asset>(_context);
            AssetAssignments = new GenericRepository<AssetAssignment>(_context);
            Departments = new GenericRepository<Department>(_context);
            AuditLogs = new GenericRepository<AuditLog>(_context);
            AuditTrails = new GenericRepository<AuditTrail>(_context);
            EmployeeDocuments = new GenericRepository<EmployeeDocument>(_context);
            EmployeeRemarks = new GenericRepository<EmployeeRemark>(_context);
            EmployeeSkills = new GenericRepository<EmployeeSkill>(_context);
            EmployeeEducations = new GenericRepository<EmployeeEducation>(_context);
            EmployeeExperiences = new GenericRepository<EmployeeExperience>(_context);
            EmergencyContacts = new GenericRepository<EmergencyContact>(_context);
            FamilyDetails = new GenericRepository<FamilyDetail>(_context);
            PromotionHistories = new GenericRepository<PromotionHistory>(_context);
        }

        public IGenericRepository<Employee> Employees { get; }
        public IGenericRepository<AppUser> Users { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }
        public IGenericRepository<PasswordHistory> PasswordHistories { get; }
        public IGenericRepository<Attendance> Attendances { get; }
        public IGenericRepository<LeaveRequest> LeaveRequests { get; }
        public IGenericRepository<LeaveBalance> LeaveBalances { get; }
        public IGenericRepository<Holiday> Holidays { get; }
        public IGenericRepository<Payroll> Payrolls { get; }
        public IGenericRepository<SalaryStructure> SalaryStructures { get; }
        public IGenericRepository<SalaryManagement> SalaryManagements { get; }
        public IGenericRepository<Asset> Assets { get; }
        public IGenericRepository<AssetAssignment> AssetAssignments { get; }
        public IGenericRepository<Department> Departments { get; }
        public IGenericRepository<AuditLog> AuditLogs { get; }
        public IGenericRepository<AuditTrail> AuditTrails { get; }
        public IGenericRepository<EmployeeDocument> EmployeeDocuments { get; }
        public IGenericRepository<EmployeeRemark> EmployeeRemarks { get; }
        public IGenericRepository<EmployeeSkill> EmployeeSkills { get; }
        public IGenericRepository<EmployeeEducation> EmployeeEducations { get; }
        public IGenericRepository<EmployeeExperience> EmployeeExperiences { get; }
        public IGenericRepository<EmergencyContact> EmergencyContacts { get; }
        public IGenericRepository<FamilyDetail> FamilyDetails { get; }
        public IGenericRepository<PromotionHistory> PromotionHistories { get; }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            return (IGenericRepository<TEntity>)_repositories.GetOrAdd(
                typeof(TEntity),
                _ => new GenericRepository<TEntity>(_context));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
