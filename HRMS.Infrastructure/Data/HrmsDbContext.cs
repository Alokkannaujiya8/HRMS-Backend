using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace HRMS.Infrastructure.Data
{
    public class HrmsDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public HrmsDbContext(DbContextOptions<HrmsDbContext> options)
            : base(options)
        {
        }

        public HrmsDbContext(
            DbContextOptions<HrmsDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<EmployeeSkill> EmployeeSkills { get; set; }
        public DbSet<EmployeeRemark> EmployeeRemarks { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<SalaryManagement> SalaryManagements { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payroll>()
                .Property(e => e.BasicSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payroll>()
                .Property(e => e.Bonus)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payroll>()
                .Property(e => e.Deduction)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payroll>()
                .Property(e => e.NetSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.EmployeeId, a.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<LeaveBalance>()
                .HasIndex(lb => new { lb.EmployeeId, lb.LeaveType })
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasOne<Employee>()
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalaryStructure>()
                .HasIndex(ss => ss.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<SalaryStructure>()
                .Property(ss => ss.Base)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryStructure>()
                .Property(ss => ss.HRA)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryStructure>()
                .Property(ss => ss.DA)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryStructure>()
                .Property(ss => ss.PFDeductions)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryStructure>()
                .Property(ss => ss.Tax)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeSkill>()
                .HasOne(es => es.Employee)
                .WithMany(e => e.Skills)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeRemark>()
                .HasOne(er => er.Employee)
                .WithMany(e => e.Remarks)
                .HasForeignKey(er => er.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeDocument>()
                .HasOne(ed => ed.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalaryManagement>()
                .HasIndex(sm => sm.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.BasicSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.IncrementPercent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.IncrementAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.SpecialAllowance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.SpecialDeduction)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalaryManagement>()
                .Property(sm => sm.TotalSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Holiday>()
                .HasIndex(h => h.HolidayDate)
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = PrepareAuditEntries();
            var result = await base.SaveChangesAsync(cancellationToken);

            if (auditEntries.Count > 0)
            {
                await AuditTrails.AddRangeAsync(auditEntries, cancellationToken);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private List<AuditTrail> PrepareAuditEntries()
        {
            ChangeTracker.DetectChanges();

            var changedBy = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var changedAt = DateTime.UtcNow;
            var auditLogs = new List<AuditTrail>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditTrail || entry.State is EntityState.Detached or EntityState.Unchanged)
                {
                    continue;
                }

                var action = entry.State switch
                {
                    EntityState.Added => "Added",
                    EntityState.Modified => "Modified",
                    EntityState.Deleted => "Deleted",
                    _ => "Unknown"
                };

                var keyProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
                var recordId = keyProperty != null ? entry.Property(keyProperty).CurrentValue?.ToString() : null;

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                if (entry.State == EntityState.Added)
                {
                    foreach (var property in entry.Properties)
                    {
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    foreach (var property in entry.Properties)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties)
                    {
                        if (!property.IsModified)
                        {
                            continue;
                        }

                        var original = property.OriginalValue;
                        var current = property.CurrentValue;

                        if (Equals(original, current))
                        {
                            continue;
                        }

                        oldValues[property.Metadata.Name] = original;
                        newValues[property.Metadata.Name] = current;
                    }
                }

                if (entry.State == EntityState.Modified && oldValues.Count == 0 && newValues.Count == 0)
                {
                    continue;
                }

                auditLogs.Add(new AuditTrail
                {
                    TableName = entry.Metadata.ClrType.Name,
                    ActionType = action,
                    RecordId = recordId,
                    ChangedBy = changedBy,
                    ChangedAt = changedAt,
                    OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                    NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues)
                });
            }

            return auditLogs;
        }
    }
}
